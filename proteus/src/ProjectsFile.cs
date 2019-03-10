using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class GlobalConfig
    {
        public string VsSolutionName;
    }
    public class ProjectsFile
    {
        public GlobalConfig GlobalConfig = new GlobalConfig();
        
        public List<BuildTarget> Targets = new List<BuildTarget>();

        public void Load(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            BuildTarget curTarget = null;

            int iLineNumber = 0;
            foreach (string line in lines)
            {
                iLineNumber++;
                string line2 = line.Trim();
                ParseProjectLine(line2, ref Targets, ref curTarget, iLineNumber);
            }
            //Add last target
            if (curTarget != null)
                Targets.Add(curTarget);
        }
        private void ParseProjectLine(string line, ref List<BuildTarget> targets, ref BuildTarget curTarget, int intLineNumber)
        {
            line = BuildUtils.ParseEatComment(line);
            if (string.IsNullOrEmpty(line))
                return;

            string[] values = BuildUtils.SplitValues(line, '=');
            if (values.Length != 2)
            {
                Globals.Logger.LogError("Line " + intLineNumber + ": Invalid token in projects file '" + line + "'. Ignoring..");
                return;
            }

            string key = values[0].ToLower().Trim();
            string value = BuildUtils.Dequote(values[1]).Trim();

            if (String.IsNullOrEmpty(value))
            {
                Globals.Logger.LogError("Line " + intLineNumber + ":Invalid value in projects file '" + line + "'. Ignoring..");
                return;
            }

            ParseLineValue(key, value, ref targets, ref curTarget, intLineNumber);
        }
        private void ParseLineValue(string key, string value, ref List<BuildTarget> targets, ref BuildTarget curTarget, int intLineNumber)
        {
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            //***PROJECT PARSE VARIABLES>
            bool blnTemp;

            switch (key.ToLower())
            {
                case "project":
                    if (curTarget != null)
                    {
                        targets.Add(curTarget);
                    }
                    curTarget = new BuildTarget();
                    curTarget.TargetName = value;

                    break;
                case "sourcedirectory":
                    if (System.IO.Path.IsPathRooted(value))
                        throw new Exception("Failure - source directory must be a path relative to the branch root. Such as 'my_project\\my_source' ");
                    curTarget.ProjectSourceDirectories.Add(value);
                    break;
                case "target":
                    switch (value.ToLower())
                    {
                        case "executable":
                            curTarget.TargetType = BuildTargetType.Executable;
                            break;
                        case "exe":
                            curTarget.TargetType = BuildTargetType.Executable;
                            break;
                        case "library":
                            curTarget.TargetType = BuildTargetType.Library;
                            break;
                        case "lib":
                            curTarget.TargetType = BuildTargetType.Library;
                            break;
                        default:
                            throw new Exception("Invalid target type '" + value + "'. Valid types are exe,executable,lib,library.");
                    }
                    break;
                case "exename":
                    curTarget.ExeName = value;
                    break;
                case "pgdname":
                    curTarget.PgdName = value;
                    break;
                case "pdbname":
                    curTarget.PdbName = value;
                    break;
                case "libname":
                    curTarget.LibName = value;
                    break;


                case "configuration":
                    curTarget.ConfigurationName = value;
                    break;
                case "inheritdefault":
                    Boolean.TryParse(value, out blnTemp);
                    curTarget.InheritDefaults = blnTemp;
                    break;
                case "platform":
                    curTarget.Platform = value;
                    break;
                case "objectfileextension":
                    if (curTarget.ObjectFileExtensions == null)
                        curTarget.ObjectFileExtensions = new List<string>();
                    curTarget.ObjectFileExtensions.Add(value);
                    break;
                case "additionalincludedirectory":
                    if (curTarget.AdditionalIncludeDirectories == null)
                        curTarget.AdditionalIncludeDirectories = new List<string>();
                    curTarget.AdditionalIncludeDirectories.Add(value);
                    break;
                case "additionallibrarydirectory":
                    if (curTarget.AdditionalLibraryDirectories == null)
                        curTarget.AdditionalLibraryDirectories = new List<string>();
                    curTarget.AdditionalLibraryDirectories.Add(value);
                    break;
                case "additionaldependency":
                    if (curTarget.AdditionalDependencies == null)
                        curTarget.AdditionalDependencies = new List<string>();
                    curTarget.AdditionalDependencies.Add(value);
                    break;
                case "compilerdefine":
                    if (curTarget.CompilerDefines == null)
                        curTarget.CompilerDefines = new List<string>();
                    curTarget.CompilerDefines.Add(value);
                    break;
                case "projectdependency":
                    if (curTarget.ProjectDependencies == null)
                        curTarget.ProjectDependencies = new List<string>();
                    curTarget.ProjectDependencies.Add(value);
                    break;


                case "compilerflag":
                    if (curTarget.CompilerFlags == null)
                        curTarget.CompilerFlags = new List<string>();
                    curTarget.CompilerFlags.Add(value);
                    break;
                case "linkerflag":
                    if (curTarget.LinkerFlags == null)
                        curTarget.LinkerFlags = new List<string>();
                    curTarget.LinkerFlags.Add(value);
                    break;
                case "librarianflag":
                    if (curTarget.LibrarianFlags == null)
                        curTarget.LibrarianFlags = new List<string>();
                    curTarget.LibrarianFlags.Add(value);
                    break;

                case "outputdirectorybin":
                    if (String.IsNullOrEmpty(curTarget.ProjectOverrideDirBin) == false)
                        Globals.Logger.LogWarn("Project bin dir is already set. Setting a second time.");
                    curTarget.ProjectOverrideDirBin = value;
                    break;
                case "outputdirectorylib":
                    if (String.IsNullOrEmpty(curTarget.ProjectOverrideDirLib) == false)
                        Globals.Logger.LogWarn("Project lib dir is already set. Setting a second time.");
                    curTarget.ProjectOverrideDirLib = value;
                    break;

                case "recursivesearchforsource":
                    curTarget.RecursiveSearchSourceDirectory = Convert.ToBoolean(value);
                    break;

                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                // *** GLOBALS
                case "vssolutionname":
                    if (!String.IsNullOrEmpty(GlobalConfig.VsSolutionName))
                        Globals.Logger.LogWarn("VS Solution name was already set to " + GlobalConfig.VsSolutionName + ", setting to " + value);
                    GlobalConfig.VsSolutionName = value;
                    break;
                default:
                    Globals.Logger.LogError("Line " + intLineNumber + ": Invalid token '" + value + "' in key '" + key + "'. Ignoring..");
                    break;
            }

        }
    }
}
