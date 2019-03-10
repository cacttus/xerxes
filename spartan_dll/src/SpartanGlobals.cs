using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;

namespace Spartan
{
    public class SpartanGlobals
    {
        #region Public: Build Variables

        public static bool GlobalsInitialized = false;
        public static string SpartanRootDirectory;  //Very important
        public static string LogFileName;

        public static BuildConfiguration BuildConfiguration = BuildConfiguration.Release;
        public static ProgramFunction ProgramFunction = ProgramFunction.None;
        public static List<CompileOption> CompileOptions = new List<CompileOption>();

        public static int UserSuppliedBuildId = -1;

        public static string AgentServiceName = "AgentService";//The name recognized by windows management.
        public static string CoordinatorExeName = "AgentCmd";//The name recognized by windows management. // Sort of fucked up
        public static string CoordinatorVsHostExeName = "AgentCmd.vshost";//The name recognized by windows managementt. // Sort of fucked up
        public static string AgentExeName = "AgentCmd";
        public static string RomulusExeName = "Romulus";
        public static string BuildGuiExeName = "BuildGui";
        public static string BuildGuiVsHostExeName = "BuildGui.vshost";

        public const int CompilerThreadTimeoutInMilliseconds = (1000) * (60) * (10);
        public const int GlobalAgentIdleWaitInSeconds = 20;
        public const int GlobalAgentIdleTimeInMilliseconds = 200; //**this needs to be faster because the BUILD GUI polls the agents every second.
        public const int SendAndRecvTimeout = 60000;
        public const int CoordinatorToAgentConnectionInterval = 15000;//This cuases a huge slowdown
        public const int CoordinatorConnectionAttemptTimeout = 100;
        public const int CoordinatorDisconnectAttemptTimeout = 100;
        public const int MaxNumServerConnections = 100000000;

        #endregion




        #region Public: Network Variables

        public const string MsgHeaderBuildRequest       = "BRQ00000";
        public const string MsgHeaderFail               = "FAIL0000"; 
        public const string MsgHeaderOkToCompileFile    = "CFL00000"; 
        public const string MsgHeaderQueryAvailable     = "AVL00000";
        public const string MsgHeaderCompileComplete    = "CCP00000";
        public const string MsgStopCompilation          = "STC00000";
        public const string MsgQueryRequestStatus       = "QRS00000";
        public const string MsgAgentStatusInfo          = "APS00000";
        public const string MsgUnreserveProcessor       = "UNRS0000";
        public const string MsgResetGlobalCompileState  = "GRSP0000";
        public const string MsgRestartServer            = "RS000000";

        public const int ClientRecvPort = 58484;
        public const int ClientSendPort = 58485;
        public const int ServerRecvPort = 58486;
        public const int ServerSendPort = 58487;//**see also buildmonitor globals.

        public static Dictionary<PacketType, string> PacketHeaderDictionary = new Dictionary<PacketType, string>();
        public static List<AgentInfo> CoordinatorAgentInfos = new List<AgentInfo>();// ** Used by coordinator only
        
        #endregion





        #region Public: Network Methods

        public static string PacketTypeToMsgHeader(PacketType pt)
        {
            string value;
            if (PacketHeaderDictionary.TryGetValue(pt, out value) == false)
                throw new NotImplementedException();
            return value;
        }
        public static PacketType GetPacketType(ref string strBuffer)
        {
            string strCode = NetworkUtils.ParseDataChunk(ref strBuffer, 8);

            PacketType myValue = PacketHeaderDictionary.FirstOrDefault(x => x.Value == strCode).Key;

            if (myValue == PacketType.InvalidPacketType)
                throw new NotImplementedException();
            return myValue;
        }
        public static BroSourceFile ReadFileFromStream(ref string buffer)
        {
            BroSourceFile f = new BroSourceFile();

            f.FileBranchName = NetworkUtils.ReadStringFromStream(ref buffer).Trim();
            int len = NetworkUtils.UnpackInt(ref buffer);

            f.FileData = NetworkUtils.ReadStringFromStream(ref buffer);

            if (f.FileData.Length != len)
                throw new Exception(" File update length was invalid. got " + f.FileData.Length + " expected " + len);

            return f;
        }
        public static AgentCommand ReadCommandFromStream(ref string buffer)
        {
            AgentCommand ac = new AgentCommand();
            ac.ClientReservationId = NetworkUtils.UnpackInt(ref buffer);
            ac.CommandText = NetworkUtils.UnpackString(ref buffer);
            ac.OutFileName = NetworkUtils.UnpackString(ref buffer);
            return ac;
        }
       
        #endregion

        #region Public: Command Methods

