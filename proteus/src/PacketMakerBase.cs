using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class PacketMakerBase
    {
        #region Protected: Members
        protected class RecvStateObj
        {
            public byte[] buf = new byte[2048];
            public System.Net.Sockets.Socket sock;
        }
        protected class WorkingPacket
        {
            public Object _objLockObject = new Object();
            public int _intLastPacketSize = -1;
            public string _strRecvBuffer = "";
        }

        #endregion
        #region Protected: Members

        protected static int PacketCrcSizeBytes = 4;
       // protected static string PacketTerminator = "~END\0\0\0\0";
        protected WorkingPacket _objWorkingPacket = new WorkingPacket();
        protected Queue<string> _lstPackets = new Queue<string>();
        protected Object _objPacketsLockObject = new Object();

        #endregion

        public string DequeuePacket()
        {
            string st;
            lock (_objPacketsLockObject)
            {
                if (_lstPackets.Count == 0)
                    return "";
                st = _lstPackets.Dequeue();
            }
            return st;
        }
        public static byte[] MakePacket(string strPacket)
        {
            string crc = NetworkUtils.PackCrc32(strPacket);// Packs the CRC as a typically packed UINT
           
            //*Add Footer.  Packet size does not count the 12 integer bytes prepended to the packet
            string packSize = NetworkUtils.PackInt(strPacket.Length + crc.Length);
           
            //[packsize][ [packet] [crc] ]

            string finalPacket = packSize + strPacket + crc;

            byte[] ret = System.Text.Encoding.ASCII.GetBytes(finalPacket);

            return ret;
        }
        protected void AddReceivedDataToPacket(string buf)
        {
            lock (_objWorkingPacket._objLockObject)
            {
                //loop through buffer
                while (buf.Length > 0)//for (int ch = 0; ch < buf.Length; ch++)
                {
                    //check for new packet
                    if (_objWorkingPacket._intLastPacketSize == -1)
                    {
                        _objWorkingPacket._intLastPacketSize = NetworkUtils.UnpackInt(ref buf);
                    }

                    // ** Loop byte for byte and check the packets.
                    if (CheckAddPacket() == false)
                    {
                        if (buf.Length > 0)
                        {
                            _objWorkingPacket._strRecvBuffer += buf[0];
                            buf = buf.Substring(1);
                        }
                    }
                }
                //This here is critical - we must check after the loop because the count may equal - the loop exits and 
                // we get a stuck packet.
                CheckAddPacket();
            }
        }
        private bool CheckAddPacket()
        {
            //Check to see if the buffer has a packet.
            string strPacket;

            if (_objWorkingPacket._strRecvBuffer.Length != _objWorkingPacket._intLastPacketSize)
                return false;

            strPacket = _objWorkingPacket._strRecvBuffer;

            //Reset packet buffer
            _objWorkingPacket._strRecvBuffer = "";
            _objWorkingPacket._intLastPacketSize = -1;

            // The packet had no footer.
            if ((strPacket.Length - PacketCrcSizeBytes) < 0)
            {
                Globals.Logger.LogError("RCV INvalid - Packet had no footer:" + strPacket);
                return false;
            }

            if (!NetworkUtils.UnpackAndCheckCrc32(ref strPacket))
            {
                Globals.Logger.LogError("Packet CRC32 Failed: " + strPacket);
                return false;
            }

            // **All good, enqueue packet
            lock (_objPacketsLockObject)
            {
                _lstPackets.Enqueue(strPacket);
            }
            return true;
        }
    }
}
