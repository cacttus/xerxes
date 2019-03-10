using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;
namespace Spartan
{
    public enum AgentLocality
    {
        Remote,
        Local // the agent is on the same machine as the coordinator.
    }

    //Server side construct only.
    public class CoordinatorAgent
    {
        #region Public: Members

        public string ComputerName { get; set; }
        public AgentState AgentState { get; set; }  // If there was a network error &c that we cannt connect to him
        public int ErrorCount = 0; // number of errors this agent has hit.
        public bool IsLocalAgent { get { return ComputerName == System.Environment.MachineName; } private set { } }
        public bool AlreadyTriedToConnect = false;
        public List<CoordinatorAgentVirtualProcessor> VirtualProcessors;
        public int _intRequestIdentifierGenerator = 1000;//start at 1k so we are not confuesd
        public AgentLocality AgentLocality;
        public Object _objConnectLockObject = new Object();

        private Coordinator _objCoordinator;
        private bool _bInitialized = false;
        private PacketMakerTcp _objPacketMaker = new PacketMakerTcp();
        private System.Net.Sockets.Socket MySocket
        {
            get { return _objPacketMaker.Socket; }
            set { _objPacketMaker.Socket = value; }
        }
        private bool _blnDoHandshake = true;
        private List<string> _objCommandHistory = new List<string>();

        #endregion

        public CoordinatorAgent(string name, Coordinator coord = null)
        {
            _objCoordinator = coord;

            AgentState = AgentState.Disconnected;
            ComputerName = name;

            if (ComputerName == System.Environment.MachineName)
                AgentLocality = AgentLocality.Local;
            else
                AgentLocality = AgentLocality.Remote;
        }

        #region STATE

        public void ClearState()
        {
            if (!IsInit())
                return;

            foreach (CoordinatorAgentVirtualProcessor p in VirtualProcessors)
                p.ClearState();
        }
        public void CreateProcessors(int count)
        {
            AgentState = AgentState.Connected;
            _bInitialized = true;

            if (VirtualProcessors != null)
            {
                Globals.Logger.LogError("Tried to create agent processors, but they were already created.. this is a programmer error.");
                return;
            }
            VirtualProcessors = new List<CoordinatorAgentVirtualProcessor>();
            for (int iProc = 0; iProc < count; iProc++)
                VirtualProcessors.Add(new CoordinatorAgentVirtualProcessor(iProc));
        }
        public void TryGetFreeProcessor(int intTimeout)
        {
            if (!IsInit())
                return;

            foreach (CoordinatorAgentVirtualProcessor vp in VirtualProcessors)
            {
                if ((vp.ProcessorState == ProcessorState.Free)
                    || (vp.ProcessorState == ProcessorState.Unknown))
                {
                    vp.ProcessorState = ProcessorState.PendingAnswer;
                    vp.RequestIdentifier = _intRequestIdentifierGenerator++;

                    string data = SpartanGlobals.PacketTypeToMsgHeader(PacketType.AgentStateQueryAvailable)
                        + NetworkUtils.PackInt(vp.RequestIdentifier);

                    Send(data);

                    vp.LastQueryStateStamp = System.Environment.TickCount;
                }
                else if ((vp.ProcessorState == ProcessorState.Reserved)
                        || (vp.ProcessorState == ProcessorState.PendingAnswer))
                {
                    if (intTimeout > 0)
                    {

                        if ((Environment.TickCount - vp.LastQueryStateStamp) > intTimeout)
                        {
                            Globals.Logger.LogError("[ERROR] Query state of " + intTimeout + " timed out for Host "
                                + ComputerName
                                + " Processor "
                                + vp.ProcessorId.ToString()
                                + " force querying state again..");

                            FreeProcessor(vp);
                        }

                    }
                }
            }
        }

        #endregion

        #region WIP_COMMANDS

