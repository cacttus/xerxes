using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Proteus;
namespace Spartan
{
    public class AgentServer : IBroCompilerObject
    {
        Object _objLockObject = new Object();
        public System.Net.Sockets.TcpListener Listener { get; set; }
        public List<BroCompilerThread> CompilerThreads = new List<BroCompilerThread>();
        public List<AgentCoordinator> ClientConnections = new List<AgentCoordinator>();
        public int ReservationIds { get; set; }
        private List<AgentCoordinator> _objDroppedCoordinators = new List<AgentCoordinator>();
        public List<BroSourceFile> CompiledFiles = new List<BroSourceFile>();
        private int _intCoordId = 1;
        private DateTime _datLastCommandTime = DateTime.MinValue;
        private List<AgentCommand> _objCommandQueue = new List<AgentCommand>();

        public bool EnableExtendedLogging = false;

        public int GetUsableProcessors()
        {
            return System.Environment.ProcessorCount;
        }

        // - CTOR
        public AgentServer()
            : base("A ["+System.Environment.MachineName+"] ")
        {
            //0 is reserved
            ReservationIds = 1;
        }
        public override void Run()
        {
            CreateCompilerThreads();
            ListenForData();
        }
        private void CreateCompilerThreads()
        {
            LogInfo("Creating threads.");
            for (int n = 0; n < GetUsableProcessors(); n++)
            {
                BroCompilerThread th = new BroCompilerThread(n);
                CompilerThreads.Add(th);
                th.Run();
            }
        }
        public void ListenForData()
        {
            System.Threading.EventWaitHandle wh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

            LogInfo("Listening for connections and junk.");
            ListenForConnections();

            LogInfo("Setup complete.. waiting for build....");
            while (true)
            {
                try
                {
                    AcceptConnections();
                    UpdateCoordinators();
                    UpdateThreads();
                    DistributeClientCommands();
                    RemoveDroppedCoordinators();
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    LogInfo(" [Sockets] Error: " + e.ToString());
                }


                //Debug so we can see the cmd
                System.Windows.Forms.Application.DoEvents();

                //If we are idle.. then sleep this guy
                if ((DateTime.Now - _datLastCommandTime).TotalSeconds > SpartanGlobals.GlobalAgentIdleWaitInSeconds)
                {
                    System.Threading.Thread.Sleep(SpartanGlobals.GlobalAgentIdleTimeInMilliseconds);
                    Console.WriteLine("Agent going to sleep.." + System.Environment.TickCount);
                }
                else if (BuildConfig.BuildInterval > 0)
                    System.Threading.Thread.Sleep(BuildConfig.BuildInterval);
            }
        }
        private void UpdateCoordinators()
        {
            // Update all clients.
            foreach (AgentCoordinator ac in ClientConnections)
            {
                if (ac.IsConnected() == false)
                    DropCoordinator(ac);

                try
                {
                    ProcessData(ac);
                }
                catch(System.Net.Sockets.SocketException ex)
                {
                    if (ex.SocketErrorCode == System.Net.Sockets.SocketError.InvalidArgument)// (0x80004005)
                    {
                        // ** coordinator might have failed receive.  Drop the coordinator
                        //Coordinator Dropped.
                        //Globals.Logger.LogInfo("Coordinator dropped.
                    }
                    else
                    {
                        Globals.Logger.LogWarn("Got socket error code: "+ex.SocketErrorCode.ToString());
                    }
                    DropCoordinator(ac);
                }
                catch(Exception ex)
                {
                    Globals.Logger.LogError(ex.ToString());
                }
            }
        }
        private void AcceptConnections()
        {
            if (Listener.Pending())
            {
                if (EnableExtendedLogging==true)
                    LogInfo("Accepted new Coordinator Connection.");
                AgentCoordinator st = new AgentCoordinator(_intCoordId++);
                st.Socket = Listener.AcceptSocket();
                ClientConnections.Add(st);
            }
        }
        private void ProcessData(AgentCoordinator s)
        {
            //rcv
            string ret = s.Recv();

            if (string.IsNullOrEmpty(ret) == false)
            {
                ParseReceivedData(s, ret);
            }
        }
        private void SetActive()
        {
            _datLastCommandTime = DateTime.Now;
        }
        private void ListenForConnections()
        {
            if (Listener != null)
                Listener = null;

            Listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, SpartanGlobals.ClientRecvPort);
            Listener.Start();

