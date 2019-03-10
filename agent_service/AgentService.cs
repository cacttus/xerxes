using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Proteus;
using Spartan;

namespace AgentService
{
    public partial class AgentService : ServiceBase
    {
        private System.Threading.Thread _objThread;
        
        public AgentService()
        {
            InitializeComponent();
        }

        //void SetWindowsServiceCreds(string serviceName, string username, string password)
        //{
        //    string objPath = string.Format("Win32_Service.Name='{0}'", serviceName);
        //    using (System.Management.ManagementObject service = 
        //        new System.Management.ManagementObject(new System.Management.ManagementPath(objPath)))
        //    {
        //        object[] wmiParams = new object[10];

        //        wmiParams[6] = username;
        //        wmiParams[7] = password;
        //        service.InvokeMethod("Change", wmiParams);
        //    }

        //}
        protected override void OnStart(string[] x)
        {
            //WORK FUKCER
            System.IO.Directory.SetCurrentDirectory(
                System.AppDomain.CurrentDomain.BaseDirectory
            );
            //System.Reflection.Assembly.GetEntryAssembly().Location

            string[] args = System.IO.File.ReadAllLines(".\\AgentServiceArguments.txt");
            

            // Let this throw if it fails.
           // SetWindowsServiceCreds(SpartanGlobals.AgentServiceName, username, password);

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            Init(args.ToList());

            _objThread = new System.Threading.Thread(Process);
            _objThread.Start();

        }
        protected override void OnStop()
        {
            _objThread.Abort();
            Kill();
        }

        #region PRIVATE METHODS

        private void Init(List<string> args)
        {
            try
            {
                SpartanGlobals.InitializeGlobals(args);
                Globals.Logger.LogInfo("Setting Vc Environment Vars (for process only)");
                MsvcUtils.SetMsvcVars(11, 7);
            }
            catch (Exception ex)
            {
                //if logging ffails allow exception to pass through and show in event viewer
                Globals.Logger.LogError("Could not initialize coordinator/agent:" + ex);
                throw ex;
            }
            Globals.Logger.LogError("Initialization successful.");
        }
        private void Process()
        {
            IBroCompilerObject bc = null;
            try
            {
                SpartanGlobals.ValidateForBuild();

                if (SpartanGlobals.ProgramFunction == ProgramFunction.Coordinator)
                    bc = new Coordinator();
                else if (SpartanGlobals.ProgramFunction == ProgramFunction.Agent)
                    bc = new AgentServer();
                else
                    throw new Exception("Please supply argument either agent,/a or coordinator,/c");

                Globals.Logger.LogError("Starting main program");

                bc.Run();
            }
            catch (Exception ex)
            {
                BuildUtils.ShowErrorMessage("There was an error running the build.\n\nTechnical information:\n" + ex.ToString());
                throw ex;
            }

        }
        private void Kill()
        {
            Globals.Logger.LogInfo("");
            Globals.Logger.LogInfo("Exiting.");
            Globals.Logger.LogInfo("...Killing all Cmd processes");
            System.Diagnostics.Process[] processes;
            processes = System.Diagnostics.Process.GetProcessesByName("cmd");
            foreach (System.Diagnostics.Process process in processes)
                process.Kill();
        }

        #endregion

    }
}