        public void QueryFileStatuses(int QueryInterval)
        {
            if (!IsInit())
                return;

            foreach (CoordinatorAgentVirtualProcessor vp in VirtualProcessors)
            {
                if (vp.WipCommand != null)
                {
                    if (System.Environment.TickCount - vp.WipCommand.SendTime > QueryInterval)
                    {
                        QueryRequestStatus(vp.WipCommand.ReservationId);
                        vp.WipCommand.SendTime = System.Environment.TickCount; // Update send time
                    }
                }
            }
        }
        public void FreeProcessor(int iReservationId)
        {
            if (!IsInit())
                return;

            CoordinatorAgentVirtualProcessor vp = GetProcessorByReservationId(iReservationId);
            if (vp == null)
            {
                Globals.Logger.LogError("Virtual processor could not be found to free processor." +
                    " The query state likely has timed out. resid = " + iReservationId);
                return;
            }
            FreeProcessor(vp);
        }
        public void FreeProcessor(CoordinatorAgentVirtualProcessor vp, bool wipCanBeNull = false)
        {
            if (!IsInit())
                return;

            vp.ClearState(wipCanBeNull);
        }
        public void GetProcessorAndWipByAnyReservationId(int resId, ref CoordinatorAgentVirtualProcessor proc, ref WipCommand wip)
        {
            if (!IsInit())
                return;
            proc =
            VirtualProcessors.Where(x => ((x.CommandHistory.Where(
                y => (y.ReservationId == resId)
                ).FirstOrDefault() != null) || ((x.WipCommand != null) && (x.WipCommand.ReservationId == resId)))
                ).FirstOrDefault();

            if (proc == null)
                return;

            if ((proc.WipCommand == null) || (proc.WipCommand.ReservationId != resId))
            {
                Globals.Logger.LogWarn("Had to search processor command history to find the correct WIP processor command. " +
                    " This is usually due to a timeout compiling a file (long compilation)."
                    + " The file could have restarted.");


                wip = proc.CommandHistory.Where(x => x.ReservationId == resId).FirstOrDefault();

                //** Return null if the wip was restarted so we don't fuck up the system.
                if ((wip != null) && (wip.IsRestarted == true))
                    wip = null;
            }
            else
            {
                wip = proc.WipCommand;
            }
        }
        public CoordinatorAgentVirtualProcessor GetProcessorByReservationId(int rid)
        {
            if (!IsInit())
                return null;
            return VirtualProcessors.Where(x => (
                             (x.WipCommand != null)
                        && (x.WipCommand.ReservationId == rid)
                    )
                 ).FirstOrDefault();
        }
        public CoordinatorAgentVirtualProcessor GetProcessorByRequestId(int rid)
        {
            if (!IsInit())
                return null;

            if (VirtualProcessors == null)
            {
                Globals.Logger.LogWarn("Virtual processors are null for " + ComputerName);
                return null;
            }
            return VirtualProcessors.Where(x => (x.RequestIdentifier == rid)).FirstOrDefault();
        }

        private void QueryRequestStatus(int requestId)
        {
            string data =
                SpartanGlobals.PacketTypeToMsgHeader(PacketType.QueryRequestStatus)
                + NetworkUtils.PackInt(requestId)
                + NetworkUtils.PackString(ComputerName);

            Globals.Logger.LogInfo(" Sending Rqstat rqid=" + requestId + " agent=" + ComputerName);

            Send(data);
        }

        #endregion

        #region NETWORKING


        public bool IsConnected()
        {
            return (MySocket != null) && (Connected());
        }
        public bool? Connect(bool waitForResult = false, bool doHandshake = true)
        {
            lock (_objConnectLockObject)
            {
                if (AgentState == AgentState.Connecting)
                    return null;
                if (IsConnected())
                    return null;
                AgentState = AgentState.Connecting;
                _blnDoHandshake = doHandshake;
            }

            System.Net.IPAddress ip = NetworkUtils.GetIpAddress(ComputerName);

            MySocket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.IP);

