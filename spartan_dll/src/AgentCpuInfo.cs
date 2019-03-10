using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Spartan
{
    public class AgentCpuInfo
    {
        public int CpuId;
        public string WorkingFileName; // name of file being compiled.
        public string WorkingCommand; //full command string
        public DateTime FileSendTime; //actually, the time we invoked the file on the agent.

        public string FileUpTime;//Don't pack this.  Easier to show time as a string so we can tell if minvalue.

        public string Serialize()
        {
            string ret;
            ret = NetworkUtils.PackInt(CpuId)
                + NetworkUtils.PackString(WorkingFileName)
                + NetworkUtils.PackString(WorkingCommand)
                + NetworkUtils.PackDateTime(FileSendTime)
                ;
            return ret;
        }
        public void Deserialize(ref string str)
        {
            CpuId = NetworkUtils.UnpackInt(ref str);
            if (CpuId > 100)
                throw new Exception("Parse error. CPU ID is greater than 100.");

            WorkingFileName = NetworkUtils.UnpackString(ref str);
            WorkingCommand = NetworkUtils.UnpackString(ref str);
            FileSendTime = NetworkUtils.UnpackDateTime(ref str);
        }
    }
}
