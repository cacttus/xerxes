using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public static class BuildConfig
    {
        public static bool BuildConfigInitialized = false;

        public static bool UseDependencyCache = false;

        // Build Intervals in milliseconds
        public static int BuildInterval                  = 100;
        public static int FileStatusQueryInterval        = 500;
        public static int ProcessorQueryTimeout          = 120000; // this should probably be -1 or disabled or way huge.  This is the max time a file can compile.
        public static int CompilerMaxErrorLimit          = 1;
        public static bool BuildProcessesAreAttachedToUI = false; // if true we kill the cmds when the UI disappears.

        // Input arguments
        //public static bool UseGlobalUncPaths = false;   //  Whether we are going to use global paths like \\dp0001\...  This is required for the dist build to work.

        public static BuildPlatform BuildPlatform;
        public static BuildConfiguration GlobalBuildConfiguration = new BuildConfiguration();
        public static string DefaultUncRoot = "C:\\"; // appended to UNC paths if no root is specified. such as \\machine\\C$
        public static string BuildConfigFileName = "build.cfg";

        // Master path where build files are stored.  This is passed in as a parameter to all programs
        public static string BuildConfigFilePath; // Location of build config file.

        // UNC capable paths
        public static string CompilerBinPath { get; set; }
        public static string BranchRootDirectory { get; set; }

        // Relative Paths
        public static string GlobalBranchTempPath { get; set; }//Local to branch, global to all projects
        public static string GlobalBranchLibPath { get; set; }//Local to branch, global to all projects
        public static string GlobalBranchBinPath { get; set; }//Local to branch, global to all projects

        public static string BatchFileName { get; set; } //filename only
        public static string DepFileName { get; set; } //filename only
        public static string BuildLogFileName { get; set; } //filename only
        public static string ProjectsFileName { get; set;  }
        public static string MachineName = System.Environment.MachineName;
        
        public static string CoordinatorName;

        public static string GlobalCompileOutputDirectory
        {
            get
            {
                return GlobalRootedTempPath;
            }
        }
        public static string GlobalRootedBinPath
        {
            get
            {
                return
                System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        BuildConfig.BranchRootDirectory,
                        BuildConfig.GlobalBranchBinPath
                    ),
                    GlobalBuildConfiguration.GetPathName()
                );
            }
            private set { }
        }
        public static string GlobalRootedLibPath
        {
            get
            {
                return
                System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        BuildConfig.BranchRootDirectory,
                        BuildConfig.GlobalBranchLibPath
                    ),
                    GlobalBuildConfiguration.GetPathName()
                );
            }
            private set { }
        }
        public static string GlobalRootedTempPath
        {
            get
            {
                string pn = GlobalBuildConfiguration.GetPathName();
                return
                System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        BuildConfig.BranchRootDirectory,
                        BuildConfig.GlobalBranchTempPath
                    ),
                    pn
                );
            }
            private set { }
        }

        public static string GetCacheDirectory()
        {
            if (UseDependencyCache == true)
                return GlobalRootedTempPath;
            else
                return string.Empty;
        }

        private static bool IsAdministrator()
        {
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        public static string GetSpartanPathRooted()
        {
            return BuildConfigFilePath;
        }
        public static void LoadConfig()
        {
            if (BuildConfigInitialized==true)
            {
                Globals.Logger.LogInfo("BuildConfig was already initialized..returning.");
                return;
            }

            if (string.IsNullOrEmpty(BuildConfigFilePath))
                Globals.Logger.LogError("Failed to load config.  the /o switch is required which tells the build where to find your project config file. Ex. /o\".\\dir\\build.cfg\".  To use a coordinator configuration specify a UNC path with \\CoordinatorMachineName\\c$\\.. ", true);

            string cfgPath = System.IO.Path.Combine(BuildConfigFilePath, BuildConfigFileName);

            if (!System.IO.File.Exists(cfgPath))
            {
                Globals.Logger.LogWarn("First file exist failed");
                System.IO.FileInfo fi = new System.IO.FileInfo(cfgPath);
                fi.Refresh();
                if (!fi.Exists)
                {
                    Globals.Logger.LogError("IsAdmin = " 
                        + IsAdministrator().ToString() 
                        + "Could not find build config file '"
                        + BuildConfigFileName
                        + "' in directory '"
                        + BuildConfigFilePath
                        + "' ( " + cfgPath + ").  Engine will still try to continue to load file\n\n ", false);
                }
            }

            string[] lines = System.IO.File.ReadAllLines(cfgPath);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                ParseConfigLine(line);
            }
          //  if (UseGlobalUncPaths == true)
           // {
                CompilerBinPath = MakeUncRootToCoordMachine(CompilerBinPath);
                BranchRootDirectory = MakeUncRootToCoordMachine(BranchRootDirectory);

                if (!System.IO.Directory.Exists(BranchRootDirectory))
                    throw new Exception("Branch root directory '" + BranchRootDirectory + "' does not exist.  Please update branch root in 'build.cfg' in the build files folder. Note: The root must NOT be a UNC path if UNC is enabled in the build.cfg. ");
          //  }
          //  else
         //       Globals.Logger.LogWarn("*Warning:UNC paths were not specified. Distributed build will fail. Set UseUncRootPath=\"true\" in build.cfg to use UNC paths.");

            BuildConfigInitialized = true;
        }
        private static void ParseConfigLine(string inline)
        {
            string line = BuildUtils.ParseEatComment(inline);
            if (string.IsNullOrEmpty(line))
                return;

            string[] values = BuildUtils.SplitValues(line,'=');;
            if(values.Length!=2)
            {
                Globals.Logger.LogWarn("Invalid value '" + line + "' encountered in config file. Ignoring.");
                return;
            }

            string key = values[0].Trim();
            string value = values[1].Trim();
            value = BuildUtils.Dequote(value);

            //b root / global config / src path
            switch(key.ToLower())
            {
                case "vcbinpath": CompilerBinPath = value; break;
                case "branchrootdirectory": BranchRootDirectory = value; break;
                case "tempdir": GlobalBranchTempPath = value; break;
                case "libdir": GlobalBranchLibPath = value; break;
                case "bindir": GlobalBranchBinPath = value; break;
                case "batchfilename": BatchFileName = value; break;
                case "depfilename": DepFileName = value; break;
                case "buildlogfilename": BuildLogFileName = value; break;
                case "projectsfilename": ProjectsFileName = value; break;
                //case "useuncrootpath": UseGlobalUncPaths = (value.ToLower().Trim() == "true") ? true : false; break;
                case "defaultuncroot": DefaultUncRoot = value; break;
                case "buildinterval": BuildInterval = System.Convert.ToInt32(value); break;
                case "filestatusqueryinterval": FileStatusQueryInterval = System.Convert.ToInt32(value); break;
                case "processorquerytimeout": ProcessorQueryTimeout = System.Convert.ToInt32(value); break;
                case "usedependencycache": UseDependencyCache = (value.ToLower()=="yes") || (System.Convert.ToBoolean(value)==true); break;
                default:
                    Globals.Logger.LogWarn("Invalid value '" + line + "' encountered in config file. Ignoring.");
                    break;
            }
        }

        public static string GetMakeAndBatchFileDirectory()
        {
            return BuildConfig.GlobalRootedTempPath;
        }
        public static string GetMakeFilePath()
        {
            return System.IO.Path.Combine(BuildConfig.GlobalRootedTempPath, BuildConfig.BatchFileName);
        }
        public static string GetBatchFilePath()
        {
            return System.IO.Path.Combine(BuildConfig.GlobalRootedTempPath, BuildConfig.DepFileName);
        }

        public static string MakeUncRootToCoordMachine(string path)
        {
            // Adds a path root if none specified, or
            // adds the \\ unc path.

            return FileUtils.MakeUncRoot(BuildConfig.CoordinatorName, path, DefaultUncRoot);

            //if (path[0] == '\\' && path[1] == '\\')
            //    return path;    // assume path is valid UNC root.

            //if (path[0] == '\\' || path[0] == '/')
            //    path = path.Substring(1);

            //int rlen = System.IO.Path.GetPathRoot(path).Length;
            //string root = path.Substring(0, rlen);
            //if (string.IsNullOrEmpty(root))
            //    root = DefaultUncRoot;
            //path = path.Substring(rlen, path.Length - rlen);

            //if (UseGlobalUncPaths == true)
            //{
            //    //transform to unc path.
            //    root = "\\\\" + BuildConfig.CoordinatorName + "\\" + root.Replace(':', '$');
            //}


            //return System.IO.Path.Combine(root, path);
        }
        public static void ParseArgs(List<string> args)
        {
            string temp;
            foreach (string arg in args)
            {
                temp = "";

                if (StringUtils.ParseCmdArg(arg, BuildFlags.ConfigPlatform, ref temp))
                {
                    GlobalBuildConfiguration.BuildPlatform = temp;
                }
                else if (StringUtils.ParseCmdArg(arg, BuildFlags.ConfigName, ref temp))
                {
                    GlobalBuildConfiguration.ConfigurationName = temp;
                }
                else if (StringUtils.ParseCmdArg(arg, BuildFlags.BuildDir, ref temp))
                {
                    BuildConfigFilePath = temp;
                    CoordinatorName = ParseCoordMachine(BuildConfigFilePath);
                    if (String.IsNullOrEmpty(CoordinatorName))
                        throw new Exception(@"Coordinator name was not found in the /o switch while parsing the UNC path.\n" +
                            @" Please make sure that the build path is a UNC path, for example '\\MyPCName\C$\dev\build'.  (arg = " + arg + ")");
                }
                else if(StringUtils.ParseCmdArg(arg, BuildFlags.MaxErrorLimit, ref temp))
                {
                    CompilerMaxErrorLimit = Convert.ToInt32(temp);
                    if (CompilerMaxErrorLimit <= 0)
                    {
                        Globals.Logger.LogWarn("[BuildConfig] invalid error limit found:" + temp + ", setting to default of 1.");
                        CompilerMaxErrorLimit = 1;
                    }
                }
                else if(StringUtils.ParseCmdArg(arg, BuildFlags.AttachBuildProcessesToUI, ref temp))
                {
                    BuildProcessesAreAttachedToUI = Convert.ToBoolean(temp);
                }
                else 
                    Globals.Logger.LogWarn("[BuildConfig] Possible unrecognized switch " + arg + " not recognized.");

            }

        }
        public static string ParseCoordMachine(string UncPath)
        {
            Uri uri = new Uri(UncPath);
            return uri.Host;
        }
    }
}
