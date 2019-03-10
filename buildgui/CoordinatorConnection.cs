using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;
namespace BuildGui
{
    public class CoordinatorConnection
    {
        PacketMakerTcp _objPacketMaker = new PacketMakerTcp();
        public System.Net.Sockets.Socket Socket;

        public CoordinatorConnection()
        {
           
        }

    }
}
