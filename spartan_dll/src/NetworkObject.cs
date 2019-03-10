using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class NetworkObject
    {
        public string ComputerName { get; set; }
        public NetworkObjectState NetworkObjectState { get; set; }  // If there was a network error &c that we cannt connect to him

    }
}
C:\p4\dev\proteus\spartan_dll\src\NetworkObject.cs