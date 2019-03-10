using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class FileDependencyCache
    {
        protected class FdtFileNode
        {
            public string Path = "";
            public bool IsDirty = false;
          //  public List<FdtFileNode> DependentIncludes = new List<FdtFileNode>();
        }

        private List<string> _lstObjectFileExtensions = new List<string>();
        private List<string> _lstHeaderFileExtensions = new List<string>();
        private System.Collections.Hashtable _objHeaderList;

        private const string HeaderFileRegex = @"^(\s)*(#include)(\s*)("")(\s*)([a-zA-Z0-9\._\-\+~\\/]+)(\s*)("")(\s*)$";

        public FileDependencyCache()
        {
            _objHeaderList = new System.Collections.Hashtable();

            _lstObjectFileExtensions.Add(".cpp");
            _lstObjectFileExtensions.Add(".c");
            _lstObjectFileExtensions.Add(".cc");

            _lstHeaderFileExtensions.Add(".h");
            _lstHeaderFileExtensions.Add(".hpp");
        }
        // Recursively search to find if this file has any dirty headers
        public bool ObjectIsDirty(string astrSourceFileLoc, string astrObjectFileLoc, BuildCache bc, BuildTarget aobjBuildTaret)
        {
            string fileDir;
            string dir;
            bool isDirty;

            if (!System.IO.File.Exists(astrSourceFileLoc))
                return true;
            if (!System.IO.File.Exists(astrObjectFileLoc))
                return true;

            fileDir = System.IO.Path.GetDirectoryName(astrSourceFileLoc);
            dir     = System.IO.Directory.GetCurrentDirectory();
            DateTime datLastObjModify = FileUtils.GetLastWriteTime(astrObjectFileLoc);

            System.IO.Directory.SetCurrentDirectory(fileDir);
            {
                isDirty = FindFirstDirtyDependency(astrSourceFileLoc, astrObjectFileLoc, bc, aobjBuildTaret, datLastObjModify);
            }
            System.IO.Directory.SetCurrentDirectory(dir);

            return isDirty;
        }
        private bool FindFirstDirtyDependency(string astrSourceFileLoc,
                                              string astrObjectFileLoc,
                                              BuildCache bc,
                                              BuildTarget aobjBuildTarget,
                                              DateTime adatLastObjModifyTime)
        {
            List<FdtFileNode> headers;
            bool blnDirty = false;

            headers = ParseHeaders(astrSourceFileLoc, aobjBuildTarget);

            foreach (FdtFileNode header in headers)
            {
                //If the dictionary has teh header already then just return that header's dirty flag.
                if (_objHeaderList.Contains(header.Path))
                {
                    FdtFileNode node = (FdtFileNode)_objHeaderList[header.Path];
                    if (node.IsDirty)
                    {
                        blnDirty = true;
                        break;
                    }
                }
                else
                {
                    _objHeaderList.Add(header.Path,header);

                    if (bc.HeaderFileDoesNotExistOrIsOutOfDate_Not_Recursive(header.Path, adatLastObjModifyTime))
                    {
                        //Found
                        blnDirty = true;
                        header.IsDirty = blnDirty;
                        break;
                    }
                    else
                    {
                        //Continue search
                        blnDirty = FindFirstDirtyDependency(header.Path, astrObjectFileLoc, bc, aobjBuildTarget, adatLastObjModifyTime);
                        header.IsDirty = blnDirty;
                        
                        if (blnDirty == true)
                            break;// Make sure to break and not overwrite blnDirty with a false - that would invalidate tree structure.
                    }
                }
            }

            return blnDirty;
        }
        private List<FdtFileNode> ParseHeaders(string fileLoc, BuildTarget aobjBuildTarget)
        {
            List<FdtFileNode> objReturnedList;
            string sourceFilePath;
            string[] lines;

            objReturnedList = new List<FdtFileNode>();

            // fileLoc must be an absolute path.
            if (!System.IO.Path.IsPathRooted(fileLoc))
            {
                Globals.Logger.LogWarn("Path is not rooted.");
                return objReturnedList;
            }

            sourceFilePath = System.IO.Path.GetDirectoryName(fileLoc);
            lines          = System.IO.File.ReadAllLines(fileLoc);

            foreach (string line in lines)
            {
                System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(line, HeaderFileRegex);
                for (int nMatch = 0; nMatch < matches.Count; nMatch++)
                {

                    string headerFileName = matches[nMatch].Value;
                    string[] vals = headerFileName.Split('\"');

                    if (vals.Length == 3)
                    {
                        headerFileName = vals[1];
                    }
                    else
                    {
                        Globals.Logger.LogError("Header parse error in file " + fileLoc + " File may be invalid, skipping.");
                        return objReturnedList;
                    }

                    CollectHeader(fileLoc, headerFileName, sourceFilePath, objReturnedList, aobjBuildTarget);
                }
            }

            return objReturnedList;
        }
        private void CollectHeader(string astrSourceFileName,
                                   string astrHeaderFileName, // ** header file names will have relative paths appended
                                   string astrSourceFilePath,
                                   List<FdtFileNode> aobjCollectedList, 
                                   BuildTarget aobjBuildTarget
                                   )
        {
            string ext = System.IO.Path.GetExtension(astrHeaderFileName);
            if (!_lstHeaderFileExtensions.Contains(ext.ToLower()))
            {
                Globals.Logger.LogWarn("Found include that wasn't a .h file: " + astrHeaderFileName + "\n In FIle: " + astrSourceFileName + "\n");
                return;
            }

            string headerFilePath;
            if (!TryGetHeaderFilePath(astrHeaderFileName, astrSourceFilePath, aobjBuildTarget, out headerFilePath))
            {
                Globals.Logger.LogWarn("Header file could not be found at path: " + headerFilePath);
                return;
            }

            FdtFileNode hf = new FdtFileNode() { Path = headerFilePath };

            // Cache in dictionary so we don't recur forever, and also add to the list of new files.
            aobjCollectedList.Add(hf);
        }

        private bool TryGetHeaderFilePath(string astrHeaderFileName, string astrSourceFilePath, BuildTarget aobjBuildTarget, out string astrOutHeaderFilePath)
        {
            astrOutHeaderFilePath = String.Empty;
            string tmpFilePath;

            tmpFilePath = System.IO.Path.Combine(astrSourceFilePath, astrHeaderFileName);
            tmpFilePath = System.IO.Path.GetFullPath(tmpFilePath); // Simplify erroneous ../../../
            if(System.IO.File.Exists(tmpFilePath))
            {
                astrOutHeaderFilePath = tmpFilePath;
                return true;
            }

            string strDir;
            foreach (string strIncludeDirectory in aobjBuildTarget.AdditionalIncludeDirectories)
            {
                strDir = strIncludeDirectory;

                if (!System.IO.Path.IsPathRooted(strDir))
                {
                    strDir = BuildUtils.TryAppendUncBranchRoot(strDir);
                }
                tmpFilePath = System.IO.Path.Combine(strDir, astrHeaderFileName);
                if (System.IO.File.Exists(tmpFilePath))
                {
                    astrOutHeaderFilePath = tmpFilePath;
                    tmpFilePath = System.IO.Path.GetFullPath(tmpFilePath); // Simplify erroneous ../../../
                    return true;
                }
            }


            return false;
        }

    }
}













