        public static void InitializeGlobals(List<string> args)
        {
            PacketHeaderDictionary.Add(PacketType.BuildRequest, MsgHeaderBuildRequest);
            PacketHeaderDictionary.Add(PacketType.Error, MsgHeaderFail);
            PacketHeaderDictionary.Add(PacketType.OkToCompileFile, MsgHeaderOkToCompileFile);
            PacketHeaderDictionary.Add(PacketType.AgentStateQueryAvailable, MsgHeaderQueryAvailable);
            PacketHeaderDictionary.Add(PacketType.CompileRequestComplete, MsgHeaderCompileComplete);
            PacketHeaderDictionary.Add(PacketType.StopCompilation, MsgStopCompilation);
            PacketHeaderDictionary.Add(PacketType.QueryRequestStatus, MsgQueryRequestStatus);
            PacketHeaderDictionary.Add(PacketType.AgentStatusInfo, MsgAgentStatusInfo);
            PacketHeaderDictionary.Add(PacketType.UnreserveProcessor, MsgUnreserveProcessor);
            PacketHeaderDictionary.Add(PacketType.ResetGlobalCompileState, MsgResetGlobalCompileState);
            PacketHeaderDictionary.Add(PacketType.RestartServer, MsgRestartServer);
            
            string errTxt = SpartanGlobals.ParseArgs(args);//must parse first to determine if agent.
            if (string.IsNullOrEmpty(LogFileName))
                throw new Exception("Log file name was null... /a or /c wasn't specified in command line.\n Command args are : " + args.ToString());
            Proteus.Proteus.Initialize(LogFileName);
            BuildConfig.ParseArgs(args);
            BuildConfig.LoadConfig();
            
            if(string.IsNullOrEmpty(errTxt)==false)
                Globals.Logger.LogWarn(errTxt);

            GlobalsInitialized = true;
        }
        public static string ParseArgs(List<string> args)
        {
            string temp = string.Empty;
            string tstr = string.Empty;
            string errorTxt = string.Empty; // can't log in this method as logger isn't created yet

            foreach (string str in args)
            {
                tstr = str.Trim();
                
                if (StringUtils.ParseCmdArg(tstr, BuildFlags.Debug, ref temp))
                {
                    BuildConfiguration = BuildConfiguration.Debug;
                }
                else if (StringUtils.ParseCmdArg(tstr, BuildFlags.CoordProgram, ref temp))
                {
                    if (ProgramFunction != ProgramFunction.None)
                        throw new Exception("Multiple program functions (/a or /c) specified.");
                    ProgramFunction = ProgramFunction.Coordinator;
                    SpartanGlobals.LogFileName = "coordinator.log";
                }
                else if (StringUtils.ParseCmdArg(tstr, BuildFlags.AgentProgram, ref temp))
                {
                    if (ProgramFunction != ProgramFunction.None)
                        throw new Exception("Multiple program functions (/a or /c) specified.");
                    ProgramFunction = ProgramFunction.Agent;
                    SpartanGlobals.LogFileName = "agent.log";
                }
                else if (StringUtils.ParseCmdArg(tstr, BuildFlags.Clean, ref temp))
                {
                    CompileOptions.Add(CompileOption.Clean);
                }
                else if (StringUtils.ParseCmdArg(tstr, BuildFlags.BuildDir, ref temp))
                {
                    SpartanRootDirectory = temp;
                }
                else if (StringUtils.ParseCmdArg(tstr, BuildFlags.BuildId, ref temp))
                {
                    UserSuppliedBuildId = Convert.ToInt32(temp);
                }
                else if (StringUtils.ParseCmdArg(tstr, BuildFlags.AgentName, ref temp))
                {
                    CoordinatorAgentInfos.Add(new AgentInfo() { Name = temp });
                }
                else
                {
                    errorTxt += "Possible Unrecognized argument " + str + " ";
                }
            }
            return errorTxt;
        }
        public static void ValidateForBuild()
        {
            string strErrors = string.Empty;

            if (SpartanGlobals.ProgramFunction == ProgramFunction.None)
                strErrors += " * Program function was not set.  Must specify /c for coordinator or /a for agent.";

            if (SpartanGlobals.ProgramFunction == ProgramFunction.Coordinator)
            {
                if (String.IsNullOrEmpty(BuildConfig.GlobalBuildConfiguration.BuildPlatform))
                    strErrors += " * Build Configuration platform is not set. (use /cp switch) \n";
                if (String.IsNullOrEmpty(BuildConfig.GlobalBuildConfiguration.ConfigurationName))
                    strErrors += " * Build Configuration name is not set. (use /cn switch) \n";
            }

            if (strErrors != string.Empty)
                Globals.Logger.LogError(
                    " Could not validate build configuration. \n" +
                    " The following errors must be corrected before build can proceed.:\n"
                    + strErrors,
                    true
                    );
        }


        private static bool DevOrRealProcessIsRunning(string realName, string devName)
        {
            System.Diagnostics.Process[] procs;

            procs = System.Diagnostics.Process.GetProcessesByName(realName);
            if (procs.Length > 0)
                return true;

            procs = System.Diagnostics.Process.GetProcessesByName(devName);
            if (procs.Length > 0)
                return true;

            return false;
        }
        public static bool BuildGuiIsRunning()
        {
            return DevOrRealProcessIsRunning(SpartanGlobals.BuildGuiExeName, SpartanGlobals.BuildGuiVsHostExeName);
        }
        public static bool CoordinatorIsRunning()
        {
            return DevOrRealProcessIsRunning(SpartanGlobals.CoordinatorExeName, SpartanGlobals.CoordinatorVsHostExeName);
        }
        #endregion



    }
}
