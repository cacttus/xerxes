/*
 * Romulus Makefile System
 * 
 *  Derek Page
 *  20150701
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Romulus
{
    public class Program
    {
        static void Main(string[] args)
        {
            Proteus.Proteus.Initialize("romulus.log");
            Globals.Logger.LogInfo("Initializing..");
            BuildConfig.ParseArgs(args.ToList());
            BuildConfig.LoadConfig();

            try
            {
                if (String.IsNullOrEmpty(BuildConfig.GlobalBuildConfiguration.BuildPlatform)
                    || String.IsNullOrEmpty(BuildConfig.GlobalBuildConfiguration.ConfigurationName))
                    throw new Exception("Build platform or configuration was not specified. Makefile cannot generate wihout a configuration which matches at least one project configuration. Specify these with /cn\"..\" /cp\"..\" switches.");


                MakeManager mm = new MakeManager();
                mm.LoadBuildTargets();
                mm.Make();
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError(ex.ToString());
                System.Diagnostics.Debugger.Break();
            }

            Globals.Logger.LogInfo("Romulus makefile generation complete.");
        }



    }
}
