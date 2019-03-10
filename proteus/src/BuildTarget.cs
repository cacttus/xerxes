using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{

    public class BuildTarget
    {
        public bool InheritDefaults = false; // whether we are to inherit the default project, if specified.

        public BuildTargetType? TargetType;
        public BuildConfiguration BuildConfiguration = null; //created by the manager.
        public bool? RecursiveSearchSourceDirectory = null;

        public string TargetName = ""; // project name
       
        public string ExeName;
        public string PgdName;
        public string PdbName;
        public string LibName;

        public string OutputPath = "";
        public string Platform = ""; //eg. Win32  Do not use - use BuildCOnfiguration class
        public string ConfigurationName = "";//eg. Debug Do not use - use BuildCOnfiguration class

        public List<SourceFile> SourceFiles = new List<SourceFile>();//CPP /C files **Note: these are ALL object files in the project not just the ones we build.

        public List<string> ProjectSourceDirectories = new List<string>(); // the source root for the project. acct game audio etc.
        public List<string> CompilerDefines = new List<string>();
        public List<string> AdditionalIncludeDirectories = new List<string>();
        public List<string> AdditionalLibraryDirectories = new List<string>();
        public List<string> AdditionalDependencies = new List<string>();
        public List<string> ProjectDependencies = new List<string>(); // Strings - these are managed by the manager don't refernce.
        public List<string> ObjectFileExtensions = new List<string>();
        public List<string> CompilerFlags = new List<string>();
        public List<string> LinkerFlags = new List<string>();
        public List<string> LibrarianFlags = new List<string>();

        public BuildStep TargetStep;    // Used only during makefile creation.  Do not use.

        public string ProjectOverrideDirBin = string.Empty;
        public string ProjectOverrideDirLib = string.Empty;

        public string ProjectRootedTempPath
        {
            get
            {
                return System.IO.Path.Combine(BuildConfig.GlobalRootedTempPath, TargetName);
            }
            private set { }
        }
        public string ProjectRootedBinPath
        {
            get
            {
                if (ProjectOverrideDirBin != null)
                    return System.IO.Path.Combine(
                        System.IO.Path.Combine(
                            BuildConfig.BranchRootDirectory,
                            ProjectOverrideDirBin.ToString()
                        )
                    );
                else
                    return System.IO.Path.Combine(BuildConfig.GlobalRootedBinPath, TargetName);
            }
            private set { }
        }
        public string ProjectRootedLibPath
        {
            get
            {
                if (ProjectOverrideDirLib != null)
                    return System.IO.Path.Combine(
                        System.IO.Path.Combine(
                            BuildConfig.BranchRootDirectory,
                            ProjectOverrideDirLib.ToString()
                        )
                    );
                else
                    return System.IO.Path.Combine(BuildConfig.GlobalRootedLibPath, TargetName);

            }
            private set { }
        }

        public BuildTarget()
        {

        }
        public string GetOutputName()
        {
            if (TargetType == null)
                return "";
            else if (TargetType.Value == BuildTargetType.Library)
                return LibName;
            else if (TargetType.Value == BuildTargetType.Executable)
                return ExeName;
            else
                throw new NotImplementedException();
           
        }
        public void AddFrom(BuildTarget other)
        {
            if (other.TargetType != null)
                if (TargetType == null)
                    TargetType = other.TargetType;

            if (other.BuildConfiguration != null)
                if (BuildConfiguration == null)
                    BuildConfiguration = other.BuildConfiguration;

            if (String.IsNullOrEmpty(other.ProjectOverrideDirBin) == false)
                if (String.IsNullOrEmpty(ProjectOverrideDirBin))
                    ProjectOverrideDirBin = other.ProjectOverrideDirBin;

            if (String.IsNullOrEmpty(other.ProjectOverrideDirLib) == false)
                if (String.IsNullOrEmpty(ProjectOverrideDirLib))
                    ProjectOverrideDirLib = other.ProjectOverrideDirLib;
            
            if (other.RecursiveSearchSourceDirectory != null)
                if (RecursiveSearchSourceDirectory == null)
                    RecursiveSearchSourceDirectory = other.RecursiveSearchSourceDirectory;

            if (String.IsNullOrEmpty(other.Platform) == false)
                if (String.IsNullOrEmpty(Platform))
                    Platform = other.Platform;
            if (String.IsNullOrEmpty(other.ConfigurationName) == false)
                if (String.IsNullOrEmpty(ConfigurationName))
                    ConfigurationName = other.ConfigurationName;
            if (String.IsNullOrEmpty(other.ExeName) == false)
                if (String.IsNullOrEmpty(ExeName))
                    ExeName = other.ExeName;
            if (String.IsNullOrEmpty(other.PgdName) == false)
                if (String.IsNullOrEmpty(PgdName))
                    PgdName = other.PgdName;
            if (String.IsNullOrEmpty(other.PdbName) == false)
                if (String.IsNullOrEmpty(PdbName))
                    PdbName = other.PdbName;
            if (String.IsNullOrEmpty(other.LibName) == false)
                if (String.IsNullOrEmpty(LibName))
                    LibName = other.LibName;
            if (String.IsNullOrEmpty(other.OutputPath) == false)
                if (String.IsNullOrEmpty(OutputPath))
                    OutputPath = other.OutputPath;

            foreach (string d in other.ProjectSourceDirectories)
                ProjectSourceDirectories.Add(d);
            foreach (string d in other.CompilerDefines)
                CompilerDefines.Add(d);
            foreach (string d in other.AdditionalIncludeDirectories)
                AdditionalIncludeDirectories.Add(d);
            foreach (string d in other.AdditionalLibraryDirectories)
                AdditionalLibraryDirectories.Add(d);
            foreach (string d in other.AdditionalDependencies)
                AdditionalDependencies.Add(d);
            foreach (string d in other.ObjectFileExtensions)
                ObjectFileExtensions.Add(d);
            foreach (string d in other.CompilerDefines)
                CompilerDefines.Add(d);
            foreach (string d in other.ProjectDependencies)
                ProjectDependencies.Add(d);
            foreach (string d in other.CompilerFlags)
                CompilerFlags.Add(d);
            foreach (string d in other.LinkerFlags)
                LinkerFlags.Add(d);
            foreach (string d in other.LibrarianFlags)
                LibrarianFlags.Add(d);
        }
        public List<string> GatherMkdirs()
        {
            GatherSource();

            List<string> ret = new List<string>();
            foreach (SourceFile sf in SourceFiles)
            {
                string dir = System.IO.Path.GetDirectoryName(GetObjectFilePath(sf));
                if(!ret.Contains(dir))
                    ret.Add(dir);

            }
            ret.Add(ProjectRootedBinPath);
            ret.Add(ProjectRootedLibPath);
            ret.Add(ProjectRootedTempPath);
            return ret;
        }
        /// <summary>
        /// Add the object file names to the given project based on whether
        /// they are missing from the build cache
        /// </summary>
        /// <param name="bc"></param>
        public void GatherSource()
        {
            SourceFiles = new List<SourceFile>();

            foreach (string psd in ProjectSourceDirectories)
            {
                if (String.IsNullOrEmpty(psd))
                {
                    Globals.Logger.LogWarn("Project source directory for '" + TargetName + "' is empty or not specified.  Ignore this warning if the project has no source files (i.e. it is purely an executable).");
                }
                else
                {
                    string strDir = System.IO.Path.Combine(BuildConfig.BranchRootDirectory, psd);
                    if (!System.IO.Directory.Exists(strDir))
                        throw new Exception("The given directory '" + strDir + "' does not exist.  Please update the SourceDirectory setting in Projects.cfg.");

                    System.IO.Directory.SetCurrentDirectory(strDir);

                    GatherSource_r();
                }
            }

            // Make the most recently modified files compile first.  This way we can hit possible errors faster.
            SourceFiles.Sort((a, b) => b.LastModifyDateTime.CompareTo(a.LastModifyDateTime) );
        }
        private void GatherSource_r()
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            string[] files = System.IO.Directory.GetFiles(curDir);

            foreach (string file in files)
            {
                string ext = System.IO.Path.GetExtension(file);
                if (ObjectFileExtensions.Find(x => x.ToLower() == ext.ToLower()) != null)
                {
                    SourceFile sf = new SourceFile();
                    sf.FileName = System.IO.Path.GetFileName(file);
                    sf.FilePathWithName = file;//IDK MAN

                    System.IO.FileInfo inf = new System.IO.FileInfo(file);
                    sf.LastModifyDateTime = inf.LastWriteTime;
                    
                    SourceFiles.Add(sf);
                }
            }

            if (RecursiveSearchSourceDirectory == true)
            {
                string[] dirs = System.IO.Directory.GetDirectories(curDir);
                foreach (string d in dirs)
                {
                    System.IO.Directory.SetCurrentDirectory(d);
                    GatherSource_r();
                    System.IO.Directory.SetCurrentDirectory(curDir);
                }
            }
        }
        public string GetLibrarianFlags()
        {
            string ret = "";
            //OK so LIB.exe is really LINK.exe with /LIB - it runs link to do the library process
            // Because we optimize per core, LINK was running on a different core when called by LIB
            // SO to make this faster we simply call link.exe with the /LIB switch
            ret += "/LIB ";

            if (String.IsNullOrEmpty(LibName))
                throw new Exception("Invalid - type is lib but no lib name specified for target " + TargetName);
            string outLib = System.IO.Path.Combine(ProjectRootedLibPath, LibName);

            ret += "/OUT:" + BuildUtils.Enquote(outLib) + " ";
            foreach (string lf in LibrarianFlags)
                ret += lf + " ";
            
            // **Libpaths are unnecessary for the lib operation.
            //ret += CompileLibpathSwitches();

            return ret;
        }
        public string GetLinkerOutputFullFilePath()
        {
            if (TargetType == BuildTargetType.Executable)
                return System.IO.Path.Combine(ProjectRootedBinPath, ExeName);

            else if (TargetType == BuildTargetType.Library)
                return System.IO.Path.Combine(ProjectRootedLibPath, LibName);

            else
                throw new NotImplementedException();
        }
        public string GetLinkerFlags()
        {
            //FLAGS
            string linkerFlags = string.Empty;
            string linkerLibPaths = string.Empty;
            string linkerDependencies = string.Empty;
            string linkerOutputDirectories = string.Empty;
            string linkerFinalString = string.Empty;

            foreach (string lf in LinkerFlags)
            {
                linkerFlags += lf + " ";
            }

            // Out dirs
            if (String.IsNullOrEmpty(ExeName))
                throw new Exception("Invalid - type is exe but no exe name specified for target " + TargetName);
            string outBin = System.IO.Path.Combine(ProjectRootedBinPath, ExeName);

            if (String.IsNullOrEmpty(PdbName))
                throw new Exception("Invalid - type is exe but no pgd name specified for target " + TargetName);
            string outPdb = System.IO.Path.Combine(ProjectRootedBinPath, PdbName);

            linkerOutputDirectories += "/OUT:" + BuildUtils.Enquote(outBin) + " ";
            linkerOutputDirectories += "/PDB:" + BuildUtils.Enquote(outPdb) + " ";

            string manifestFileDir = BuildUtils.Enquote(outPdb).Replace("pdb", "intermediate.manifest");
            linkerOutputDirectories += "/ManifestFile:" + manifestFileDir + " ";

            //Profile guided opt.
            if (linkerFlags.Contains("/LTCG:PGINSTRUMENT"))
            {
                if (String.IsNullOrEmpty(PgdName))
                    throw new Exception("Invalid - type is exe but no pdb name specified for target " + TargetName);
                string outPgd = System.IO.Path.Combine(ProjectRootedBinPath, PgdName);

                linkerOutputDirectories += "/PGD:" + BuildUtils.Enquote(outPgd) + " ";
            }
            else if (linkerFlags.Contains("/LTCG:PGOPTIMIZE"))
            {
                Globals.Logger.LogWarn("PGOPTIMIZE specified but no PGINSTRUMENT is specified - result will fail at link time.");
            }


            //DEPENDENCIES
            for (int n = 0; n < AdditionalDependencies.Count; n++)
            {
                linkerLibPaths += "\"" + AdditionalDependencies[n] + "\" ";
            }

            linkerLibPaths += CompileLibpathSwitches();

            // * Add everything
            linkerFinalString = linkerOutputDirectories
            + linkerFlags
            + linkerDependencies
            + linkerLibPaths
            ;

            return linkerFinalString;
        }
        public string CompileLibpathSwitches()
        {
            string ret = "";
            foreach (string dir in AdditionalLibraryDirectories)
            {
                ret += "/LIBPATH:" + "\"" + BuildUtils.TryAppendUncBranchRoot(dir) + "\" ";
            }
            return ret;
        }
        private string GetCompilerFlags(string strObjName)
        {
            string r = "";

            //FLAGS
            foreach (string cf in CompilerFlags)
            {
                if (cf.Trim() == "/Fd")
                {
                    if (!strObjName.EndsWith(".obj"))
                        Globals.Logger.LogError("Failure - filename does not end with obj. ", true);
                    string strPdb = strObjName.Replace(".obj", ".pdb");
                    r += cf + BuildUtils.Enquote(strPdb) + " ";
                }
                else
                {
                    r += cf + " ";
                }
            }

            //INCLUDES
            foreach (string dir in AdditionalIncludeDirectories)
            {
                r += "/I" + "\"" + BuildUtils.TryAppendUncBranchRoot(dir) + "\" ";
            }

            // DEFINES
            for (int n = 0; n < CompilerDefines.Count; n++)
            {
                r += "/D\"" + CompilerDefines[n] + "\" ";
            }

            return r;
        }
        public string GetObjectFileName(SourceFile sf)
        {
            string objName = System.IO.Path.GetFileNameWithoutExtension(sf.FileName);
            objName += ".obj";
            return objName;
        }
        private string GetObjectFilePathString(SourceFile sf)
        {
            //Project
            // > Source Dir > Source Name
            // 
            
            string str = System.IO.Path.GetDirectoryName(sf.FilePathWithName);
            if (str.Contains('$'))
            {
                str = str.Substring(str.IndexOf('$')+1);
            }
            if (str.Length > 0 && str[0] == '\\')
                str = str.Substring(1);
            return str;
        }
        public string GetObjectFilePath(SourceFile sf)
        {
            string comb = System.IO.Path.Combine(ProjectRootedTempPath, GetObjectFilePathString(sf));

            return System.IO.Path.Combine(comb, GetObjectFileName(sf));
        }
        public string GetSourceFileCompileString(SourceFile sf)
        {
            string objPath = GetObjectFilePath(sf);

            string buildSource =
                BuildUtils.Enquote(BuildConfig.CompilerBinPath + "\\" + "cl.exe")
                + " "
                + BuildUtils.Enquote(sf.FilePathWithName) + " "
                + "/Fo" + BuildUtils.Enquote(objPath) + " "
                + GetCompilerFlags(objPath)
            ;

            return buildSource;
        }


    }
}
