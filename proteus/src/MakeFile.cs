using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class MakeFile
    {
        private string _strBatchFileOutputPath;
        private int _intStepNumber = 0;
        private BuildOrganizer _objBuildOrganizer;
        private BuildCache _objBuildCache = null;

        public MakeFile(BuildOrganizer bo, string strBatchFileOutputPath, string cacheLoc = "")
        {
            _objBuildOrganizer = bo;
            _strBatchFileOutputPath = strBatchFileOutputPath;
            if(cacheLoc!=string.Empty)
                _objBuildCache = new BuildCache(cacheLoc);
        }
        public void Make(ProjectTree objTree)
        {
            AddMkDirs(objTree);
            AddVcVars();

            //Going depth first will ensure that the batch file builds
            //the dependent projects first and should thus build the final
            //executable last.
            objTree.IterateDepthFirst(MakeDelegate);
            objTree.IterateDepthFirst(ProjectDepsDelegate);
        }
        private void MakeDelegate(ItemNode<BuildTarget> target)
        {
            BuildTarget bt = target.Item;
            List<BuildStep> objLinkObjects = new List<BuildStep>();

            Globals.Logger.LogInfo("Gathering " + bt.TargetName);
            bt.GatherSource();

            Globals.Logger.LogInfo("Compiling " + bt.TargetName);
            CompileTarget(bt, ref objLinkObjects);

            Globals.Logger.LogInfo("Linking " + bt.TargetName);
            LinkTarget(bt, objLinkObjects);
        }
        private void ProjectDepsDelegate(ItemNode<BuildTarget> nodde)
        {
            BuildTarget bt = nodde.Item;
            List<ItemNode<BuildTarget>> childProjectNodes = nodde.FlattenBreadthFirst();
            //Flattening will contain one of these.  
            childProjectNodes.Remove(nodde);

            foreach (ItemNode<BuildTarget> btn in childProjectNodes)
            {
                if (bt.TargetStep.HasDependency(btn.Item.TargetStep))
                    throw new Exception("Error. Duplicate dependency found in project step " + btn.Item.TargetName);

                bt.TargetStep.TryAddDependency(btn.Item.TargetStep);
            }
        }

        #region OTHER_COMMANDS

        private void AddMkDirs(ProjectTree objTree)
        {
            List<BuildTarget> lbt = objTree.FlattenBreadthFirst();

            AddMkDir(BuildConfig.GlobalRootedBinPath);
            AddMkDir(BuildConfig.GlobalRootedLibPath);
            AddMkDir(BuildConfig.GlobalRootedTempPath);

            foreach (BuildTarget bt in lbt)
            {
                List<string> lst = bt.GatherMkdirs();

                foreach (string dir in lst)
                {
                    AddMkDir(dir);
                }
            }


        }
        private void AddMkDir(string dir)
        {
            AddCmd("MKDIR " + BuildUtils.Enquote(dir) + " ");
        }
        private void AddVcVars()
        {
            AddCmd("CALL " + BuildUtils.Enquote(BuildConfig.CompilerBinPath + "\\" + "vcvars32.bat"));
        }
        private void AddCmd(string cmdText)
        {
            BuildStep bs = new BuildStep(_intStepNumber++);
            bs.CommandText = "CMD " + cmdText;
            bs.BuildTargetType = BuildTargetType.Other;
            _objBuildOrganizer.BuildSteps.Add(bs);
        }

        #endregion

        private string GetLinkSwitches(BuildTarget bt)
        {
            return BuildUtils.Enquote(BuildConfig.CompilerBinPath + "\\" + "link.exe") + " " + bt.GetLinkerFlags();
        }
        private string GetLibSwitches(BuildTarget bt)
        {
            //DO NOT USE LIB.EXE see GetLibrarianFlags()
            return BuildUtils.Enquote(BuildConfig.CompilerBinPath + "\\" + "link.exe") + " " + bt.GetLibrarianFlags();
        }
        public void Write()
        {
            System.Diagnostics.Debug.Assert(_objBuildOrganizer.BuildSteps.Count > 0);
            string fileText = CompileBuildSteps();

            System.Console.WriteLine("Writing makefile " + _strBatchFileOutputPath);
            if (!System.IO.Directory.Exists(BuildConfig.GetMakeAndBatchFileDirectory()))
                System.IO.Directory.CreateDirectory(BuildConfig.GetMakeAndBatchFileDirectory());

            System.IO.File.WriteAllBytes(_strBatchFileOutputPath, System.Text.Encoding.ASCII.GetBytes(fileText));
        }
        private string CompileBuildSteps()
        {
            string ret = string.Empty;
            foreach (BuildStep bs in _objBuildOrganizer.BuildSteps)
            {
                if (bs.IsExcluded() == false)
                    ret += bs.CommandText + "\r\n";
            }
            return ret;
        }
        private void CompileTarget(BuildTarget bt, ref List<BuildStep> objLinkObjects)
        {
            // Compile stage
            foreach (SourceFile sf in bt.SourceFiles)
            {
                AddSourceFile(bt, sf, ref objLinkObjects);
            }
        }
        private void LinkTarget(BuildTarget bt, List<BuildStep> objLinkObjects)
        {
            // Link stage
            BuildStep bs = new BuildStep(_intStepNumber++);

            string linkSource = String.Empty;
            if (bt.TargetType == BuildTargetType.Executable)
            {
                linkSource += GetLinkSwitches(bt);

                foreach (BuildStep obj in objLinkObjects)
                {
                    linkSource += BuildUtils.Enquote(obj.FullFilePath) + " ";
                    bs.TryAddDependency(obj);
                }
            }
            else if (bt.TargetType == BuildTargetType.Library)
            {
                linkSource += GetLibSwitches(bt);
                foreach (BuildStep obj in objLinkObjects)
                {
                    linkSource += BuildUtils.Enquote(obj.FullFilePath) + " ";
                    bs.TryAddDependency(obj);
                }
            }
            else
            {
                throw new Exception("Build target was not specified in project file.  Specify lib");
                throw new NotImplementedException();
            }

            bs.OutputFileName = bt.GetOutputName();
            bs.FullFilePath = bt.GetLinkerOutputFullFilePath();
            bs.CommandText = linkSource;
            bs.BuildTargetType = bt.TargetType;
            
            bt.TargetStep = bs;

            _objBuildOrganizer.BuildSteps.Add(bs);
        }
        private void AddSourceFile(BuildTarget aobjBuildTarget, SourceFile aobjSourceFile, ref List<BuildStep> objLinkObjects)
        {
            string compileString = aobjBuildTarget.GetSourceFileCompileString(aobjSourceFile);
            string objectFilePath = aobjBuildTarget.GetObjectFilePath(aobjSourceFile);

            int stepId = BuildStep.ExcludedStepId; // Ids were getting out of sync in the DEP file for excluded build steps.
            bool bAlreadyCompiled = false;

            if (_objBuildCache != null)
                bAlreadyCompiled = !_objBuildCache.ObjectFileDoesNotExistOrIsOutOfDate(aobjSourceFile.FilePathWithName, objectFilePath, aobjBuildTarget);

            if(!bAlreadyCompiled)
                stepId = _intStepNumber++;

            BuildStep bs = new BuildStep(BuildTargetType.ObjectFile, stepId, aobjSourceFile.FileName, objectFilePath, compileString);
            bs.ObjectFileAlreadyCompiled = bAlreadyCompiled;
            
            _objBuildOrganizer.BuildSteps.Add(bs);
            objLinkObjects.Add(bs);
            
            
        }


    }
}
