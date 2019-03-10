using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;

namespace Spartan
{
    public class PendingProcessor
    {
        public int _intRequestId;
        public int _intReservedId;
        public CoordinatorAgent _objAgent;
        public CoordinatorAgentVirtualProcessor _objProcessor;
    }
    //public List<AgentBuildStats> AgentBuildStats;
    public class Coordinator : IBroCompilerObject
    {
        #region Private: Members
        
        private List<CoordinatorAgent> _objDroppedAgents = new List<CoordinatorAgent>();
        private BuildOrganizer _objBuildOrganizer;
        private int _iConnectStamp = SpartanGlobals.CoordinatorToAgentConnectionInterval; // last stamp when we tried to connect.  Setting this to this value allows us to connect on startup
        private ConsoleProcess _objConsoleProcess = new ConsoleProcess();
       
        #endregion

        #region Public: Members
        //**DEBUG: adds extra logging.
        public bool EnableExtendedLogging = false;

        public List<CoordinatorAgent> Agents = new List<CoordinatorAgent>();
        public String TempDirectory { get; set; } //temp compiled directory
        public System.Net.Sockets.TcpListener Listener { get; set; }
        public CompilerOutput CompilerOutput;
        public List<PendingProcessor> _objPendingProcessors = new List<PendingProcessor>();
        public Dictionary<string, List<BuildStep>> _objLocalQueue = new Dictionary<string, List<BuildStep>>();
        public BuildStats BuildStats;
        
        #endregion

        public Coordinator()
            : base("C [" + System.Environment.MachineName + "] ")
        {
        }

        #region Public: Methods

        public string GetName()
        {
            return System.Environment.MachineName + "_COORD";
        }
        public override void Run()
        {
            InitBuild();

            //Clean up temp stuff if specified
            CleanTempData();

            //Run the makefile
            RunRomulus();

            _objBuildOrganizer = new BuildOrganizer(BuildConfig.GetMakeFilePath(), BuildConfig.GetBatchFilePath());
            _objBuildOrganizer.LoadFilesAndCreateHierarchy();

            //add our guys
            AddAgents();

            //DO WORK
            CompileProjects();

            EndBuild();

        }

        #endregion

        #region Private: Methods
        private void EndBuild()
        {
            //Exit..bye
            //NOTE: do not move compiler output to folder - this happens automatically with the auto logger.
            DisconnectAgents();
        }
        private void UpdateCheckForUi()
        {
            if (!BuildConfig.BuildProcessesAreAttachedToUI)
                return;

            // If the BuildGUI ui is not running, and we are instructed to run from teh UI - then 
            // kill this process (saves some time when the build gui ends abruptly)
            
            if (!SpartanGlobals.BuildGuiIsRunning())
            {
                Globals.Logger.LogInfo("BuildGui is not found build will now terminate.");
                StopCompilation();
                Environment.Exit(1);
            }

        }
        private void RunRomulus()
        {
            LogInfo("Invoking Romulus Makefile");
            //Main meth0d
            _objConsoleProcess.BeginAsync();

            string cmdText = "";
            cmdText += BroCompilerUtils.RomulusBinPath + " ";
            if (SpartanGlobals.SpartanRootDirectory == null)
                throw new Exception("Spartan root directory cannot be null.  Please specify option /o ex. /o\".\\dir\\\" ");

            //We shouldn't need to re-initialize the build config from here.
            cmdText += StringUtils.MakeArg(BuildFlags.BuildDir ,BuildUtils.Enquote(BuildConfig.BuildConfigFilePath));
            cmdText += StringUtils.MakeArg(BuildFlags.ConfigName ,BuildUtils.Enquote(BuildConfig.GlobalBuildConfiguration.ConfigurationName));
            cmdText += StringUtils.MakeArg(BuildFlags.ConfigPlatform ,BuildUtils.Enquote(BuildConfig.GlobalBuildConfiguration.BuildPlatform));

            cmdText += StringUtils.MakeArg("/i"); //?

            if (SpartanGlobals.BuildConfiguration == BuildConfiguration.Debug)
                cmdText += StringUtils.MakeArg(BuildFlags.Debug); //debug
            else if (SpartanGlobals.BuildConfiguration == BuildConfiguration.Release)
                ;
            else
                throw new Exception("Invalid bld config");

            _objConsoleProcess.Execute(cmdText);
            //Begin/end exist here cbecuause romulus must exit so we knwo the makefile wa smade
            _objConsoleProcess.Exit(true);
        }
        private void AddAgents()
        {
            LogInfo("Adding Agents");
            foreach (AgentInfo inf in SpartanGlobals.CoordinatorAgentInfos)
            {
                if (inf.Name == null)
                    Globals.Logger.LogError("Agent info name was null - the /ag switch passed in was probably null also.", true);
                Agents.Add(new CoordinatorAgent( inf.Name,this));
            }
        }
        private void CleanTempData()
        {
            //Don't remove
            // We don't specify clean in the make file because
            // that is OPTIONAL for each build.
            if (SpartanGlobals.CompileOptions.Contains(CompileOption.Clean))
            {
                // BuildConfig.te
                CleanDir(BuildConfig.GlobalRootedTempPath, true);
                CleanDir(BuildConfig.GlobalRootedLibPath, true);
                //CleanDir(BroCompilerUtils.CleanDr_Tmp);
                // CleanFile(BroCompilerUtils.CleanFl_Pdb);
                //CleanFile(BroCompilerUtils.CleanFl_Exe);
                //CleanFile(BroCompilerUtils.CleanFl_Ilk);
            }
        }

