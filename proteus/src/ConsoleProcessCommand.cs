using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class ConsoleProcessCommand
    {
        public ConsoleProcessCommandType CommandType { get; set; }
        public string CommandText { get; set; }
        public bool Synchronous;
        public int TimeoutInMilliseconds;
        public ConsoleProcessCommand(string text, ConsoleProcessCommandType type, bool sync = false, int timeoutMs=-1)
        {
            CommandType = type;
            CommandText = text;
            Synchronous = sync;
            TimeoutInMilliseconds = timeoutMs;
        }
    }
}
