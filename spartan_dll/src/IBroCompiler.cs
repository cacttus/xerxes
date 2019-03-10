using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;

namespace Spartan
{
    public abstract class IBroCompilerObject
    {
        protected String _strPrep;

        public IBroCompilerObject( String prep)
        {
            _strPrep = prep;
        }
        protected void LogInfo(string text)
        {
            Globals.Logger.LogInfo(_strPrep + " " + text);
        }
        protected void LogWarn(string text)
        {
            Globals.Logger.LogWarn(_strPrep + " " + text);

        }
        protected void LogError(string text, bool bThrow = false)
        {
            Globals.Logger.LogError(_strPrep + " " + text);
            if (bThrow)
                throw new Exception(_strPrep + " " + text);
        }
        public abstract void Run();
    }
}
