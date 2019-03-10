using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;
namespace Spartan
{
    //Coordinator connection on the agent.
    public class AgentCoordinator
    {
        public int CoordinatorId;
        PacketMakerTcp _objPacketMaker = new PacketMakerTcp();
        public System.Net.Sockets.Socket Socket { get { return _objPacketMaker.Socket; } set { _objPacketMaker.Socket = value; } }

        private bool _blnStopTransmission = false;

        public AgentCoordinator(int coordID)
        {
            CoordinatorId = coordID;
        }
        public void StopTransmission() { _blnStopTransmission = true; }
        public void Disconnect()
        {
            Socket.Disconnect(false);
        }
        public void Send(string str, int iTimeout = SpartanGlobals.SendAndRecvTimeout)
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
                Globals.Logger.LogInfo("Failed to send data: Coordiantor was not connected");
                return;
            }
            _objPacketMaker.SendPacket(str, iTimeout);
        }
        public string Recv(int iTimeout = SpartanGlobals.SendAndRecvTimeout)
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
    }
}
