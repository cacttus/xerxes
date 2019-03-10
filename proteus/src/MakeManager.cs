using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class MakeManager
    {
        ProjectTreeManager _objBuildTreeManager = new ProjectTreeManager();
        public MakeManager()
        {
        }
        public void LoadBuildTargets()
        {
            Globals.Logger.LogInfo("Loading projects..");
            string path = System.IO.Path.Combine(BuildConfig.BuildConfigFilePath, BuildConfig.ProjectsFileName);
            if (!System.IO.File.Exists(path))
                throw new Exception("Failed to load projects file from path '" + path + "'.");

            ProjectsFile pf = new ProjectsFile();
            pf.Load(path);

            _objBuildTreeManager.BuildProjectDependencyTree(pf.Targets);
        }
        public void Make()
        {
            string makeFileOutputPath = BuildConfig.GetMakeFilePath();
            string depFileOutputPath = BuildConfig.GetBatchFilePath();

            BuildOrganizer bo = new BuildOrganizer(makeFileOutputPath, depFileOutputPath, BuildConfig.GetCacheDirectory());

            ProjectTree objTree = _objBuildTreeManager.GetTreeByConfiguration(BuildConfig.GlobalBuildConfiguration);
            
            if (objTree == null)
                throw new Exception("Build tree could not be found for build configuration " + BuildConfig.GlobalBuildConfiguration.ConfigurationName + " - " + BuildConfig.GlobalBuildConfiguration.BuildPlatform);

            //instead send the tree in here.
            bo.BuildMakeFilesFromTargets(objTree);
            bo.WriteMakeFiles();

            //TEST
            // bo.LoadFilesAndCreateHierarchy();
        }

    }
}
