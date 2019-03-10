using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Proteus;
using Spartan;

namespace Spartan
{
    class CommandProgram
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;

            SpartanCommandLine cmdLine = new SpartanCommandLine();
            cmdLine.Execute(args);
        }
    }
}
