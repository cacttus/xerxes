using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class Logger
    {
        private const int NumDaysToKeepOldLogs = 4;
        private static Object _objLoggerLockObject = new Object();

        public String LogFilePath { get; private set; }
        public string LogDir { get; private set; }//20151017 changed tehse to private.

        public Logger(string logFileName, string LogDirectory, bool moveOldLogs = true, bool cleanOldLogs = true)
        {
            //path must be rooted or else we end up logging all over the place.
            if (System.IO.Path.IsPathRooted(LogDirectory) == false)
                LogDirectory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), LogDirectory);
            LogDir = LogDirectory;
            LogFilePath = System.IO.Path.Combine(LogDirectory, logFileName);

            string output = String.Empty;

            FileUtils.MoveAndCleanLogs(LogFilePath, NumDaysToKeepOldLogs, "log_arch", true);

            if (!System.IO.Directory.Exists(LogDir))
                System.IO.Directory.CreateDirectory(LogDir);

            if (!System.IO.File.Exists(LogFilePath))
            {
                using(System.IO.FileStream fs = System.IO.File.Create(LogFilePath) )
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            LogInfo(output);
            LogInfo("Logfile Created at " + LogFilePath);
        }

        #region Public: Methods
       
        public void LogInfo(string text)
        {
            lock (_objLoggerLockObject)
            {
                SyncWriteFile(GetHeader() + " " + text + "\r\n");

                System.Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[DEBUG] " + text);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public void LogWarn(string text)
        {
            lock (_objLoggerLockObject)
            {
                SyncWriteFile(GetHeader() + " " + text + "\r\n");

                System.Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[WARN ] " + text);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public void LogError(string text, bool bThrow = false, bool bWriteToEventLog = false)
        {
            lock (_objLoggerLockObject)
            {
                SyncWriteFile(GetHeader() + " " + text + "\r\n");

                System.Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] " + text);
                System.Console.ForegroundColor = ConsoleColor.White;

                if(bWriteToEventLog==true)
                    System.Diagnostics.EventLog.WriteEntry(Globals.ProgramName, 
                        "Error",
                        System.Diagnostics.EventLogEntryType.Error, 
                        0);

                if (bThrow == true)
                    throw new Exception(text);
            }
        }

        #endregion

        #region Private: Methods

        private string GetHeader()
        {
            return BuildUtils.DateTimeString(DateTime.Now, true);
        }
        private void SyncWriteFile(string text)
        {
            int tA = System.Environment.TickCount;
            while (true)
            {
                int tB = System.Environment.TickCount;
                if ((tB - tA)> 2000)
                {
                    Console.WriteLine(" Error Could not log to file - file is probably locked.. took more than 2s.");
                    return;
                }

                try
                {
                    System.IO.File.AppendAllText(LogFilePath, text);
                    return;
                }
                catch (System.IO.IOException) 
                {
                    int n = 0;
                    n++;
                }
                //System.Threading.Thread.Sleep(1);
                System.Windows.Forms.Application.DoEvents();
            }
          
          
        }

        
        #endregion
    }
}
