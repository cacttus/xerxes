using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace BuildGui
{
    public class BuildMonitorGlobals
    {
        public static string AppName = "Romulus Build Utility";

        public const int ProgramCommandUdpPortRecv = 58489;// ** see also spartan globals.
        
        public class Commands
        {
            public const string Build = @"/build";
            public const string CancelBuild = @"/cancelbuild";
        }
    }
}
