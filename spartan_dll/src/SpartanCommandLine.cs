using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Spartan
{
    public class SpartanCommandLine
    {
        IBroCompilerObject _objCompilerObject = null;

        public void Execute(string[] args)
        {
            Init(args);
            Run();
            Destroy();
        }

        private void Init(string[] args)
        {
            try
            {
                SpartanGlobals.InitializeGlobals(args.ToList());
                Globals.Logger.LogInfo("Setting Vc Environment Vars (for process only)");
                MsvcUtils.SetMsvcVars(11, 7);
            }
            catch (Exception ex)
            {
                try
                {
                    Globals.Logger.LogError("Could not initialize coordinator/agent:" + ex);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Could not log error to file..:" + ex2);
                    Console.WriteLine("Could not initialize coordinator/agent:" + ex);
                }
                Console.ReadKey();
            }
        }
        private void Run()
        {
            try
            {
                SpartanGlobals.ValidateForBuild();

                if (SpartanGlobals.ProgramFunction == ProgramFunction.Coordinator)
                    _objCompilerObject = new Coordinator();
                else if (SpartanGlobals.ProgramFunction == ProgramFunction.Agent)
                    _objCompilerObject = new AgentServer();
                else
                    throw new Exception("Please supply argument either agent,/a or coordinator,/c");

                _objCompilerObject.Run();
            }
            catch (Exception e)
            {
                BuildUtils.ShowErrorMessage("There was an error running the build.\n\nTechnical information:\n" + e.ToString());
            }
        }
        private void Destroy()
        {
            Globals.Logger.LogInfo("");
            Globals.Logger.LogInfo("Exiting.");


            Globals.Logger.LogInfo("...Killing all Cmd processes");
            System.Diagnostics.Process[] processes;
            processes = System.Diagnostics.Process.GetProcessesByName("cmd");
            foreach (System.Diagnostics.Process process in processes)
                process.Kill();

            Globals.Logger.LogInfo("...Cleanup complete");
        }

    }
}
