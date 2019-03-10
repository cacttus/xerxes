using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Proteus;
using System.Threading.Tasks;
namespace Spartan
{

    public class BroCompilerThread
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetCurrentProcessorNumber();
       // private static int _iSuspendTimeMillis = 100;

        private System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
        private System.Diagnostics.Process ShellProcess;
        private ProcessorState _eProcessorState;
        private Object _objLockObject = new Object();
        private int _iProcessorAffinityBitMask { get; set; } // - The ID of the processor we prefer this thread on.
        private Queue<BroCommand> _commandsToThread { get; set; }
        private Queue<BroCommand> _commandsToHost { get; set; }
        private string _strCompilerOutput;
        private int _intProcessorId;
      //  private int _nCommandsExecuted = 0;
        private List<int> _objDeadCoordinatorIds = new List<int>();
        private DateTime _datLastCommandTime = DateTime.MinValue;

        #region PROPERTIES
        public int ProcessorId 
        {
            get 
            {
                return _intProcessorId; 
            }
        }
        public int ReservationId { get; set; }
        private Object _objAgentCommandLockObject = new Object();
        private AgentCommand _objAgentCommand;
        public AgentCommand AgentCommand
        {
            get
            {
                lock (_objAgentCommandLockObject)
                {
                    return _objAgentCommand;
                }

            }
            set
            {
                lock (_objAgentCommandLockObject)
                {
                    _objAgentCommand = value;
                }
            }
        }
        private Object _objCompilerOutputLockObject = new Object();
        public string CompilerOutput
        {
            get
            {
                lock (_objCompilerOutputLockObject)
                {
                    return _strCompilerOutput;
                }

            }
            private set
            {
                lock (_objCompilerOutputLockObject)
                {
                    _strCompilerOutput = value;
                }
            }
        }
        private Object _objProcessorStateLockObject = new Object();
        public ProcessorState ProcessorState
        {
            get
            {
                lock (_objProcessorStateLockObject)
                {
                    return _eProcessorState;
                }

            }
            set
            {
                lock (_objProcessorStateLockObject)
                {
                    _eProcessorState = value;
                }
            }
        }
        private bool _blnProcessRunning;
        private Object _objProcessRunningLockObject = new Object();
        private bool ProcessRunning
        {
            get
            {
                lock (_objProcessRunningLockObject)
                {
                    return _blnProcessRunning;
                }
            }
            set
            {
                lock (_objProcessRunningLockObject)
                {
                    _blnProcessRunning = value;
                }
            }
        }
        #endregion

        public BroCompilerThread(int iProcessorAffinity)
        {
            _intProcessorId = iProcessorAffinity;
            _iProcessorAffinityBitMask = (1<<iProcessorAffinity); // Start only accepts 1-5 not 0-4
            _commandsToThread = new Queue<BroCommand>();
            _commandsToHost = new Queue<BroCommand>();
        }

        public void PurgeIfWorkingForCoordinator(int coordinatorId)
        {
            if(    (AgentCommand != null)
                && (AgentCommand.InvokingCoordinatorId == coordinatorId))
                    SendMessageToThread(new BroCommand(ThreadMessageType.ThreadAbortCompilation));
            _objDeadCoordinatorIds.Add(coordinatorId);
        }

        #region THREAD_COMMANDS
        public void Kill()
        {
            //Make sure we empty the queue first so we can peek at the abort.
            PurgeAllThreadMessages();
            //Send baort
            SendMessageToThread(new BroCommand(ThreadMessageType.ThreadAbortCompilation));
        }
        public Object _objCommandsToThreadLockObject = new Object();
        public Object _objCommandsToHostLockObject = new Object();
        public void PurgeAllThreadMessages()
        {
            lock (_objCommandsToHostLockObject)
                _commandsToHost.Clear();
            lock (_objCommandsToThreadLockObject)
                _commandsToThread.Clear();
        }
        public void SendMessageToThread(BroCommand cmd)
        {
            if (cmd.ThreadMessageType == ThreadMessageType.ThreadAbortCompilation)
                Globals.Logger.LogInfo("Thread got abort compilation msg.");
            lock (_objCommandsToThreadLockObject)
            {
                _commandsToThread.Enqueue(cmd);
            }
        }
        public BroCommand GetMessageFromThread()
        {
            BroCommand cmd = null;

            lock (_objCommandsToHostLockObject)
            {
                if (_commandsToHost.Count > 0)
                    cmd = _commandsToHost.Dequeue();
            }
            return cmd;
        }
        private BroCommand ThreadGetMessage()
        {
            BroCommand cmd = null;

            lock (_objCommandsToThreadLockObject)
            {
                if (_commandsToThread.Count > 0)
                {
                    cmd = _commandsToThread.Dequeue();
                    //If we have a command from a dead coordinator circumvent it and
                    // return as if hter was none.
                    if (cmd.AgentCommand != null)
                    {
                        if (_objDeadCoordinatorIds.Contains(cmd.AgentCommand.InvokingCoordinatorId))
                        {
                            Globals.Logger.LogInfo("Ignoring dead thread message");
                            cmd = null;
                        }
                    }
                }
              
            }
            return cmd;
        }
        private void ThreadSendMessage(BroCommand cmd)
        {
            lock (_objCommandsToHostLockObject)
            {
                _commandsToHost.Enqueue(cmd);
            }
        }
        #endregion

