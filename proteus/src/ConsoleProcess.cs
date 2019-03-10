using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{

    public class ConsoleProcess
    {
        #region Props Private

        private System.ComponentModel.BackgroundWorker bw;
        private System.Diagnostics.Process ShellProcess { get; set; }
        private string _strOutput;
        private Object _objLockObject = new Object();
        private Object _objOutputLockObject = new Object();
        private Object _objMessageLockObject = new Object();
        private ConsoleProcessCommandState _eCommandState;
        private Queue<ConsoleProcessCommand> _commandsToThread { get; set; }
        private Queue<ConsoleProcessCommand> _commandsToHost { get; set; }
        private int _intProcessorIndex;
        private bool _blnSynchronous;

        #endregion

        #region Props Public
        public bool Success;
        private bool _blnHasStarted = false;
        public bool HasStarted
        {
            get
            {
                return _blnHasStarted;
            }
        }
        public bool IsRunning
        {
            get
            {
                return (CommandState == ConsoleProcessCommandState.Running);
            }
        }
        public string CommandOutput
        {
            get
            {
                lock (_objOutputLockObject)
                {
                    return _strOutput;
                }

            }
            private set
            {
                lock (_objOutputLockObject)
                {
                    _strOutput = value;
                }
            }
        }
        public ConsoleProcessCommandState CommandState
        {
            get
            {
                lock (_objLockObject)
                {
                    return _eCommandState;
                }

            }
            private set
            {
                lock (_objLockObject)
                {
                    _eCommandState = value;
                }
            }
        }
        protected System.ComponentModel.BackgroundWorker BackgroundWorker
        {
            get
            {
                lock (_objLockObject)
                {
                    return bw;
                }

            }
            private set
            {
                lock (_objLockObject)
                {
                    bw = value;
                }
            }
        }
       
        #endregion

        public ConsoleProcess(bool synchronous = false)
        {
            _commandsToThread = new Queue<ConsoleProcessCommand>();
            _commandsToHost = new Queue<ConsoleProcessCommand>();
            _blnSynchronous = synchronous;
        }

        #region Public: Methods
       
        public void BeginAsync(int iProcessorIndex = -1)
        {
            _intProcessorIndex = iProcessorIndex;

            bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += WaitForCommands;
            //bw.RunWorkerCompleted += WorkerCompleted;
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }
        public void Execute(string cmdText, int timeoutMs = -1)
        {
            //**Note: if synchronous and the process doesn't exit, the command will block.
            SendMessage(new ConsoleProcessCommand(cmdText, ConsoleProcessCommandType.ExecuteCommandText, _blnSynchronous, timeoutMs));
        }
        public void Exit(bool waitForThreadToDie, int timeout = 4000)
        {
            int tA = System.Environment.TickCount;
            //Wait for idle.
            while(CommandState == ConsoleProcessCommandState.Running && _commandsToThread.Count>0)
            {
                if ((System.Environment.TickCount - tA) > timeout)
                {
                    Globals.Logger.LogWarn("Timeout exceeded for concoel porocess.");
                    break;
                }
                System.Windows.Forms.Application.DoEvents();
            }
            SendMessage(new ConsoleProcessCommand("yes", ConsoleProcessCommandType.Exit));
            while (CommandState != ConsoleProcessCommandState.Exited)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            BackgroundWorker.CancelAsync();

        }
        public void SendMessage(ConsoleProcessCommand cmd)
        {
            lock (_objMessageLockObject)
            {
                _commandsToThread.Enqueue(cmd);
            }
        }
        public ConsoleProcessCommand GetMessageFromThread()
        {
            ConsoleProcessCommand cmd = null;

            lock (_objMessageLockObject)
            {
                if (_commandsToHost.Count > 0)
                    cmd = _commandsToHost.Dequeue();
            }
            return cmd;
        }
        
        #endregion

        #region Protected: Methods

        protected virtual bool ProcessCommand(ConsoleProcessCommand cmd)
        {
            bool blnReturn = true;
            switch (cmd.CommandType)
            {
                case ConsoleProcessCommandType.ExecuteCommandText:
                    lock (_objLockObject)
                    {
                        CommandState = ConsoleProcessCommandState.Running;
                        _blnHasStarted = true;
                        if (_blnSynchronous)
                        {
                            ExecuteCommandSync(cmd.CommandText, cmd.TimeoutInMilliseconds);
                        }
                        else
                        {
                            ShellProcess.StandardInput.WriteLine("rem #BEGIN#");
                            ShellProcess.StandardInput.WriteLine(cmd.CommandText);
                            ShellProcess.StandardInput.WriteLine("rem #COMPLETE#");
                        }
                    }
                    break;
                case ConsoleProcessCommandType.Exit:
                    lock (_objLockObject)
                    {
                        if (_blnSynchronous)
                        {
                            CommandState = ConsoleProcessCommandState.Exited;
                        }
                        else
                        {
                            ShellProcess.StandardInput.WriteLine("rem #EXIT#");
                            ShellProcess.StandardInput.BaseStream.Flush();
                        }

                        //if (CommandState == CommandState.Running)
                        //    WaitForLastExecutedCommand();

                        blnReturn = false;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return blnReturn;
        }
        
        #endregion

        #region Private: Methods
        private void FinishExecuting(string output, bool success)
        {
            
            //**Set output in our proc
            CommandOutput = output;
            CommandOutput += "##end##\n";

            Success = success;
            CommandState = ConsoleProcessCommandState.Idle;
        }
        private void ExecuteCommandSync(string text, int timeoutInMilliseconds)
        {
            //  string strExe = string.Empty, strArgs = string.Empty;
            // BroCompilerUtils.SplitCommand(objCmd.AgentCommand.CommandText, ref strExe, ref strArgs);
            string processName;
            string processArgs;
            
            try
            {
                text = text.Trim();
                processName = text.Substring(0, text.IndexOf(" "));
                processArgs = text.Substring(text.IndexOf(" "));
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError(ex.ToString());
                FinishExecuting("Failed to get process name from string.", false);
                return;
            }

            ShellProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo inf = new System.Diagnostics.ProcessStartInfo(processName,processArgs);

            inf.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            inf.CreateNoWindow = false;
            inf.UseShellExecute = false;
            //inf.RedirectStandardInput = true;
            inf.RedirectStandardOutput = true;
            inf.RedirectStandardError = true;

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

            ShellProcess.StartInfo = inf;

            bool blnSuccess = true;
            //**Main CL Program execute
            try
            {
                // STart will fail if process not found.
                ShellProcess.Start();
                ShellProcess.BeginErrorReadLine();
                ShellProcess.BeginOutputReadLine();

                // Processes cannot exit until all their output is written.
                // if the output of hte process fills the output buffer the buffer
                // must be read.
                if (ShellProcess.WaitForExit(timeoutInMilliseconds))
                {
                }
                else
                {
                    ShellProcess.Kill();
                    blnSuccess = false;
                }
                
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError(ex.ToString());
                strOutput += ex.ToString();
            }
            
            FinishExecuting(strOutput + strError, blnSuccess);
        }

        private ConsoleProcessCommand ThreadGetMessage()
        {
            ConsoleProcessCommand cmd = null;

            //System.Threading.Monitor.TryEnter(_objLockObject, 10);
            lock (_objMessageLockObject)
            {
                if (_commandsToThread.Count > 0)
                    cmd = _commandsToThread.Dequeue();
            }
            return cmd;
        }
        private void ThreadSendMessage(ConsoleProcessCommand cmd)
        {
            lock (_objMessageLockObject)
            {
                _commandsToHost.Enqueue(cmd);
            }
        }
        private void InvokeConsole()
        {
            if (_blnSynchronous)
            {

            }
            else
            {
                ShellProcess = CreateShellProcess();
            
                // Must read async see the vc documentation on ReadToEnd().. it will deadlock
                ShellProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(DataReceivedEventHandler);
                ShellProcess.BeginOutputReadLine();

            }
        }
        private System.Diagnostics.Process CreateShellProcess()
        {
            System.Diagnostics.ProcessStartInfo inf = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K ");
            inf.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            //inf.CreateNoWindow = true;
            inf.UseShellExecute = false;
            inf.RedirectStandardInput = true;
            inf.RedirectStandardOutput = true;
            
            System.Diagnostics.Process ret = System.Diagnostics.Process.Start(inf);

            return ret;
        }
        private void DataReceivedEventHandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("#EXIT#"))
                {
                    //We got all data
                    CommandState = ConsoleProcessCommandState.Exited;
                    return;
                }
                if (e.Data.Contains("#COMPLETE#"))
                {
                    //We got all data
                    CommandState = ConsoleProcessCommandState.Idle;
                    return;
                }
                CommandOutput += e.Data;
                CommandOutput += "\n";
                Console.Write("[" + System.Threading.Thread.CurrentThread.ManagedThreadId + "] ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(e.Data);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        private void WaitForCommands(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            CommandState = ConsoleProcessCommandState.Idle;

            InvokeConsole();
            while (true)
            {
                ConsoleProcessCommand rcvCmd = ThreadGetMessage();
                if (rcvCmd != null &&
                    CommandState != ConsoleProcessCommandState.Exited)
                {
                    if (ProcessCommand(rcvCmd) == false)
                        break;
                }
                else
                {
                    //Wait a bit to process messages
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            

        }
        private void WaitForLastExecutedCommand(int timeout = (10*60*1000))
        {
            int a = System.Environment.TickCount;
            while (CommandState != ConsoleProcessCommandState.Idle &&
                CommandState != ConsoleProcessCommandState.Exited)
            {
                int b = System.Environment.TickCount;
                if (b - a > timeout)
                {
                    CommandOutput += " [ERROR] Process exceeded max timout limit of " + timeout.ToString() + "ms";
                    return;
                }
                System.Threading.Thread.Sleep(100);
            }
            //Note we set the processor state ADFTER we have sent the file.
        }
       
        #endregion

    }
}
