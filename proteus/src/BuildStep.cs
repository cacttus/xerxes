using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class BuildStep
    {
        #region Private: Members
        private static string _strSplitChar = "|";
        private List<BuildStep> Dependencies = new List<BuildStep>();
        #endregion

        #region Public:Members

        public const int ExcludedStepId = -1;

        public List<int> UnreferencedSteps; // Used only when we compile.  Ignore this.

        public DateTime StartTime;//begin /end execution for profiling
        public DateTime EndTime;

        public bool ObjectFileAlreadyCompiled = false; // if BuildCache found that the file was already compiled.

        public BuildStepStatus Status = BuildStepStatus.None;
        public int StepNumber;
        public string CommandText;
        public string OutputFileName;
        public string FullFilePath;
        public BuildTargetType? BuildTargetType;

        #endregion
        #region Public:Methods

        public BuildStep(int stepNumber) 
        { 
            StepNumber = stepNumber; 
        }
        public BuildStep(BuildTargetType btt,
                         int stepNumber,
                         string fileName,
                         string fullFilePath,
                         string command)
        {
            StepNumber = stepNumber;
            OutputFileName = fileName;
            FullFilePath = fullFilePath;
            CommandText = command;
            BuildTargetType = btt;
        }
        public void TryAddDependency(BuildStep bs)
        {
            //Do not add excluded steps.
            if (bs.IsExcluded())
                return;

            Dependencies.Add(bs);
        }
        public bool HasDependency(BuildStep bs)
        {
            return Dependencies.Contains(bs);
        }
        public bool IsExcluded()
        {
            bool bExcluded = false;

            // - put other exclude logic here
            if (ObjectFileAlreadyCompiled == true)
                bExcluded = true;

            //Ensure that our step ID is -1 if we are excluded so the DEP and BAT files don't fall out of sync
            if (bExcluded==true)
                Globals.ThrowIf(StepNumber != BuildStep.ExcludedStepId, 
                    " Build step ID was set, but the build step was excluded. " 
                    + " This would cause the DEP file to be out of sync. " 
                    + " Find where this problem is happening in the code. "
                    + " If this happens a workaround is to set UseDependencyCache = 'no' in the build configuration file. ");

            return bExcluded;
        } 
        public static BuildStep DecompileStepDependencyList(string stepString)
        {
            string[] values = stepString.Split('|');
            int nv = 0;

            int stepNumber = System.Convert.ToInt32(values[nv++]);
            BuildStep ret = new BuildStep(stepNumber);
            ret.BuildTargetType = (BuildTargetType)Enum.Parse(typeof(BuildTargetType), values[nv++]);
            ret.OutputFileName = values[nv++];
            ret.CommandText = values[nv++];
           
            int numDeps = System.Convert.ToInt32(values[nv++]);
            ret.UnreferencedSteps = new List<int>();
            for (int n = 0; n < numDeps; n++)
            {
                int bsId = System.Convert.ToInt32(values[nv++]);
                ret.UnreferencedSteps.Add(bsId);
            }
            return ret;
        }
        public static string CompileStepDependencyList(BuildStep bs)
        {
            string ret = string.Empty;
            ret += bs.StepNumber.ToString() + _strSplitChar;
            ret += bs.BuildTargetType.ToString() + _strSplitChar;
            ret += bs.OutputFileName + _strSplitChar;
            ret += bs.CommandText + _strSplitChar;

            ret += bs.Dependencies.Count().ToString() + _strSplitChar;

            foreach (BuildStep bd in bs.Dependencies)
            {

                Globals.ThrowIf(bd.IsExcluded() == true, "Build step " + bs.StepNumber + " had a reference to an excluded dependency.");
                
                ret += bd.StepNumber.ToString() + _strSplitChar;
            }

            ret = ret.Substring(0, ret.Length - 1);
            ret += "\r\n";
            return ret;
        }

        #endregion

    }
}
