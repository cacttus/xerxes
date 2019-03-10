using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public enum NetworkObjectLocality  { Remote, Local }
    public enum NetworkObjectState { Disconnected, Connecting, Handshaking, Connected, Dropped }

    /// <summary>
    /// Base class for clients to establish connections to servers.
    /// Represents a server on the client.
    /// </summary>
    public class NetworkClient
    {
        #region Public:Members

        public string ServerName { get; set; }
        public int ClientId; // local id
        public NetworkObjectState NetworkObjectState { get; set; }  // If there was a network error &c that we cannt connect to him
        public NetworkObjectLocality NetworkObjectLocality;
        public NetworkVirtualServer VirtualServer;

        #endregion

        #region Private:Members

        private bool _bInitialized = false;
        public System.Net.Sockets.Socket MySocket
        {
            get { return _objPacketMaker.Socket; }
            set { _objPacketMaker.Socket = value; }
        }
        private PacketMakerTcp _objPacketMaker = new PacketMakerTcp();
        private Object _objConnectLockObject = new Object();

        #endregion

        #region Public:Methods

        public NetworkClient(string name)
        {
            ServerName = name;
            _objPacketMaker = new PacketMakerTcp();

            if (ServerName == System.Environment.MachineName)
                NetworkObjectLocality = NetworkObjectLocality.Local;
            else
                NetworkObjectLocality = NetworkObjectLocality.Remote;
        }
        public bool IsConnected()
        {
            return (MySocket != null) && (Connected());
        }
        public bool? Connect(int intRemotePort,
                             bool waitForResult = false,
                             int waitForResultTimeoutMilliseconds = 10000)
        {
            lock (_objConnectLockObject)
            {
                if (NetworkObjectState == NetworkObjectState.Connecting)
                    return null;
                if (NetworkObjectState == NetworkObjectState.Handshaking)
                    return null;
                if (IsConnected())
                    return null;

                NetworkObjectState = NetworkObjectState.Connecting;
            }

            System.Net.IPAddress ip = null;
            System.Net.IPHostEntry hostEntry;

            hostEntry = System.Net.Dns.GetHostEntry(ServerName);

            if (hostEntry == null)
                Globals.Logger.LogError("Failed to find host enrty for computer name:" + ServerName, true);

            for (int n = 0; n < hostEntry.AddressList.Length; n++)
                if (hostEntry.AddressList[n].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ip = hostEntry.AddressList[n];

            if (ip == null)
                Globals.Logger.LogError("Failed to find ip address for computer host:" + ServerName, true);

            MySocket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.IP
                );

            IAsyncResult result = MySocket.BeginConnect(ip, intRemotePort, AgentConnectedCallback, this);

            if (waitForResult==true)
            {
                bool success = result.AsyncWaitHandle.WaitOne(waitForResultTimeoutMilliseconds, true);
                return success;
            }
            return null;
        }
        public void Disconnect()
        {
            lock (_objConnectLockObject)
                NetworkObjectState = NetworkObjectState.Disconnected;

            if (MySocket == null)
                return;

            if (IsConnected() == false)
                return;

            Globals.Logger.LogInfo("Disconnnecting..");

            try
            {
                MySocket.Disconnect(false);
            }
            catch (System.Net.Sockets.SocketException se)
            {
                Globals.Logger.LogInfo("Trying to disconnect we got " + se.ToString());
            }
            Globals.Logger.LogInfo("Disconnnected..");
        }
        public void Send(string buf, int iTimeout = NetworkSettings.DefaultSendAndRecvTimeout)
        {
            _objPacketMaker.SendPacket(buf, iTimeout);
        }
        public string Recv(int iTimeout = NetworkSettings.DefaultSendAndRecvTimeout)
        {
            string ret = _objPacketMaker.GetNextPacket(iTimeout);

            if (String.IsNullOrEmpty(ret) == false)
            {
                NetworkPacketType pt = NetworkUtils.UnpackPacketType(ref ret);
                ProcessPacket(pt, ret);
            }

            return ret;
        }
        #endregion

        #region Protected:Methods

        protected virtual void ProcessPacket(NetworkPacketType pt, string rawData)
        {
            string sndBuf = String.Empty;
 
            switch (pt)
            {
                case NetworkPacketType.Handshake:
                    // Handshake from server complete.
                    ClientId = NetworkUtils.UnpackInt(ref rawData);
                    NetworkObjectState = NetworkObjectState.Connected;
                    break;
                default:
                    Globals.Logger.LogError("Error Invalid packet Header: Buf Data: " + rawData);
                    sndBuf = NetworkUtils.PackPacketType(NetworkPacketType.Error);
                    Send(sndBuf);
                    break;
            }
        }

        #endregion

        #region Private:Methods

        private bool Connected()
        {
            if (MySocket.Connected == false)
                return false;

            bool part1 = MySocket.Poll(1000, System.Net.Sockets.SelectMode.SelectRead);
           
            bool part2 = (MySocket.Available == 0);

            if (part1 && part2)
                return false;
            else
                return true;
        }
        private bool IsInit()
        {
            return _bInitialized;
        }
        private void AgentConnectedCallback(IAsyncResult ar)
        {
            NetworkClient ag = (NetworkClient)ar.AsyncState;

            try
            {
                ag.MySocket.EndConnect(ar);
                
                DoHandshake();

                Globals.Logger.LogInfo("Connected to " + ag.ServerName);

                lock (ag._objConnectLockObject)
                {
                    ag.NetworkObjectState = NetworkObjectState.Connected;
                }
            }
            catch (System.Net.Sockets.SocketException se)
            {
                if (se.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused)
                {
                    //Swallow - The agent is not running.
                    Globals.Logger.LogInfo("  Agent " + ServerName + " is not running.");
                }
                if (se.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
                {
                    Globals.Logger.LogInfo("  Agent " + ServerName + " timed out.");
                }
                lock (ag._objConnectLockObject)
                {
                    ag.NetworkObjectState = NetworkObjectState.Disconnected;
                }
            }

        }
        private void DoHandshake()
        {
            // Send a handshake to the server
            string data =
                NetworkUtils.PackPacketType(NetworkPacketType.Handshake)
               + NetworkUtils.PackString(ServerName)
               ;

            Send(data);

            NetworkObjectState = NetworkObjectState.Handshaking;
        }
        #endregion

    }
}
