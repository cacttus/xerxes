using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    /// <summary>
    /// Represents a server object. 
    /// Clients use NetworkClient to send data to this endpoint.
    /// </summary>
    public abstract class NetworkServer
    {
        private int _intClientIdGenerator = 0;

        public System.Net.Sockets.TcpListener Listener { get; set; }
        public List<NetworkVirtualClient> VirtualClients = new List<NetworkVirtualClient>();

        private int _intReceivePort;
        private DateTime _datLastCommandTime = DateTime.MinValue;
        private List<NetworkVirtualClient> _objDroppedClients = new List<NetworkVirtualClient>();
        public int IdleWait = 100;
        public int IdleTime = 1000;
        public int IdleStep = 0;

        public NetworkServer(int receivePort)
        {
            _intReceivePort = receivePort;
            IdleWait = NetworkSettings.GlobalServerIdleWaitInSeconds;
            IdleTime = NetworkSettings.GlobalServerIdleTimeInMilliseconds;
            IdleStep = NetworkSettings.GlobalServerIdleStepTimeInMilliseconds;
        }

        public void ListenForData()
        {
            System.Threading.EventWaitHandle wh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

            Globals.Logger.LogInfo("Setting up...");
            ListenForConnections();

            Globals.Logger.LogInfo("Setup complete...");
            while (true)
            {
                try
                {
                    AcceptConnections();
                    UpdateClients();
                    RemoveDroppedClients();
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Globals.Logger.LogInfo(" [Sockets] Error: " + e.ToString());
                }

                Update();
                Idle();
            }
        }
        protected virtual void Update()
        {
            // Do update logic here.
        }
        protected virtual NetworkVirtualClient CreateNewClient(System.Net.Sockets.Socket sock, int intClientId)
        {
            //**Override in derived class.
            return new NetworkVirtualClient(sock, intClientId);
        }

        private void ListenForConnections()
        {
            if (Listener != null)
                Listener = null;

            Listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, _intReceivePort);
            try
            {
                Listener.Start();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    Globals.Logger.LogWarn("Got an excpetion that we are using more than one address for port " 
                        + _intReceivePort + "  it is not usually preritted.");
                    throw ex;
                }
            }

            Globals.Logger.LogInfo("Client is listening on port:" + _intReceivePort.ToString());
        }
        private void AcceptConnections()
        {
            if (Listener.Pending())
            {
                NetworkVirtualClient st = CreateNewClient(Listener.AcceptSocket(), _intClientIdGenerator++);
                Globals.Logger.LogInfo("Accepted new Connection from " + st.ClientName);
                
                VirtualClients.Add(st);
            }
        }
        private void UpdateClients()
        {
            // Update all clients.
            foreach (NetworkVirtualClient ac in VirtualClients)
            {
                if (ac.IsConnected() == false)
                    DropClient(ac);
                else
                {
                    if (ac.ProcessData())
                        _datLastCommandTime = DateTime.Now;
                }
            }
        }

        private void Idle()
        {
            //Debug so we can see the cmd
            System.Windows.Forms.Application.DoEvents();

            //If we are idle.. then sleep this guy
            if ((DateTime.Now - _datLastCommandTime).TotalSeconds > IdleWait)
                System.Threading.Thread.Sleep(IdleTime);

            // Per Step sleep
            if (IdleStep > 0)
                System.Threading.Thread.Sleep(IdleStep);
        }
        private void RemoveDroppedClients()
        {
            //Update dropped
            foreach (NetworkVirtualClient ac in _objDroppedClients)
            {
                Globals.Logger.LogWarn("Dropping client " + ac.ClientName);
                VirtualClients.Remove(ac);
            }
            _objDroppedClients.Clear();
        }
        private void DropClient(NetworkVirtualClient sc)
        {
            if (!_objDroppedClients.Contains(sc))
                _objDroppedClients.Add(sc);
        }
    }
}
