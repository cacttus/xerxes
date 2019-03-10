using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;
namespace Spartan
{
    public class CompilerOutput
    {
        private List<string> OutputLines = new List<string>();
        private string _strFileLocation;
        private string _strFileDir;
        private int _intBuildId;
        private const string _cstrLogArchFolderName = "build_log_arch";

        public CompilerOutput(string location, int buildId)
        {
            _intBuildId = buildId;
            _strFileDir = location;

            if(!System.IO.Directory.Exists(_strFileDir))
                System.IO.Directory.CreateDirectory(_strFileDir);

            string fileName = BuildUtils.GetBuildLogFileName(buildId);
            _strFileLocation = BuildUtils.GetBuildLogFilePath(buildId);

            //Automatically moves old log to new folder and cleans all other logs.
            FileUtils.MoveAndCleanLogs(_strFileLocation, 4, _cstrLogArchFolderName, false);

            if (!System.IO.File.Exists(_strFileLocation))
                System.IO.File.CreateText(_strFileLocation);

        }
        public void AddLine(string line)
        {
            line =  line.Trim() + "\n";
            OutputLines.Add(line);
            SyncWriteFile(line);
        }
        private void SyncWriteFile(string text)
        {
            int tA = System.Environment.TickCount;
            while (true)
            {
                int tB = System.Environment.TickCount;
                if ((tB - tA) > 2000)
                {
                    Console.WriteLine(" Error Could not log to file - file is probably locked.. took more than 2s.");
                    return;
                }

                try
                {
                    //This exception will throw often. Ignore it.
                    //This exception will throw often. Ignore it.
                    //This exception will throw often. Ignore it.
                    System.IO.File.AppendAllText(_strFileLocation, text);
                    return;
                }
                catch (System.IO.IOException )
                {
                    //This exception will throw often. Ignore it.
                    //This exception will throw often. Ignore it.
                    //This exception will throw often. Ignore it.
                    int n = 0;
                    n++;
                }
                //System.Threading.Thread.Sleep(1);
                System.Windows.Forms.Application.DoEvents();
            }


        }
    }
}