            Console.WriteLine("Client is listening on port:" + SpartanGlobals.ClientRecvPort.ToString());
        }
        private void ParseReceivedData(AgentCoordinator sc, string rawData)
        {
            string sndBuf = String.Empty;
            string rcvBuf = rawData;
            PacketType pt = SpartanGlobals.GetPacketType(ref rcvBuf);
            switch (pt)
            {
                   
                case PacketType.RestartServer:
                    //Hack. The processes screw up sometimes I DONT KNOW WHY. So this will 
                    // effectively restart the windows service fresh.
                    LogInfo("Restart received.  Killing Agent and waiting for service reboot.");
                    Environment.Exit(1);
                    break;
                case PacketType.UnreserveProcessor:
                    SetActive();
                    int reserveId = NetworkUtils.UnpackInt(ref rcvBuf);
                    UnreserveProcessor(reserveId);
                    break;
                case PacketType.AgentStatusInfo:

                    // Client wants to know how many processors we have
                    string coordName = NetworkUtils.UnpackString(ref rcvBuf);
                    int procs = GetUsableProcessors();

                    AgentInfo inf = GetAgentInfo();

                    sndBuf = SpartanGlobals.PacketTypeToMsgHeader(PacketType.AgentStatusInfo)
                        + NetworkUtils.PackString(coordName)
                        + inf.Serialize()
                    ;

                    //LogInfo("Sending status info" + sndBuf);
                    if (EnableExtendedLogging==true)
                        LogInfo("Proc query, " + procs + " procs, sending to " + coordName);
                    sc.Send(sndBuf);

                    break;
                case PacketType.AgentStateQueryAvailable:
                    //Client has asked for us to reserve a processor for a command
                    //
                    SetActive();
                    int requestId = NetworkUtils.UnpackInt(ref rcvBuf);

                    //Make a reservation with a queue id.
                    int reserveId2 = ReserveProcessor();

                    // ** IF IT IS ZERO THEN THERE IS NO FREE PROCESSOR
                    if (reserveId2 != 0)
                        LogInfo("    Reserving " + reserveId2);

                    sndBuf =
                        SpartanGlobals.PacketTypeToMsgHeader(PacketType.AgentStateQueryAvailable)
                        + NetworkUtils.PackInt(requestId)
                        + NetworkUtils.PackInt(reserveId2)
                        ;

                    sc.Send(sndBuf);
                    break;
                case PacketType.OkToCompileFile:
                    // Client has gotten our processor reservation and sent us a 
                    // command in return.
                    SetActive();
                    AgentCommand ac = SpartanGlobals.ReadCommandFromStream(ref rcvBuf);
                    ac.StartTime = DateTime.Now;

                    //Must set the invoking coordinator so we can send back safely.
                    ac.InvokingCoordinatorId = sc.CoordinatorId;
                    
                    LogInfo("Queing reservation " + ac.ClientReservationId);
                    _objCommandQueue.Add(ac);
                    
                    //tell u of the world i see 44 22 1
                
                    //Send back to client
                    sndBuf = SpartanGlobals.PacketTypeToMsgHeader(PacketType.OkToCompileFile);
                    sndBuf += NetworkUtils.PackInt(ac.ClientReservationId);

                    sc.Send(sndBuf);
                    break;
                case PacketType.QueryRequestStatus:
                    //Client wnts to know where his crap file is dammit
                    SetActive();
                    int rq = NetworkUtils.UnpackInt(ref rcvBuf); //request id
                    string cn = NetworkUtils.UnpackString(ref rcvBuf);   //computer name

                    //OK i assume we are sending the reservation id NOT the request ID
                    CompileRequestStatus stat = QueryRequestStatus(rq);

                    if (stat != CompileRequestStatus.Compiling)
                    {
                        int n = 0;
                        n++;
                    }
                    //OK i assume we are sending the reservation id NOT the request ID
                    sndBuf =
                        SpartanGlobals.PacketTypeToMsgHeader(PacketType.QueryRequestStatus)
                        + NetworkUtils.PackInt((int)stat)
                        + NetworkUtils.PackInt(rq)
                        + NetworkUtils.PackString(cn)
                        ;

                    LogWarn("Got Request Status rq=" + rq + " stat=" + stat.ToString());

                    sc.Send(sndBuf);
                    break;
                case PacketType.Error:
                    LogError("Error Invalid packet Header: Buf Data: " + rcvBuf);

                    sndBuf = SpartanGlobals.PacketTypeToMsgHeader(PacketType.Error);
                    sc.Send(sndBuf);
                    break;
                //case PacketType.ResetGlobalCompileState:
                //    ResetBuild();
                //    break;
                case PacketType.StopCompilation:
                    LogWarn("Received Request To Stop Compilation from coordinator " + sc.CoordinatorId);
                    StopEverythingForCoordinator(sc);
                    break;
                default:
                    LogError("Error Invalid packet Header: Buf Data: " + rcvBuf);

                    sndBuf = SpartanGlobals.PacketTypeToMsgHeader(PacketType.Error);
                    sc.Send(sndBuf);
                    break;
            }
        }
        private void DistributeClientCommands()
        {
            List<AgentCommand> toRemove = new List<AgentCommand>();

            // LOCAL COMMANDS COME FIRST
            //reserve processors for -1 commands.
            //foreach (AgentCommand cmd in _objCommandQueue)
            //{

            //}
            
            //**Doesnt makes sense we should be doing this on the coordinator not ont he agent.

            // THEN CLIENT COMMANDS
            foreach (AgentCommand ac in _objCommandQueue)
            {
             //   if (ac.ClientReservationId != -1)
           //     {
                    SendCommandToReservedProcessor(ac);
                    toRemove.Add(ac);
          //      }
            }
            foreach(AgentCommand ac in toRemove)
            {
                _objCommandQueue.Remove(ac);
            }
        }
        private int ReserveProcessor()
        {
            for (int n = 0; n < CompilerThreads.Count(); n++)
            {
                if (CompilerThreads[n].ProcessorState == ProcessorState.Free)
                {
                    CompilerThreads[n].ProcessorState = ProcessorState.Reserved;
                    CompilerThreads[n].ReservationId = ReservationIds++;
                    return CompilerThreads[n].ReservationId;
                }
            }
            return 0;
        }
        private int UnreserveProcessor(int resId)
        {
            LogWarn("Trying to unreserving processor... resid="+resId);
            for (int n = 0; n < CompilerThreads.Count(); n++)
            {
                if (CompilerThreads[n].ReservationId == resId)
                {
                    LogWarn("..Found");
                    if(CompilerThreads[n].ProcessorState == ProcessorState.Reserved)
                    {
                        CompilerThreads[n].ReservationId = 0;
                        CompilerThreads[n].ProcessorState = ProcessorState.Free;
                        LogWarn("...Freed processor ");
                    }
                }
            }
            return 0;
        }
        private void SendCommandToReservedProcessor(AgentCommand ac)
        {
            System.Diagnostics.Debug.Assert(ac.ClientReservationId != -1);
            System.Diagnostics.Debug.Assert(ac.ClientReservationId != 0);

            BroCompilerThread aserv = CompilerThreads.Where(x => x.ReservationId == ac.ClientReservationId).FirstOrDefault();

            if (aserv == null)
                LogError("Agent thread was null.",true);

            if (aserv.ProcessorState != ProcessorState.Reserved)
            {
                LogError(" FATAL - Tried to queue Command but the reserved processor was not reserved state :"
                    + aserv.ProcessorState.ToString() 
                    + " relying on server to resend file.");
                return;
            }
            //System.Diagnostics.Debug.Assert(CompilerThreads[n].ProcessorState == ProcessorState.Reserved);

            aserv.ProcessorState = ProcessorState.Working;

            BroCommand cmd = new BroCommand();
            cmd.ThreadMessageType = ThreadMessageType.ThreadCompileFile;
            cmd.AgentCommand = ac;

            aserv.SendMessageToThread(cmd);

        }
        private void UpdateThreads()
        {
            foreach(BroCompilerThread bth in CompilerThreads)
            {
               // bth.CheckForDeadShellProcess();

                BroCommand cmd = bth.GetMessageFromThread();
                if (cmd != null)
                {
                    switch (cmd.ThreadMessageType)
                    {
                            //**Note this uses the  network packet enum but 
                        case ThreadMessageType.ThreadCompileRequestComplete:
                            SetActive();
                            //SendCompiledFileToServer(CompilerThreads[iThread], cmd);
                            SendCompletionToServer(bth, cmd);
                            break;
                        default:
                            throw new Exception("Invalid thread packet type, or not found.");

                    }

                }
            }

        }
        private void SendCompletionToServer(BroCompilerThread th, BroCommand cmd)
        {

            LogInfo("Sending Completion to server rid=" + cmd.AgentCommand.ClientReservationId);

            string strComp = SpartanGlobals.PacketTypeToMsgHeader(PacketType.CompileRequestComplete)
                + NetworkUtils.PackInt(cmd.AgentCommand.ClientReservationId)
                + NetworkUtils.PackString(cmd.CompilerOutput)
                + NetworkUtils.PackString(cmd.AgentCommand.CommandText)
                ;

            AgentCoordinator cc = ClientConnections.Where(x => x.CoordinatorId == cmd.AgentCommand.InvokingCoordinatorId).FirstOrDefault();

            if (cc == null)
            {
                LogError("Coordinator with ID " + cmd.AgentCommand.InvokingCoordinatorId 
                    + " not found.  It has probably disconnected.");
                return;
            }

            cc.Send(strComp);

            // * set process
            FreeProcessor(th);
        }
        private CompileRequestStatus QueryRequestStatus(int requestId)
        {
            for (int iThread = 0; iThread < CompilerThreads.Count; iThread++)
            {
                // check compiling
                if (
                       (CompilerThreads[iThread].AgentCommand!=null)
                    && (CompilerThreads[iThread].AgentCommand.ClientReservationId == requestId)
                    )
                    return CompileRequestStatus.Compiling;

                // check requested second - because we would have a file in the thread or not
                if (    CompilerThreads[iThread].ReservationId == requestId 
                    && CompilerThreads[iThread].ProcessorState== ProcessorState.Reserved)
                    return CompileRequestStatus.Reserved;

                // check sent
                BroSourceFile bf2 = CompiledFiles.Where(x => x.ClientReservationId == requestId).FirstOrDefault();
                if (bf2 != null)
                    return CompileRequestStatus.Sent;
            }
            //nto found
            return CompileRequestStatus.NotFound;
        }
        private void FreeProcessor(BroCompilerThread th)
        {
            th.ProcessorState = ProcessorState.Free;
            th.ReservationId = 0;
        }
        private void StopEverythingForCoordinator(AgentCoordinator sc)
        {
            LogInfo("...Stopping coordinator " + sc.CoordinatorId);
            //stop receiving data
            sc.StopTransmission();
            System.Threading.Thread.Sleep(500);

            ResetBuild();

            System.Threading.Thread.Sleep(500);
            LogInfo("...Disconnecting from coordinator " + sc.CoordinatorId);
            sc.Disconnect();

            DropCoordinator(sc);
        }
        private void ResetBuild()
        {
            LogInfo("...Resetting Build");
            LogInfo("...Killing active thread processes ");
            foreach (BroCompilerThread th in CompilerThreads)
            {
                th.Kill();
            }

            System.Threading.Thread.Sleep(500);

            LogInfo("...Killing processes ");
            KillAllCompileRelatedTasks();
        }
        private void RemoveDroppedCoordinators()
        {
            //Update dropped
            foreach (AgentCoordinator ac in _objDroppedCoordinators)
            {
                if (EnableExtendedLogging==true)
                    LogInfo("Dropping coordinator " + ac.CoordinatorId);
                else
                    Console.WriteLine("Dropping coordinator " + ac.CoordinatorId);
                ClientConnections.Remove(ac);
                foreach(BroCompilerThread bct in CompilerThreads)
                {
                    bct.PurgeIfWorkingForCoordinator(ac.CoordinatorId);
                }
            }
            _objDroppedCoordinators.Clear();
        }
        private void DropCoordinator(AgentCoordinator sc)
        {
            if (!_objDroppedCoordinators.Contains(sc))
                _objDroppedCoordinators.Add(sc);
        }
        private void KillAllCompileRelatedTasks()
        {
            LogWarn("...Killing CL");
            System.Diagnostics.Process[] processes;
            try
            {

                processes = System.Diagnostics.Process.GetProcessesByName("cl");
                foreach (System.Diagnostics.Process process in processes)
                {
                    Globals.Logger.LogInfo("  Killing " + process.Id);
                    process.Kill();
                    Globals.Logger.LogInfo("  Killed");
                }

                LogWarn("...Killing LINK");
                processes = System.Diagnostics.Process.GetProcessesByName("link");
                foreach (System.Diagnostics.Process process in processes)
                {
                    Globals.Logger.LogInfo("  Killing " + process.Id);
                    process.Kill();
                    Globals.Logger.LogInfo("  Killed");
                }
            }
            catch (Exception ex)
            {
                LogInfo("failed to stop some processes.  Swallowing exception");
                LogInfo(ex.ToString());
            }
        }
        private AgentInfo GetAgentInfo()
        {
            AgentInfo inf = new AgentInfo();

            inf.Name = System.Environment.MachineName;
            inf.IsConnected = true;

            foreach(BroCompilerThread th in CompilerThreads)
            {
                if (th == null)
                    continue;
                AgentCpuInfo cpuinf = new AgentCpuInfo();
                cpuinf.CpuId = th.ProcessorId;
                //Threads don't know about file names.  They only execute commands.
                if (th.AgentCommand == null)
                {
                    cpuinf.WorkingFileName = "";
                    cpuinf.WorkingCommand = "";
                    cpuinf.FileSendTime = DateTime.MinValue;
                }
                else
                {
                    cpuinf.WorkingFileName = th.AgentCommand.CommandText;
                    cpuinf.WorkingFileName = th.AgentCommand.OutFileName;
                    cpuinf.FileSendTime = th.AgentCommand.StartTime;
                }
                inf.Cpus.Add(cpuinf);
            }
            return inf;
        }
    }
}
