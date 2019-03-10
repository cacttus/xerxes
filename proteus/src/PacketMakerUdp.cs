using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class PacketMakerUdp : PacketMakerBase
    {
        public delegate void PacketsReceivedDelegate(string strPacket);

        private System.Net.Sockets.UdpClient _objCommandClient;
        private int _intRecvPort;

        private PacketsReceivedDelegate _objDelegate;

        public PacketMakerUdp()
        {
        }
        public void SendAsync(string data, string strRcvHost, int intRcvHostPort)
        {
            byte[] bytes = MakePacket(data);
            ConstructSocket(false);
            _objCommandClient.BeginSend(bytes, bytes.Length, 
                strRcvHost, intRcvHostPort, new AsyncCallback(SendPendingCommandsCallback), null);
        }
        public void RecvAsync(int port, PacketsReceivedDelegate del)
        {
            _intRecvPort = port;
            _objDelegate = del;
            ConstructSocket(true);
            _objCommandClient.BeginReceive(new AsyncCallback(ReceivePendingCommandsCallback), null);
        }
        private void ConstructSocket(bool isRecvSocket)
        {
            if (isRecvSocket)
                _objCommandClient = new System.Net.Sockets.UdpClient(_intRecvPort);
            else
                _objCommandClient = new System.Net.Sockets.UdpClient();//**No port. Important.
        }
        private void ReceivePendingCommandsCallback(IAsyncResult res)
        {
            System.Net.IPEndPoint RemoteIpEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, _intRecvPort);
            byte[] received = _objCommandClient.EndReceive(res, ref RemoteIpEndPoint);

            string str = System.Text.Encoding.ASCII.GetString(received);
            AddReceivedDataToPacket(str);

            while (_lstPackets.Count > 0)
            {
                _objDelegate(_lstPackets.Dequeue());
            }

            _objCommandClient.BeginReceive(new AsyncCallback(ReceivePendingCommandsCallback), null);
        }
        private void SendPendingCommandsCallback(IAsyncResult res)
        {
            //Wut
        }

    }
}