        public void Run()
        {
            bw.DoWork += WaitForCommands;
            // bw.RunWorkerCompleted += SendCompiledObjectToServer;
            bw.RunWorkerAsync();
        }
        private void BeginProcessing()
        {
            ProcessRunning = true;
          //  _nCommandsExecuted = 0;
        }
        private void EndProcessing()
        {
            ProcessRunning = false;
        }
        private void CreateCompilerProcess()
        {
            _eProcessorState = ProcessorState.Working;
            ReservationId = 100000;//NOte always set reservation ID to ZERO when we free the processor

            LogMsg(" All Ok...");
            CompilerOutput = ""; //reset the garbage

            ReservationId = 0;//NOte always set reservation ID to ZERO when we free the processor
            _eProcessorState = ProcessorState.Free;
        }

        private void WaitForCommands(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // we may have problem directing to stdout
            CreateCompilerProcess();

            while (true)
            {
                ProcessNextThreadMessage();

                if ((DateTime.Now - _datLastCommandTime).TotalSeconds > SpartanGlobals.GlobalAgentIdleWaitInSeconds)
                    System.Threading.Thread.Sleep(SpartanGlobals.GlobalAgentIdleTimeInMilliseconds);
                System.Windows.Forms.Application.DoEvents();
            }
        }
        private ThreadMessageType? PeekNextThreadMessage()
        {
            lock (_objCommandsToThreadLockObject)
            {
                if (_commandsToThread.Count > 0)
                    return _commandsToThread.Peek().ThreadMessageType;
            }
            return null;
        }
        private void ProcessNextThreadMessage()
        {
            BroCommand rcvCmd = ThreadGetMessage();
            if (rcvCmd != null)
            {
                //Note: these are sync locked
                ProcessCommand(rcvCmd);
            }
            else
            {
                //Wait a bit to process messages
                System.Windows.Forms.Application.DoEvents();
            }
        }
        private void ProcessCommand(BroCommand objCmd)
        {
            string dat = objCmd.Data;

            switch (objCmd.ThreadMessageType)
            {
                case ThreadMessageType.ThreadCompileFile:
                    CompileFile(objCmd);
                    break;
                case ThreadMessageType.ThreadAbortCompilation:
                    ResetThreadState();
                    break;
                default:
                    break;
            }
        }
        private void LogMsg(string str)
        {
            Console.WriteLine("Core " + _intProcessorId + "(" + GetCurrentProcessorNumber() + ")[" + System.Threading.Thread.CurrentThread.ManagedThreadId + "] " + str);
        }
        #region OUTPUT_HANDLERS
        /*
        public void OnConsoleDataReceivedHandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //**For some reason we're getting null here which seems to come
            // from another separate process so just return and ignore.
            if (e.Data == null)
                return;
            System.Diagnostics.Debug.Assert(sender == ShellProcess);
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("#complete"))
                {
                    System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(e.Data, "#.*#");
                    string st = m.Value;
                    st=st.Replace("#","");
                    int rqid = System.Convert.ToInt32(st.Split(new char[] { '|' })[1]);
                    int pid = System.Convert.ToInt32(st.Split(new char[] { '|' })[2]);

                    if (
                        (rqid != ReservationId) ||
                        (pid != _intProcessorId)
                        )
                    {
                        int n = 0;
                        n++;
                    }

                    LogMsg("rq " + rqid + " Complete.");

                    //We got all data
                    EndProcessing();
                    return;
                }
                CompilerOutput += e.Data;
                CompilerOutput += "\n";

            }
        }
         * */
        #endregion
        private void CompileFile(BroCommand objCmd)
        {
            _datLastCommandTime = DateTime.Now;

            if (objCmd.AgentCommand == null)
                throw new Exception("[Thread ] No Source file given.");

            AgentCommand = objCmd.AgentCommand;

            BeginProcessing();

            //****************************
            // If the file name is too long, we have to break up the command into
            // an arguments file and use that arguments file.
            string strCommand = objCmd.AgentCommand.CommandText;
          //  System.IO.FileStream fileStream = null;
            string tempPath = "";
            string strArgs, strExe;
            strArgs = string.Empty;
            strExe = string.Empty;
            if (strCommand.Length > 2047)
            {

                // create and use a file
                tempPath = System.IO.Path.Combine(
                    System.IO.Directory.GetCurrentDirectory(),
                    System.Environment.TickCount.ToString() + ".txt");

                BroCompilerUtils.SplitCommand(strCommand, ref strExe, ref strArgs);

                //If we are not link or lib then throw
                if (!strExe.Contains("link.exe"))
                    if (!strExe.Contains("lib.exe"))
                        if (!strExe.Contains("cl.exe"))
                            Globals.Logger.LogError("Command was too long.  Command must be either link.exe or lib.exe or cl.exe to become truncated.");


                System.IO.File.WriteAllBytes(tempPath, System.Text.Encoding.ASCII.GetBytes(strArgs));
                strCommand = strExe;
                strArgs = " @" + tempPath;
            }
            ShellProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo inf;
            if(strArgs!=string.Empty)
                inf = new System.Diagnostics.ProcessStartInfo(strCommand, strArgs);
            else
                inf = new System.Diagnostics.ProcessStartInfo(strCommand);
           // System.Diagnostics.ProcessStartInfo inf = new System.Diagnostics.ProcessStartInfo(strExe,strArgs);

            inf.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            inf.CreateNoWindow = true;
            inf.UseShellExecute = false;
            //inf.RedirectStandardInput = true;
            inf.RedirectStandardOutput = true;
            inf.RedirectStandardError = true;

            ShellProcess.StartInfo = inf;

            string strOutput = string.Empty;
            string strError = string.Empty;

            ShellProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    strOutput += args.Data + Environment.NewLine;
                }
            };
            ShellProcess.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    strError += args.Data + Environment.NewLine;
                }
            };

            bool blnSuccess = true;

            //**************************
            // Start our crap
            //**Main CL Program execute
            try
            {
                //sometimes proc exits before we can execute things on it.
                //ShellProcess.ProcessorAffinity = new IntPtr(_iProcessorAffinityBitMask);
                ShellProcess.Start();
                //ShellProcess.BeginErrorReadLine();
                //ShellProcess.BeginOutputReadLine();
                ShellProcess.PriorityClass = ProcessPriorityClass.RealTime;
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError(ex.ToString());
                blnSuccess = false;
            }

            //**************************
            // Wait for crap to exti
            try
            {
                using (Task<bool> processWaiter = Task.Factory.StartNew(() => ShellProcess.WaitForExit(SpartanGlobals.CompilerThreadTimeoutInMilliseconds)))
                using (Task<string> outputReader = Task.Factory.StartNew((Func<object, string>)ReadStream, ShellProcess.StandardOutput))
                using (Task<string> errorReader = Task.Factory.StartNew((Func<object, string>)ReadStream, ShellProcess.StandardError))
                {
                    bool waitResult = processWaiter.Result;

                    if (!waitResult)
                    {
                        ShellProcess.Kill();
                    }

                    Task.WaitAll(outputReader, errorReader);
                    // if waitResult == true hope those already finished or will finish fast
                    // otherwise wait for taks to complete to be able to dispose them

                    if (!waitResult)
                        blnSuccess = false;
                    else
                        blnSuccess = true;

                    //exitCode = ShellProcess.ExitCode;

                    strOutput = outputReader.Result;
                    strError = errorReader.Result;
                }

            }
            catch (Exception ex)
            {
                Globals.Logger.LogError(ex.ToString());
                blnSuccess = false;
            }
            //**************************
            // Wait for crap to exti
            //if (ShellProcess.WaitForExit(SpartanGlobals.CompilerThreadTimeoutInMilliseconds))
            //{
            //}
            //else
            //{
            //    ShellProcess.Kill();
            //    blnSuccess = false;
            //}

            //**************************
            //Try to delete "too big" command file if we made one
            try
            {
                if (tempPath != string.Empty && System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError("Could not delete temp file " + tempPath + "\n  Exception:\n" + ex.ToString());
            }

            //**************************
            //**Set output in our proc
            CompilerOutput = strOutput + strError;
            CompilerOutput += "##end##\n";

            //**************************
            if (objCmd.AgentCommand != null)
            {
                if (!_objDeadCoordinatorIds.Contains(objCmd.AgentCommand.InvokingCoordinatorId))
                {
                    // Send Response when done
                    BroCommand toHostCmd = new BroCommand();
                    toHostCmd.ThreadMessageType = ThreadMessageType.ThreadCompileRequestComplete;
                    toHostCmd.AgentCommand = objCmd.AgentCommand;
                    toHostCmd.CompilerOutput = CompilerOutput;
                    CompilerOutput = "";

                    AgentCommand = null;

                    LogMsg(toHostCmd.CompilerOutput);

                    ThreadSendMessage(toHostCmd);
                }
                else
                {
                    Globals.Logger.LogInfo(
                        "Got dead message for coordinator id "
                        + objCmd.AgentCommand.InvokingCoordinatorId
                        + ". ignroing."
                        );
                }
            }

        }
        private void ResetThreadState()
        {
            Globals.Logger.LogInfo("Resetting processor " + _intProcessorId);
            System.Threading.Thread.Sleep(1000);
            CompilerOutput = "";
            ProcessRunning = false;
            _eProcessorState = ProcessorState.Free;
            AgentCommand = null;
            PurgeAllThreadMessages();
        }

        private static string ReadStream(object streamReader)
        {
            string result = ((System.IO.StreamReader)streamReader).ReadToEnd();

            return result;
        } // put breakpoint on this line


    }




}