        private void DropAgent(CoordinatorAgent ag)
        {
            ag.AgentState = AgentState.Dropped;

            ag.Disconnect();

            if (_objDroppedAgents == null)
                _objDroppedAgents = new List<CoordinatorAgent>();
            _objDroppedAgents.Add(ag);
        }
        private void ConnectToAgents(int interval)
        {
            bool blnAtLeastOneConnected = false;

            if (System.Environment.TickCount - _iConnectStamp > interval)
            {
                int ta = System.Environment.TickCount;
                LogInfo(" Polling for new agents...");
                _iConnectStamp = System.Environment.TickCount;
                foreach (CoordinatorAgent ag in Agents)
                {
                    //if (ag.AlreadyTriedToConnect == true)
                    //   continue;

                    if (ag.IsConnected())
                    {
                        blnAtLeastOneConnected = true;
                        continue;
                    }
                    try
                    {
                        ag.Connect();

                        // blnAtLeastOneConnected = true;
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        if (ag.AlreadyTriedToConnect == false)
                            LogWarn("Failed to connect to host '" + ag.ComputerName + "'- Host was unknown");
                        ag.AlreadyTriedToConnect = true;
                    }
                }
                if (blnAtLeastOneConnected == false)
                    LogWarn("No agents connected...cannot compile");
                LogInfo("...done");

                int tb = System.Environment.TickCount;
                LogInfo("Poll took " + (tb - ta) + "ms.");
            }


        }
        private void DisconnectAgents()
        {
            LogInfo("Disconnecting from agents");

            StopCompilation();
            System.Threading.Thread.Sleep(500);
            for (int i = 0; i < Agents.Count; i++)
            {
                Agents[i].Disconnect();
            }
        }

        private void InitBuild()
        {

            BuildStats = new BuildStats();
            BuildStats.BuildStartTime = DateTime.Now;

            CompilerOutput = new CompilerOutput(BuildConfig.GlobalCompileOutputDirectory, SpartanGlobals.UserSuppliedBuildId);
            CompilerOutput.AddLine(BuildUtils.DateTimeString(DateTime.Now) + " Build started ");

        }

