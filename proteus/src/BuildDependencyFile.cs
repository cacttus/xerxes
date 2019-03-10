using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class BuildDependencyFile
    {
        private string _strMakeData = "";
        private string _strBatchFileOutputPath;
        private BuildOrganizer _objBuildOrganizer;

        public BuildDependencyFile(BuildOrganizer bo, string strBatchFileOutputPath)
        {
            _objBuildOrganizer = bo;
            _strBatchFileOutputPath = strBatchFileOutputPath;
            _strMakeData = "";
        }
        public void Make()
        {
            foreach (BuildStep bs in _objBuildOrganizer.BuildSteps)
            {
                if (bs.IsExcluded() == false)
                    _strMakeData += BuildStep.CompileStepDependencyList(bs);
            }
        }
        public void Write()
        {
            System.Console.WriteLine("Writing dep file " + _strBatchFileOutputPath);
            if (!System.IO.Directory.Exists(BuildConfig.GetMakeAndBatchFileDirectory()))
                System.IO.Directory.CreateDirectory(BuildConfig.GetMakeAndBatchFileDirectory());

            string strFileData = _strMakeData;

            System.IO.File.WriteAllBytes(_strBatchFileOutputPath, System.Text.Encoding.ASCII.GetBytes(strFileData));
        }
    }
}
