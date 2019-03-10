using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Spartan
{
    //Used by build gui and coordinator to define agents.
    public class AgentInfo
    {
        public string Name;
        private Object _objIsConnectedLockObject = new Object();
        private bool _blnIsConnected = false;
        public bool IsConnected
        {
            get
            {
                lock (_objIsConnectedLockObject)
                {
                    return _blnIsConnected;
                }
            }
            set
            {
                lock (_objIsConnectedLockObject)
                {
                    _blnIsConnected = value;

                    if (_blnIsConnected == false)
                    {
                        foreach (AgentCpuInfo aci in Cpus)
                            aci.WorkingFileName = "";
                    }
                }
            }
        }
        public List<AgentCpuInfo> Cpus = new List<AgentCpuInfo>();

        public string Serialize()
        {
            string ret;
            string strCpuSerialized = string.Empty;

            foreach (AgentCpuInfo aci in Cpus)
                strCpuSerialized += aci.Serialize();

            ret = NetworkUtils.PackString(Name)
                + NetworkUtils.PackInt(System.Convert.ToInt32(IsConnected))
                + NetworkUtils.PackInt(Cpus.Count)
                + strCpuSerialized
                ;
            return ret;
        }
        public void Deserialize(ref string packet)
        {
            Cpus = new List<AgentCpuInfo>();

            Name = NetworkUtils.UnpackString(ref packet);
            _blnIsConnected = System.Convert.ToBoolean(NetworkUtils.UnpackInt(ref packet));
            int cpuCount = NetworkUtils.UnpackInt(ref packet);
            
            for(int icpu=0; icpu<cpuCount; icpu++)
            {
                AgentCpuInfo inf = new AgentCpuInfo();
                inf.Deserialize(ref packet);
                Cpus.Add(inf);
            }
        }
    }
}