        private void CompileProjects()
        {
            LogInfo("Compiling");
            _objBuildOrganizer.StartBuild();
            BuildStats.TotalFileCount = _objBuildOrganizer.NumRemainingSteps();

            ExecuteHeaderCommands();

            DistributeFilesAndPollAgents();
        }
        private void ExecuteHeaderCommands()
        {
            //TODO: 
            //  mkdir for the debug / release
            // and such *** 
            // *** do this crap in C# NOT in batch becasue batch is FUCKED

            ////Main meth0d
            _objConsoleProcess.BeginAsync();

            while (true)
            {
                BuildStep bs = _objBuildOrganizer.GetNextAvailableBuildStepCommand();
                if (bs == null)
                    break;

                //Remove the "CMD " from teh beginning of the command
                bs.CommandText = bs.CommandText.Substring(4, bs.CommandText.Length - 4);

                _objConsoleProcess.Execute(bs.CommandText);

                _objBuildOrganizer.SetStepCompleted(bs);
            }
            _objConsoleProcess.Exit(true);
        }
        private void DistributeFilesAndPollAgents()
        {
            while (_objBuildOrganizer.IsBuilding())
            {
                //Just call this here blah
                PollAgents();

                //Send build steps to processors
                DistributeFiles();

                // Kill our process if the UI dies.
                UpdateCheckForUi();

                //Debug so we can see the cmd                
                System.Windows.Forms.Application.DoEvents();
                if (BuildConfig.BuildInterval > 0)
                {
                    System.Threading.Thread.Sleep(BuildConfig.BuildInterval);
                }
            }
            BuildStats.BuildEndTime = DateTime.Now;
            
            LogInfo("%*|Build Complete|" + (BuildStats.BuildEndTime - BuildStats.BuildStartTime).ToString());

        }
        private void DistributeFiles()
        {
            List<PendingProcessor> toRemove = new List<PendingProcessor>();
            foreach (PendingProcessor pp in _objPendingProcessors)
            {
                if (ExecuteBuildStep(pp) == true)
                    toRemove.Add(pp);
            }
            foreach (PendingProcessor pp in toRemove)
                _objPendingProcessors.Remove(pp);
            
        }
        private void PollAgents()
        {
            string originaldata;
            string data;
            CoordinatorAgent objAgent;

            // Try to connect to agents every 3 seconds
            ConnectToAgents(SpartanGlobals.CoordinatorToAgentConnectionInterval);

            for (int iAgent = 0; iAgent < Agents.Count; iAgent++)
            {
                objAgent = Agents[iAgent];
                try
                {
                    if (!objAgent.IsConnected())
                        continue;

                    originaldata = objAgent.Recv();
                    data = originaldata;

                    if (String.IsNullOrEmpty(data) == false)
                    {
                        //Do stuff
                        PacketType pt = SpartanGlobals.GetPacketType(ref data);
                        HandlePacketData(objAgent, pt, ref data);
                    }

                    if (!_objBuildOrganizer.IsBuilding())
                        break;

                    //if (_objCommandQueue.Count == 0)
                    //{
                    //    // - Compile complete - we have to log and shit.
                    //    LogInfo("Compile Complete..");
                    //    _eCompileStatus = CompileStatus.Complete;
                    //    break;
                    //}

                    objAgent.TryGetFreeProcessor(BuildConfig.ProcessorQueryTimeout);
                    objAgent.QueryFileStatuses(BuildConfig.FileStatusQueryInterval);
                }
                catch (System.Net.Sockets.SocketException sex)
                {
                    LogError(sex.ToString());
                    objAgent.ErrorCount++;
                    objAgent.ClearState();
                    LogInfo("Agent " + objAgent.ComputerName + " has " + objAgent.ErrorCount + " errors.");
                    //Just log and quit - don't drop the agen.
                    //DropAgent(objAgent);
                }

            }


            // Update dropped
            UpdateDroppedAgents();
        }
        private void StopCompilation()
        {
            _objBuildOrganizer.StopBuild();

            foreach (CoordinatorAgent ag in Agents)
            {
                if (ag.IsConnected())
                {
                    ag.Send(SpartanGlobals.PacketTypeToMsgHeader(PacketType.StopCompilation));
                }
            }

            System.Threading.Thread.Sleep(500);

            foreach (CoordinatorAgent ag in Agents)
            {
                DropAgent(ag);
            }

        }
        private void UpdateDroppedAgents()
        {
            foreach (CoordinatorAgent aa in _objDroppedAgents)
            {
                Agents.Remove(aa);
            }

            if (Agents.Count == 0)
            {
                LogError("All agents have been dropped - Compilation aborted.", false);
                _objBuildOrganizer.StopBuild();
            }
        }
        private void HandlePacketData(CoordinatorAgent ag, PacketType pt, ref string data)
        {
            switch (pt)
            {
                case PacketType.AgentStatusInfo:
                    //How many processors does the agent have

                    string coordName = NetworkUtils.UnpackString(ref data);
                    
                    if (coordName != GetName())
                    {
                        LogError("Received agent info packet for wrong coordinator: " + coordName);
                    }
                    else
                    {
                        AgentInfo inf = new AgentInfo();
                        inf.Deserialize(ref data);

                        SetAgentProcCountAndBeginTransactions(inf.Name, inf.Cpus.Count);
                    }

                    break;
                case PacketType.AgentStateQueryAvailable:
                    //We have a processor now Distribute a file
                    int requestId = NetworkUtils.UnpackInt(ref data);
                    int reservedId = NetworkUtils.UnpackInt(ref data);

                    HandleQueryState(ag, requestId, reservedId);

                    break;
                case PacketType.OkToCompileFile:
                    // ** client is OK with compiling this file.
                   if(EnableExtendedLogging==true)
                       LogInfo("<< Compilation Confirmed Agent: " + ag.ComputerName + " resid:" + NetworkUtils.UnpackInt(ref data));

                    break;
                case PacketType.CompileRequestComplete:
                    //We have completed a compilation woo woo
                    int reservationid = NetworkUtils.UnpackInt(ref data);
                    string compilerOutput = NetworkUtils.UnpackString(ref data);
                    string commandText = NetworkUtils.UnpackString(ref data);

                    CompleteCompileRequest(ag, reservationid, compilerOutput);

                    break;
                case PacketType.QueryRequestStatus:
                    // Where is my damn file??
                    CompileRequestStatus st = (CompileRequestStatus)NetworkUtils.UnpackInt(ref data);
                    int resId = NetworkUtils.UnpackInt(ref data);
                    string agentName2 = NetworkUtils.UnpackString(ref data);

                    HandleRequestStatus(ag, st, resId, agentName2);

                    break;
                default:
                    LogError("Got an invalid response from the client: " + data, true);
                    break;
            }
        }
        private void CompleteCompileRequest(CoordinatorAgent ag, int reservationId, string compilerOutput)
        {
            //Send info to Log and whatever else.
            CoordinatorAgentVirtualProcessor vp = null;
            WipCommand wp = null;
            bool success = true;

            //get the crap
            CoordGetprocessorAndWipByReservationId(reservationId, ag, ref vp, ref wp);

            if ((wp == null) || (vp == null))
                return;

            //handle output
            success = HandleCompilerOutput(compilerOutput);
            
            // Finish build.
            wp.BuildStep.EndTime = DateTime.Now;

            // Print to console
            PrintStats(wp, ag, vp, reservationId);

            // Finalize the file.
            FinalizeCompiledFile(success, reservationId, ag, ref vp, ref wp);
        }
        private void FinalizeCompiledFile(bool success, int reservationId, CoordinatorAgent ag, ref CoordinatorAgentVirtualProcessor vp, ref WipCommand wp)
        {
            if (success == false)
                wp.BuildStep.Status = BuildStepStatus.Failed;
            else
                wp.BuildStep.Status = BuildStepStatus.Success;

            if (vp.WipCommand == null)
                Globals.Logger.LogError("Wip command was n ull.. why IDK - command could have taken too long (which it shouldn null the wip) ",true);

            _objBuildOrganizer.SetStepCompleted(vp.WipCommand.BuildStep);
            ag.FreeProcessor(reservationId);
        }
        private void CoordGetprocessorAndWipByReservationId(int reservationId,
                                                        CoordinatorAgent ag,
                                                        ref CoordinatorAgentVirtualProcessor vp,
                                                        ref WipCommand wp)
        {
            ag.GetProcessorAndWipByAnyReservationId(reservationId, ref vp, ref wp);

            // Cleanup processor resources
            if (vp == null)
            {
                LogError("Virtual processor not found.  File has been lost.  has been associated with agent = " 
                    + ag.ComputerName 
                    + " reservation ID " 
                    + reservationId);
                return;
            }
            if(wp==null)
            {
                LogError("Could not find wip. Possibly it has been restarted. Ignoring compiled file ");
                return;
            }
            if (vp.WipCommand == null)
            {
                LogError("Could not get valid wip command for processor. File has been lost. " 
                    + " The processor has likely been cleared. agent = " 
                    + ag.ComputerName 
                    + " resId = " + reservationId);
                return;
            }
            if (vp.WipCommand.BuildStep == null)
            {
                LogError("Could not get valid build step for Wip command.   File has been lost.  agent = " 
                    + ag.ComputerName 
                    + " procId = " 
                    + vp.ProcessorId 
                    + " resid= " 
                    + reservationId);
                return;
            }
        }
        private bool HandleCompilerOutput(string compilerOutput)
        {
            bool success = true;

            if (String.IsNullOrEmpty(compilerOutput))
                LogWarn("No compiler output was returned for file. File might have passed through.");

            if (!String.IsNullOrEmpty(compilerOutput))
            {
                List<string> lst = new List<string>(compilerOutput.Split(new char[] { '\n' }));

                foreach (string line in lst)
                {
                    //**This also seems to indicate the end of a file stream in ML.
                    if (String.IsNullOrEmpty(line))
                        continue;

                    LogInfo(line);
                    if (line.Contains("error"))
                    {
                        // Add error per file error.
                        _objBuildOrganizer.AddBuildError();
                    }

                    CompilerOutput.AddLine(line);
                }
            }

            if (_objBuildOrganizer.BuildStatus == BuildStatus.CompileErrorLimitReached) 
            {
                CompilerOutput.AddLine("Max Error limit of " + BuildConfig.CompilerMaxErrorLimit + " reached. Aborting compilation.");
            }

            return success;
        }
        private void PrintStats(WipCommand wc, CoordinatorAgent ag, CoordinatorAgentVirtualProcessor vp, int reservationId)
        {
            TimeSpan ts = wc.BuildStep.EndTime - wc.BuildStep.StartTime;
            int nRemainingSteps= _objBuildOrganizer.NumRemainingSteps();
          
            TimeSpan bt = DateTime.Now - BuildStats.BuildStartTime;
            if(EnableExtendedLogging==true)
                LogInfo("<<COMPLETE [" + ag.ComputerName + "," + reservationId + "] " 
                    + ts.TotalSeconds.ToString() + "s" 
                    + "   (" + (BuildStats.TotalFileCount-nRemainingSteps) + "/" + BuildStats.TotalFileCount + ") "
                    + " (Total:" + String.Format("{0:hh\\:mm\\:ss}",bt) + ")");

            //Used for parsing file
            Globals.Logger.LogInfo("%<" + ag.ComputerName + "|" + vp.ProcessorId + "|" + wc.BuildStep.OutputFileName + "|" + reservationId);
        }
        private void HandleRequestStatus(CoordinatorAgent ag, CompileRequestStatus st, int resId, string agentName)
        {
            CoordinatorAgent ag2 = Agents.Where(x => x.ComputerName.Equals(agentName)).FirstOrDefault();

            if (ag == null)
                throw new Exception("Compiler agent " 
                    + agentName 
                    + " was not found while querying a file request status with id " 
                    + st.ToString());
            
            if (!ag2.ComputerName.Equals(ag.ComputerName))
                throw new Exception("Compiler agent " 
                    + agentName 
                    + " got the wrong request.. with id " 
                    + st.ToString());

            CoordinatorAgentVirtualProcessor vp = null;
            WipCommand wip = null;
            ag.GetProcessorAndWipByAnyReservationId(resId, ref vp, ref wip);

            //File was complete.
            if ((wip.BuildStep.Status == BuildStepStatus.Success)
                || (wip.BuildStep.Status == BuildStepStatus.Failed))
            {
                LogInfo("RqStat - File was successful. Ignoring.");
                return;
            }

            if (vp == null)
                throw new Exception(" Could not get agent by request id... request lost... one or more files have been missed.");

            switch (st)
            {
                case CompileRequestStatus.Compiling:
                    //Request is compiling
                    LogInfo("[CompRQ] File query....file is compiling.." + " (resid=" + resId + ")");
                    break;
                case CompileRequestStatus.NotFound:
                    LogError("[CompRQ] File sent to agent was not found. Re-Queueing file" + " (resid=" + resId + ")");
                    
                    ////////////
                    if (vp.WipCommand != null)
                    {
                        vp.WipCommand.IsRestarted = true;
                        _objBuildOrganizer.ReInsertFailedStep(vp.WipCommand.BuildStep);
                    }
                    ag.FreeProcessor(resId);
                    /////////////
                    break;
                case CompileRequestStatus.Reserved:
                    LogError("[CompRQ] File sent to agent was reserved but never made it to agent." + " (resid=" + resId + ")");

                    string data = SpartanGlobals.PacketTypeToMsgHeader(PacketType.UnreserveProcessor)
                        + NetworkUtils.PackInt(resId);

                    ag.Send(data);
                    
                    ///////////
                    if (vp.WipCommand != null)
                    {
                        vp.WipCommand.IsRestarted = true;
                        _objBuildOrganizer.ReInsertFailedStep(vp.WipCommand.BuildStep);
                    }
                    ag.FreeProcessor(resId);
                    ///////////////
                    break;
                case CompileRequestStatus.Sent:
                    LogInfo("[CompRQ] File sent to agent was compiled and sent to server" + " (resid=" + resId + ")");
                    //if(vp.WipCommand!=null)
                    //    _objBuildOrganizer.ReInsertFailedStep(vp.WipCommand.BuildStep);
                    //ag.FreeProcessor(resId);
                    break;
                default:
                    throw new NotImplementedException();//Invalid Enum
            }

        }
        private void HandleQueryState(CoordinatorAgent objAgent, int requestId, int reservedId)
        {
            CoordinatorAgentVirtualProcessor vp;
           
            // We got an open query from the client so we can send a packet.
            System.Diagnostics.Debug.Assert(requestId != 0);
            
            // ***GET REQUESTED RPOCESSRO
            vp = objAgent.GetProcessorByRequestId(requestId);

            if (vp == null)
            {
                LogError("Processor with Request ID " + requestId + " could not be found", false);
                return;
            }
            if (reservedId == 0)
            {
                // ** No processors are available
                // Free and query again.
                objAgent.FreeProcessor(vp, true);
                return; 
            }
            if (vp.WipCommand != null)
            {
                throw new Exception("Wip command was not null.. there was an error clearing the build step somewhere.");
            }
            
            if (EnableExtendedLogging == true)
                LogInfo(">> Got Agent: " + objAgent.ComputerName + " RequestID:" + requestId + " reserveID:" + reservedId);
            
            AddPendingProcessor(objAgent, requestId, reservedId, vp);
        }
        private void AddPendingProcessor(CoordinatorAgent objAgent,
                                         int requestId,
                                         int reservedId,
                                         CoordinatorAgentVirtualProcessor vp
                                        )
        {
            _objPendingProcessors.Add(new PendingProcessor()
                {
                    _objAgent = objAgent,
                    _intRequestId = requestId,
                    _intReservedId = reservedId,
                    _objProcessor = vp
                }
            );
        }
        private bool ExecuteBuildStep(PendingProcessor pp)
        {
            //****GET BUILD STEP
            BuildStep bs = GetBuildStepForMachine(pp._objAgent);

            if (bs == null)
            {
                //There was no processor - save it for next time we need it.
                return false;
            }
            //Used for parsing file
            Globals.Logger.LogInfo("%>" + pp._objAgent.ComputerName + "|" + pp._objProcessor.ProcessorId + "|" + bs.OutputFileName + "|" + pp._intReservedId);

            //CoordinatorAgent objAgent, int reservedId, CoordinatorAgentVirtualProcessor vp
            //****SET PENDING STATUS
            pp._objProcessor.CreateWipCommand(new WipCommand(System.Environment.TickCount, pp._intReservedId, bs));
            pp._objProcessor.WipCommand.BuildStep = bs;
            bs.StartTime = DateTime.Now;

            // *** SETND BUILD SETP TO SERVER
            string dataBuf =
                  SpartanGlobals.PacketTypeToMsgHeader(PacketType.OkToCompileFile)
                + NetworkUtils.PackInt(pp._intReservedId) //we need this to confirm the correct file that was sent.
                + NetworkUtils.PackString(bs.CommandText)
                + NetworkUtils.PackString(bs.OutputFileName)
                ;

            // Send the file.
            pp._objAgent.Send(dataBuf);

            return true;
        }
        private BuildStep GetBuildStepForMachine(CoordinatorAgent objAgent)
        {
            //Try to get a build step for a machine.  We do this so we can filter out
            // link / lib steps and execute them only on the local machine.  Executing link/lib
            // on remote machines kills performacne.

            BuildStep bs = null;
            //
            // STEP 1/2
            //
            // First try to get BS from the rogue dictionary if we have a match.
            foreach (string key in _objLocalQueue.Keys)
            {
                if (objAgent.ComputerName == key)
                {
                    List<BuildStep> bsl = null;

                    if (_objLocalQueue.TryGetValue(key, out bsl) == false)
                        Globals.Logger.LogError("Could not get value from dictionary. Idk why 1.", true);

                    // If there are items, get one.
                    if (bsl.Count > 0)
                    {
                        LogInfo("Got Local build step for " + objAgent.ComputerName);
                        bs = bsl.ElementAt(0);
                        bsl.RemoveAt(0);
                    }

                    // We have a match - break out
                    break;
                }
            }
            //
            // STEP 2/2
            //
            // - Next try to get a new BS if there are no rogue matches
            if (bs == null)
            {
                while (true)
                {
                    bs = _objBuildOrganizer.GetNextAvailableBuildStep();
                    if (bs == null)
                    {
                        //Build is complete if there are no more nodes. otherwise there are no commands at present.
                        break;
                    }

                    if ((bs.BuildTargetType == BuildTargetType.Executable) ||
                        (bs.BuildTargetType == BuildTargetType.Library) &&
                        (objAgent.ComputerName != Environment.MachineName))
                    {
                        //This build step cannot be executed on this machine. 
                        // Queue it up until we find a free agent (which we will distribute it in the above code)
                        if (_objLocalQueue.Keys.Contains(Environment.MachineName) == false)
                            _objLocalQueue.Add(Environment.MachineName, new List<BuildStep>());

                        List<BuildStep> bsl = null;
                        if (_objLocalQueue.TryGetValue(Environment.MachineName, out bsl) == false)
                            Globals.Logger.LogError("Could not get value from dictionary. Idk why 2.", true);

                        LogInfo("Step " + "Added step to local queue for agent " + objAgent.ComputerName);
                        //Forces tree to skip it

                        bsl.Add(bs);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return bs;
        }
        private void CleanDir(string dir, bool recursive = false)
        {
            if (System.IO.Directory.Exists(dir))
            {
                
                string[] files = System.IO.Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    LogInfo("Deleting.." + file);
                    try 
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception)
                    {
                        Globals.Logger.LogWarn("Could not delete file " + file);
                    }
                }

                string[] dirs = System.IO.Directory.GetDirectories(dir);
                
                if (recursive == true)
                {
                    foreach (string dir2 in dirs)
                    {
                        CleanDir(dir2, recursive);
                    }
                    try
                    {
                        System.IO.Directory.Delete(dir);
                    }
                    catch (Exception)
                    {
                        Globals.Logger.LogWarn("[Clean]: Could not delete directory " + dir);

                    }
                }
                else if (dirs.Length > 0)
                {
                    LogWarn("Clean: Directory has subdirectories - ignoring sub-directories.");
                }
            }
            else
            {
                Globals.Logger.LogWarn("Clean: Directory not found " + dir);
            }
        }
        private void CleanFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                LogInfo("Cleaning.." + file);
                System.IO.File.Delete(file);
            }
        }
        private void SetAgentProcCountAndBeginTransactions(string name, int count)
        {

            foreach (CoordinatorAgent ag in Agents)
            {
                if (ag.ComputerName.Equals(name))
                {
                    ag.CreateProcessors(count);
                    return;
                }
            }
            throw new Exception("Agent proc query failed..could not find agent:" + name);
        }
       
        #endregion

    }

}
