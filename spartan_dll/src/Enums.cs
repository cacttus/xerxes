using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;
namespace Spartan
{

    #region UTIL_CLASSES


    public class AgentCommand
    {
        public int StepNumber;
        //**new instaed of sending sources we will send agent commands.
        public string CommandText;
        public AgentCommandType AgentCommandType;
        public int ClientReservationId;
//        public AgentCoordinator InvokingCoordinator { get; set; }//Agent Only
        public int InvokingCoordinatorId;// { get; set; }//Agent Only
        public DateTime StartTime;

        public string OutFileName;  //Coord only
        public List<string> Dependencies = new List<string>();
    }

    public class BroSourceFile
    {
        public string FileData { get; set; }
        public string FileBranchName { get; set; }//Contains both path in branch root, and filename.
        public string CompilerArgs { get; set; }
        public string ProjectDirectory { get; set; }
        public string CompilerInputFileName { get; set; }
        public string CompilerOutputFileName { get; set; }
        public string BinOutputRootDirectory { get; set; }

        public int ClientReservationId { get; set; } // id the client uses to reserve the file to a processor
    }
    public class BroCompiledFile
    {
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }
    }
    public class BroCommand
    {
        public ThreadMessageType ThreadMessageType { get; set; }
        public string Data { get; set; }
        public string CompilerOutput = "";
        public AgentCommand AgentCommand { get; set; }
        public BroCommand() {}
        public BroCommand(ThreadMessageType type) { this.ThreadMessageType = type; }
    }
    public class WipCommand
    {
        public int SendTime { get; set; }   //time we sent the file to the client, NOTE: this changes when we re-query every x milis
        public int ReservationId { get; set; } // client-specific reservation id for file.
        //public AgentCommand AgentCommand { get; set; }
        public BuildStep BuildStep;
        public bool IsRestarted = false;

        public WipCommand(int sendTime, int resId, BuildStep aa)
        {
            SendTime = sendTime;
            ReservationId = resId;
            BuildStep = aa;
        }
    }
    #endregion

    #region GLOBAL_ENUMS

    public enum AgentCommandType
    {
        Compile, Link, Lib
    }
    public enum BuildConfiguration { Debug, Release }
    public enum ProgramFunction {  None, Coordinator, Agent }
    public enum CompileOption { Clean }
    public enum ProcessorState { Unknown, PendingAnswer, Working, Reserved, Free }
    public enum AgentState { Disconnected, Connecting, Connected, ProcCount, Dropped }
    public enum QueryState { Open, Pending }
    public enum FileNeeded { Need, DontNeed }

    // Types of packets
    public enum PacketType
    {
        InvalidPacketType,
        Error,
        Ack,
        BuildRequest,               // Request a build on the server
        FileUpdate,                 // a file is to be replaced in the local branch
        CheckFileUpdate,            // [hdr8][datetime][pathlen8][filepath]see if we need to update the given file.
        OkToCompileFile,            // thread is to compile a file (for the inter client only
        CompileRequestComplete,     // compiled file.
        AgentStateQueryAvailable,   // we are queried to return the number of free processors (int)
        StopCompilation,
        UpdateFileList,
        QueryRequestStatus,
        AgentStatusInfo,
        UnreserveProcessor,
        ResetGlobalCompileState,
        RestartServer
    }
    public enum ThreadMessageType
    {
        ThreadCompileFile,
        ThreadCompileRequestComplete,
        ThreadAbortCompilation
    }
    public enum CompileRequestStatus
    {
        Reserved,   // id has been reserved.. waiting for file instructions.
        Compiling,  // file in process
        NotFound,   // request id has not yet been generated
        Sent        // file was sent back to host
    }

#endregion

}
