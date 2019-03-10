using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public enum TrexClientState
    {
        None,
        Connecting,
        ExecSent,
        Executing,
        Timedout,
        Success,
        Failure
    }
    public class TrexClient : NetworkClient
    {
        public TrexClientState TrexClientState;
        public string LastReturnValue  =string.Empty;
        public int LastRequestId = -1;

        /*
         * client -> exec, cmd, 1 or 0 (sync/async), 
         * server -> exec, 1 or 0
         * 
         * server executes command
         * 
         * server -> exec_complete, output, 1 or 0
         * 
         */
        public TrexClient(string name)
            : base(name)
        {
            TrexClientState = TrexClientState.None;
        }
        public void Connect()
        {
            TrexClientState = TrexClientState.Connecting;
            base.Connect(TrexUtils.ServerBindPort, true);
        }
        public bool SendCommand(string command, TrexExecutionType type, int timeoutMs)
        {
            if (!IsConnected())
                return false;

            int execType = System.Convert.ToInt32(type);// type == TrexExecutionType.Sync ? 1 : 0;

            string str = NetworkUtils.PackPacketType(NetworkPacketType.TrexExecuteCommand)
                + NetworkUtils.PackString(command)
                + NetworkUtils.PackInt(execType)
                + NetworkUtils.PackInt(timeoutMs)
                ;
            Send(str);

            TrexClientState = TrexClientState.ExecSent;

            return true;
        }
        public void Update()
        {
            Recv();
        }
        protected override void ProcessPacket(NetworkPacketType pt, string rawData)
        {
            string sndBuf = String.Empty;

            switch (pt)
            {
                case NetworkPacketType.TrexExecuteCommand:
                    int stsatus = NetworkUtils.UnpackInt(ref rawData);
                    LastRequestId = NetworkUtils.UnpackInt(ref rawData);

                    Globals.Logger.LogInfo("Got executing response from server rid=" + LastRequestId);

                    TrexClientState = TrexClientState.Executing;

                    break;
                case NetworkPacketType.TrexExecutionComplete:

                    LastReturnValue = NetworkUtils.UnpackString(ref rawData);
                    int ret = NetworkUtils.UnpackInt(ref rawData);
                    int rid = NetworkUtils.UnpackInt(ref rawData);

                    Globals.Logger.LogInfo("Got completion from server " + LastReturnValue);

                    if (ret == 0)
                    {
                        TrexClientState = TrexClientState.Success;
                    }
                    else if (ret == 1)
                    {
                        TrexClientState = TrexClientState.Failure;
                    }
                    else
                    {
                        Globals.Throw("Trex server returned invalid completion code: " + ret);
                    }

                    break;
                default:
                    base.ProcessPacket(pt, rawData);
                    break;
            }
        }
    
    
    }
}
