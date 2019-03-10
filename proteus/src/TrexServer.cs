using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Proteus
{
    
    public class TrexServer : NetworkServer
    {
        #region Private:Members

        private class TrexCommand 
        {
            public TrexVirtualClient Client;
            public string Text;
            public ConsoleProcess ConsoleProcess;
            public int CommandRequestId;
            public int Timeout;
        }

        private int _intRequestIdGenerator = 0;
        private List<TrexCommand> _objPendingCommands = new List<TrexCommand>();
        private TrexCommand _objSyncExecutingCommand;
        private List<TrexCommand> _objAsyncExecutingCommands = new List<TrexCommand>();
        
        #endregion

        #region Public:Methods

        public TrexServer(int port) : 
            base(port)
        {
        }
        public int QueueExecution(TrexVirtualClient client, string command, TrexExecutionType type, int timeout)
        {
            TrexCommand cmd;
            cmd = new TrexCommand { Client = client, Text = command, CommandRequestId = _intRequestIdGenerator++, Timeout = timeout };

            if (type == TrexExecutionType.Async)
            {
                Execute(cmd);
                _objAsyncExecutingCommands.Add(cmd);
            }
            else
            {
                _objPendingCommands.Add(cmd);
            }

            return cmd.CommandRequestId;
        }

        #endregion

        #region Protected:Methods
        protected override NetworkVirtualClient CreateNewClient(System.Net.Sockets.Socket sock, int intClientId)
        {
            //**Override in derived class.
            return new TrexVirtualClient(this, sock, intClientId);
        }
        protected override void Update()
        {
            base.Update();
            ExecutePending();
            CompleteExecutedCommands();
        }
        #endregion
       
        #region Private:Methods

        private void Execute(TrexCommand cmd)
        {
            cmd.ConsoleProcess = new ConsoleProcess(true);
            cmd.ConsoleProcess.BeginAsync();
            cmd.ConsoleProcess.Execute(cmd.Text, cmd.Timeout);
        }
        private void ExecutePending()
        {
            List<TrexCommand> toRemove = new List<TrexCommand>();

            foreach (TrexCommand cmd in _objPendingCommands)
            {
                if (_objSyncExecutingCommand != null)
                    break;

                Globals.Logger.LogInfo("Executing " + cmd + " for " + cmd.Client.ClientName);

                Execute(cmd);

                _objSyncExecutingCommand = cmd;
                toRemove.Add(cmd);
            }

            //REMOVE
            foreach (TrexCommand cmd in toRemove)
                _objPendingCommands.Remove(cmd);
            toRemove.Clear();
        }
        private void CompleteExecutedCommands()
        {
            List<TrexCommand> toRemove = new List<TrexCommand>();

            if (_objSyncExecutingCommand!=null)
                if (CompleteCommand(_objSyncExecutingCommand))
                    _objSyncExecutingCommand = null;

            foreach (TrexCommand cmd in _objAsyncExecutingCommands)
            {
                if(CompleteCommand(cmd))
                    toRemove.Add(cmd);
            }

            //REMOVE
            foreach (TrexCommand cmd in toRemove)
                _objAsyncExecutingCommands.Remove(cmd);
            toRemove.Clear();
        }
        private bool CompleteCommand(TrexCommand cmd)
        {
            if (cmd == null)
                Globals.Throw(" TrEDX Command was null");
            if(cmd.ConsoleProcess == null)
                return true;
            if ((cmd.ConsoleProcess.IsRunning == true) || (cmd.ConsoleProcess.HasStarted==false))
                return false;

            Globals.Logger.LogInfo("Command complete Agent= "
                + cmd.Client.ClientName + " >>>cmd= "
                + cmd.Text + "\n >>>return data= "
                + cmd.ConsoleProcess.CommandOutput);


            if (cmd.Client.IsConnected() == true)
            {
                string data = NetworkUtils.PackPacketType(NetworkPacketType.TrexExecutionComplete)
                    + NetworkUtils.PackString(cmd.ConsoleProcess.CommandOutput)
                    + NetworkUtils.PackInt(System.Convert.ToInt32(!cmd.ConsoleProcess.Success))//We use !SUccess because we are reading success as 0 instead of 1 where <>0 is an error
                    + NetworkUtils.PackInt(cmd.CommandRequestId);
                    ; // zero means success
                cmd.Client.Send(data);
            }
            else
            {
                Globals.Logger.LogWarn("Client " + cmd.Client.ClientName + " has disconnected. Not return value sent");
            }

            cmd.ConsoleProcess = null;
            return true;
        }

        #endregion



    }
}
