using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class ProcessUtils
    {



        public static System.Diagnostics.Process StartProcessSafe(string strPath, bool blnWaitForStart = true, int intMaxWaitTimeMilliseconds = 2000)
        {
            System.Diagnostics.Process objProcess;
            objProcess = new System.Diagnostics.Process();
            objProcess.StartInfo.FileName = strPath; //not the full application path
            objProcess.Start();

            // Wait for process to start
            if(blnWaitForStart)
            {
                int tA = System.Environment.TickCount;
                int tB = System.Environment.TickCount;
                while ((tB - tA < intMaxWaitTimeMilliseconds) && !objProcess.HasExited) 
                {
                    System.Diagnostics.Process process = null;
                    tB = System.Environment.TickCount;
                    try
                    {
                         process = System.Diagnostics.Process.GetProcessById(objProcess.Id);
                    }
                    catch(Exception ex)
                    {
                        //Continue catching exception until process is available
                    }

                    if (process != null)
                        break;

                    System.Windows.Forms.Application.DoEvents();
                }
            }
            return objProcess;
        }
    }
}
