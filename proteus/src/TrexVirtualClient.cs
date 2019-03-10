using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Proteus
{
    public class TrexVirtualClient : NetworkVirtualClient
    {
        TrexServer _objServer;
        public TrexVirtualClient(TrexServer server, System.Net.Sockets.Socket sock, int id) : 
            base(sock,id)
        {
            _objServer = server;
        }
        protected override void ParseReceivedData(NetworkPacketType pt, string rawData)
        {
            string sndBuf = String.Empty;
            int code;

            switch (pt)
            {
                case NetworkPacketType.TrexExecuteCommand:
                    string cmdText = NetworkUtils.UnpackString(ref rawData);
                    TrexExecutionType tet = (TrexExecutionType)NetworkUtils.UnpackInt(ref rawData);
                    int timeout = NetworkUtils.UnpackInt(ref rawData);

                    int rid = _objServer.QueueExecution(this,  cmdText, tet, timeout);

                    sndBuf = NetworkUtils.PackPacketType(NetworkPacketType.TrexExecuteCommand)
                        + NetworkUtils.PackInt(1)
                        + NetworkUtils.PackInt(rid);
                    Send(sndBuf);
                    break;
                default:
                    base.ParseReceivedData(pt, rawData);
                break;
            }
        }
    }
}
