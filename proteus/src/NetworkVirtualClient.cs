using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    /// <summary>
    /// Represents a client on the server side.  hence "virtual"
    /// Allows Server to send data to clietns.
    /// </summary>
    /// <param name="acceptedSocket"></param>
    public class NetworkVirtualClient
    {
        public int ClientId = 0;
        public string ClientName;
        public System.Net.Sockets.Socket Socket { get { return _objPacketMaker.Socket; } set { _objPacketMaker.Socket = value; } }
       
        private PacketMakerTcp _objPacketMaker;
        private bool _blnStopTransmission = false;

        public NetworkVirtualClient(System.Net.Sockets.Socket acceptedSocket, int id)
        {
            ClientId = id;
            _objPacketMaker = new PacketMakerTcp(acceptedSocket);

            // Get name.
            System.Net.IPEndPoint ip = (System.Net.IPEndPoint)acceptedSocket.RemoteEndPoint;
            ClientName = System.Net.Dns.GetHostEntry(ip.Address).HostName;
        }
        public bool ProcessData()
        {
            //rcv
            string ret = Recv();

            if (string.IsNullOrEmpty(ret) == false)
            {
                Globals.Logger.LogInfo("Got packet " + ret);
                NetworkPacketType pt = NetworkUtils.UnpackPacketType(ref ret);
                ParseReceivedData(pt, ret);
            }

            return !string.IsNullOrEmpty(ret);
        }
        public void StopTransmission()
        {
            _blnStopTransmission = true;
        }
        public void Disconnect()
        {
            Socket.Disconnect(false);
        }
        public void Send(string str, int iTimeout = NetworkSettings.DefaultSendAndRecvTimeout)
        {
            if (_blnStopTransmission == true)
                return;

            if (Socket == null)
            {
                Globals.Logger.LogInfo("Failed to send data: Socket was null");
                return;
            }
            if (!IsConnected())
            {
                Globals.Logger.LogInfo("Failed to send data: Server was not connected");
                return;
            }
            _objPacketMaker.SendPacket(str, iTimeout);
        }
        public string Recv(int iTimeout = NetworkSettings.DefaultSendAndRecvTimeout)
        {
            if (_blnStopTransmission == true)
                return "";

            return _objPacketMaker.GetNextPacket(iTimeout);
        }
        public bool IsConnected()
        {
            if (Socket.Connected == false)
                return false;
            bool part1 = Socket.Poll(1000, System.Net.Sockets.SelectMode.SelectRead);
            bool part2 = (Socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        protected virtual void ParseReceivedData(NetworkPacketType pt, string rawData)
        {
            string sndBuf = String.Empty;

            switch (pt)
            {
                case NetworkPacketType.Handshake:

                    break;
                default:
                    Globals.Logger.LogError("Error Invalid packet Header: Buf Data: " + rawData);
                    sndBuf = NetworkUtils.PackPacketType(NetworkPacketType.Error);
                    Send(sndBuf);
                    break;
            }
        }




    }
}
