using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Proteus
{
    public sealed class PacketMakerTcp : PacketMakerBase
    {
        #region Public: Members

        public int LastSendStamp;//for keepalive if needed
        public Socket Socket;

        #endregion

        #region Public: Methods
        public PacketMakerTcp()
        {
        }
        public PacketMakerTcp(Socket theSocket)
        {
            Socket = theSocket;
        }
        public string GetNextPacket(int timeout=100000)
        {
            //Returns next packet in the packet queue.
            //if packet maker is currently building a packet
            // and only thus has packet fragments it returns
            // empty.  

            RecvData(false, timeout);

            return DequeuePacket();
        }
        public void SendPacket(string str, int timeout = 20000)
        {
            SendData(str, timeout);
            LastSendStamp = System.Environment.TickCount;
        }

        #endregion

        #region Private: Methods

        private void SendData(string str, int timeout = 20000)
        {
            byte[] b = MakePacket(str);

            IAsyncResult res = Socket.BeginSend(b, 0, b.Length, System.Net.Sockets.SocketFlags.None, SendCallback, Socket);
            bool success = res.AsyncWaitHandle.WaitOne(timeout, true);
            if (success == false)
                Globals.Logger.LogError("Failed to send data.", true);
        }
        private void SendCallback(IAsyncResult ar)
        {
            System.Net.Sockets.Socket s = (System.Net.Sockets.Socket)ar.AsyncState;
            System.Net.Sockets.SocketError errcode;

            s.EndSend(ar, out errcode);

            if (errcode != System.Net.Sockets.SocketError.Success)
                Globals.Logger.LogError(" SEND GOT ERROR: " + errcode.ToString());

            //??
            int n = 0;
            n++;
        }
        private void RecvData(bool block = false, int iTimeout = 100000)
        {
            //RecvDataAsync(block, iTimeout);

                RecvDataSync2(block, iTimeout);

        }
        static byte[] statbuf_sync = new byte[8192];
        private string RecvDataSync2(bool block = false, int iTimeout = 100000)
        {
            string str = "";
            int recvCount = 0;
            int recvTot = 0;
            string astr;
            while (true)
            {
                while (Socket.Available > 0)
                {
                    recvCount = Socket.Receive(statbuf_sync, 0, statbuf_sync.Length, System.Net.Sockets.SocketFlags.None);
                    astr = System.Text.Encoding.ASCII.GetString(statbuf_sync, 0, recvCount);
                    str += astr;
                    recvTot += recvCount;
                    System.Windows.Forms.Application.DoEvents();
                }

                if (recvTot > 0)
                {
                    //if (!HasFooter(str))
                    //    throw new Exception("invlaid packet 1");
                    if (str.Length == 0)
                        throw new Exception("invlaid packet 2");

                    //if (_objSocket.Available > 0)
                    //    throw new Exception("Socket still had data but we abandoned it.");
                    if (str.Length != recvTot)
                        throw new Exception("Invalid recv count.");

                    AddReceivedDataToPacket(str);

                }

                if (!block)
                    break;
                else if (recvTot > 0)//we got data
                    break;

                System.Windows.Forms.Application.DoEvents();
            }
            return str;
        }

        private void RecvDataAsync(bool block = false, int iTimeout = 100000)
        {
            RecvStateObj st = new RecvStateObj();
            st.sock = Socket;

            System.Net.Sockets.SocketError errCode;

            IAsyncResult res = Socket.BeginReceive(
                st.buf, 
                0, 
                st.buf.Length, 
                System.Net.Sockets.SocketFlags.None, 
                out errCode, 
                RecvCallback, 
                st);
        }
        private void RecvCallback(IAsyncResult ar)
        {
            RecvStateObj stateObj = (RecvStateObj)ar.AsyncState;
            System.Net.Sockets.SocketError errcode;

            int bytesGot = stateObj.sock.EndReceive(ar, out errcode);

            if (errcode != System.Net.Sockets.SocketError.Success)
                Globals.Logger.LogError("RCV GOT ERROR: " + errcode.ToString());

            string buf = System.Text.Encoding.ASCII.GetString(stateObj.buf, 0, bytesGot);

            AddReceivedDataToPacket(buf);
        }

        #endregion


    }
}
