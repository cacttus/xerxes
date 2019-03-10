using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public static class Proteus
    {
        public static bool IsInitialized = false;
        
        public static void Initialize(string LogFileName, string LogFileDir = Globals.DefaultLogDirectory)
        {
            Globals.InitializeGlobals(LogFileName, LogFileDir);
            Proteus.IsInitialized = true;
        }
    }
}
