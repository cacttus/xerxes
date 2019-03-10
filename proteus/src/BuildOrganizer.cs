using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;

namespace Proteus
{
    /// <summary> 
    /// Organizes builds by allowing the builder to select only files with built dependencies.
    /// Manages the actual build when building with Spartan.
    /// </summary>
    public class BuildOrganizer
    {
        public List<BuildStep> BuildSteps;

        private ItemTree<BuildStep> _objStepTree;
        private List<BuildStep> _objFailedSteps;
        private Dictionary<int, BuildStep> _objStepNumberToBuildStep;
        private ItemTree<BuildStep> _objModifiedStepTree;
        private BuildDependencyFile _objDepFile;
        private MakeFile _objMakeFile;
        private string _strDepFilePath;
        private string _strMakeFilePath;
        private BuildStatus _eBuildStatus;
        private int _intErrorCount;
        
        public BuildOrganizer(string strMakeFilePath, string strDepFilePath, string strBuildCachePath = "")
        {
            _strDepFilePath = strDepFilePath;
            _strMakeFilePath = strMakeFilePath;
            BuildSteps = new List<BuildStep>();
            _objStepTree = new ItemTree<BuildStep>();
            _objFailedSteps = new List<BuildStep>();

            _objStepNumberToBuildStep = new Dictionary<int, BuildStep>();
            _objMakeFile = new MakeFile(this, _strMakeFilePath, strBuildCachePath);
            _objDepFile = new BuildDependencyFile(this, _strDepFilePath);
            _eBuildStatus = BuildStatus.None;
        }

        #region BUILD

        public void StartBuild()
        {
            // Create a copy of t
            _objModifiedStepTree = new ItemTree<BuildStep>(_objStepTree);
            _eBuildStatus = BuildStatus.Building;
            _intErrorCount = 0;
            _objFailedSteps.Clear();
        }
        public bool IsBuilding()
        {
           return _eBuildStatus == BuildStatus.Building;
        }
        public BuildStatus BuildStatus { get { return _eBuildStatus; } }
        public void AddBuildError()
        {
            _intErrorCount++;
            if (_intErrorCount >= BuildConfig.CompilerMaxErrorLimit)
                _eBuildStatus = BuildStatus.CompileErrorLimitReached;
            //Drop all agents and die.
            if (_eBuildStatus == BuildStatus.CompileErrorLimitReached)
            {
                Globals.Logger.LogError("[Organizer] Compile error limit of " + BuildConfig.CompilerMaxErrorLimit + " reached. Aborting compilation");
            }
        }
        public void StopBuild()
        {
            _eBuildStatus = BuildStatus.Failed;
        }
        public bool StepIsNotPending(BuildStep bx)
        {
            if (bx.Status == BuildStepStatus.Pending)
                return false;
            else if (bx.Status == BuildStepStatus.None)
                return true;
            //toher statuses not valie
            throw new NotImplementedException();
        }
        public bool HasStepsAvailableOnly()
        {
            return _objStepTree.GetNodeCount(StepIsNotPending) > 0;
        }
        public bool HasStepsAvailableOrPending()
        {
            return _objStepTree.GetNodeCount()>0;
        }
        public int NumRemainingSteps()
        {
            return _objStepTree.GetNodeCount();
        }
        public BuildStep GetNextAvailableBuildStep()
        {
            BuildStep item = null;
            if (_objFailedSteps.Count > 0)
            {
                // ** Try failed steps first.
                item = _objFailedSteps.First();
                _objFailedSteps.Remove(item);
            }
            else
            {
                ItemNode<BuildStep> x = _objStepTree.FirstChildWithoutChildren(null, StepIsNotPending);
                if(x==null)
                {
                    if (_objStepTree.GetNodeCount() == 0)
                    {
                        Globals.Logger.LogInfo("[Organizer] Build complete.");
                        _eBuildStatus = BuildStatus.Complete;
                    }
                }
                else
                {
                    //Removes the crap also
                    item = x.Item;
                   // item.Status = BuildStepStatus.Pending;
                   // _objStepTree.Prune(item);
                }
            }

            //**Critical code here to make sure
            //the tree doesn't return pending steps.
            if(item!=null)
                item.Status = BuildStepStatus.Pending;

            return item;
        }
        public BuildStep GetNextAvailableBuildStepCommand()
        {
            foreach (BuildStep value in _objStepNumberToBuildStep.Values)
            {
                if (value.CommandText.Substring(0, 4).ToLower() == "cmd ")
                {
                    if (_objStepTree.Find(value) != null)
                    {
                        return value;
                    }
                }
            }
            return null;
        }
        public void SetStepCompleted(BuildStep bs)
        {
            _objStepTree.Prune(bs);
        }
        public void ReInsertFailedStep(BuildStep bs)
        {
            // If we fail we will take steps from this pool first.
            _objFailedSteps.Add(bs);
        }

        #endregion

        #region MAKE

        public void BuildMakeFilesFromTargets(ProjectTree objTree)
        {
            _objMakeFile.Make(objTree);
            _objDepFile.Make();
        }
        public void WriteMakeFiles()
        {
            _objMakeFile.Write();
            _objDepFile.Write();
        }
        public void LoadFilesAndCreateHierarchy()
        {
            Globals.Logger.LogInfo("[Organizer] Loading " + _strMakeFilePath);
            string[] makeLines = System.IO.File.ReadAllLines(_strMakeFilePath);
            Globals.Logger.LogInfo("[Organizer] Loading " + _strDepFilePath);
            string[] depLines = System.IO.File.ReadAllLines(_strDepFilePath);

            if (makeLines.Length != depLines.Length)
                Globals.Throw(   "The make file and dep file are out of sync. "
                               + "  Check your code to ensure the number of lines "
                               + " in each file is the same and each command corresponds to the same command in each file.");

            Globals.Logger.LogInfo("[Organizer] Parsing");

            for (int n = 0; n < depLines.Length; n++)
            {
                string line = depLines[n];
                BuildStep bs = BuildStep.DecompileStepDependencyList(line);
                _objStepNumberToBuildStep.Add(bs.StepNumber, bs);
            }

            Globals.Logger.LogInfo("[Organizer] Building build tree..");
            MakeBuildStepTree();
        }
        private BuildStep GetStepById(int id)
        {
            BuildStep bs;
            _objStepNumberToBuildStep.TryGetValue(id, out bs);
            if (bs == null)
                Globals.Throw("Build step was null for build step ID " + id.ToString());
            return bs;
        }
        private void MakeBuildStepTree()
        {
            Dictionary<int, BuildStep> copy = new Dictionary<int, BuildStep>(_objStepNumberToBuildStep);

            List<BuildStep> toAdd = new List<BuildStep>();

            //Constructs a tree from a flat list.
            foreach(int key in copy.Keys)
            {
                BuildStep bs = GetStepById(key);

                toAdd.Clear();

                foreach (int childOfChildId in bs.UnreferencedSteps)
                    toAdd.Add(GetStepById(childOfChildId));

                _objStepTree.Build(bs, toAdd, null, true, true);
       
            }

        }
        #endregion



    }
}