/*********Reference to the odl BuildCache before we modify it.... this is if we want to use the dependency tree again.
    public class BuildCache
    {
        public string CacheRoot { get; set; }
        public string BranchRoot { get; set; }

        public BuildCache(string cacheRoot, string branchRoot)
        {
            CacheRoot = cacheRoot;
            BranchRoot = branchRoot;
        }
        public bool BranchFileExistsOnDisk(string fileBranchName)
        {
            string branchPath = System.IO.Path.Combine(BranchRoot, fileBranchName);
            return System.IO.File.Exists(branchPath);
        }
        // - C ache of all files that have been built.
        public void CacheDependencyFile(string fileBranchName)
        {
            string cachePath = System.IO.Path.Combine(CacheRoot, fileBranchName);
            string branchPath = System.IO.Path.Combine(BranchRoot, fileBranchName);

            bool b =  System.IO.File.Exists(branchPath);
            if (b == false)
            {
                if (branchPath.Contains("BroIncludes"))
                {
                }
                Console.WriteLine("*Possible warning, could not find header: '" + branchPath + "' file not cached");
                return;
            }
            System.Diagnostics.Debug.Assert(b==true);

            string dirName = System.IO.Path.GetDirectoryName(cachePath);
            
            System.IO.Directory.CreateDirectory(dirName);

            byte[] bytes = System.IO.File.ReadAllBytes(branchPath);
            System.IO.File.WriteAllBytes(cachePath, bytes);
        }
        public bool GetWhetherCachedDependencyFileDoesNotExistOrIsOutOfDate(string fileBranchName)
        {
            string cachePath = System.IO.Path.Combine(CacheRoot, fileBranchName);
            string branchPath = System.IO.Path.Combine(BranchRoot, fileBranchName);

            if (System.IO.File.Exists(cachePath) == false)
                return true;// This will return positive even for files that are not in the build.

            System.IO.FileInfo cd = new System.IO.FileInfo(cachePath);
            System.IO.FileInfo bd = new System.IO.FileInfo(branchPath);
            if (bd.LastWriteTime > cd.LastWriteTime)
                return true;
            return false;
        }

    } 
*/