            IAsyncResult result = MySocket.BeginConnect(ip, SpartanGlobals.ClientRecvPort, AgentConnected, this);

            if (waitForResult)
            {
                bool success = result.AsyncWaitHandle.WaitOne(SpartanGlobals.CoordinatorConnectionAttemptTimeout, true);
                return success;
            }
            return null;
        }

        public void Disconnect(bool blnWaitForResult = false)
        {
            lock (_objConnectLockObject)
                AgentState = AgentState.Disconnected;
            

            if (MySocket == null)
                return;
            if (IsConnected() == false)
                return;

            Globals.Logger.LogInfo("Disconnnecting..");
            try
            {
                IAsyncResult result = MySocket.BeginDisconnect(false, AgentDisconnected, this);
                if (blnWaitForResult)
                    result.AsyncWaitHandle.WaitOne(SpartanGlobals.CoordinatorDisconnectAttemptTimeout, true);
            }
            catch (System.Net.Sockets.SocketException se)
            {
                Globals.Logger.LogInfo("Trying to disconnect we got " + se.ToString());
            }
        }
        public void Send(string buf, int iTimeout = SpartanGlobals.SendAndRecvTimeout)
        {
            _objPacketMaker.SendPacket(buf, iTimeout);
            _objCommandHistory.Add(buf.Substring(0, 8));
        }
        public string Recv(int iTimeout = SpartanGlobals.SendAndRecvTimeout)
        {
            string ret = _objPacketMaker.GetNextPacket(iTimeout);
            return ret;
        }

        private bool Connected()
        {
            if (MySocket.Connected == false)
                return false;

            bool part1 = MySocket.Poll(1000, System.Net.Sockets.SelectMode.SelectRead);
            bool part2 = (MySocket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        private bool IsInit()
        {
            if (!_bInitialized)
                return false;

            if (VirtualProcessors == null)
            {
                Globals.Logger.LogWarn("Virtual processors are null for " + ComputerName);
                return false;
            }
            return true;
        }
        private void DoHandshake()
        {
            if (VirtualProcessors == null)
            {
                if (AgentState != AgentState.ProcCount)
                {
                    string coordName;
                    if (_objCoordinator == null)
                        coordName = System.Environment.MachineName;
                    else
                        coordName = _objCoordinator.GetName();

                    string data =
                        SpartanGlobals.PacketTypeToMsgHeader(PacketType.AgentStatusInfo)
                       + NetworkUtils.PackString(coordName)
                       ;

                    Send(data);
                    AgentState = AgentState.ProcCount;
                }
                //can't process anything if the vps are null.
                return;
            }
        }
        private void AgentConnected(IAsyncResult ar)
        {
            CoordinatorAgent ag = (CoordinatorAgent)ar.AsyncState;

            try
            {
                ag.MySocket.EndConnect(ar);

                Globals.Logger.LogInfo("Connected to " + ag.ComputerName);

                //  _objManualResetEvent.Set();
                bool dohs = false;
                lock (ag._objConnectLockObject)
                {
                    ag.AgentState = AgentState.Connected;
                    dohs = _blnDoHandshake;
                }

                if (dohs)
                    ag.DoHandshake();

            }
            catch (System.Net.Sockets.SocketException se)
            {
                if (se.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused)
                {
                    //Swallow - The agent is not running.
                    Globals.Logger.LogInfo("  Agent " + ComputerName + " is not running.");
                }
                if (se.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
                {
                    Globals.Logger.LogInfo("  Agent " + ComputerName + " timed out.");
                }
                lock (ag._objConnectLockObject)
                {
                    ag.AgentState = AgentState.Disconnected;
                }
            }

        }
        private void AgentDisconnected(IAsyncResult ar)
        {
            Globals.Logger.LogInfo("Disconnnected..");
        }
        #endregion

    }

}
