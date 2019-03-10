using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class FileUtils
    {
        #region File Maintenance
        //Performs basic file maintenance on log or data files.
        //      ** Assumes the file is not yet created.
        //       Creates new file with a different name.
        //       moves old files to "temp" folders.
        //       deletes files that are older than x days /  hours
        // Return: Returns a string describing the errors/ operations.
        public static string MoveAndCleanLogs(
            string astrNewFileNameWithPath, 
            int aintNumDaysToKeepOldFiles = 4,
            string astrArchiveFolder = "arch",
            bool ablnAppendEpochTimeToDuplicateFile = true // If false it will rename the file with a _n such as _1, _2, _3.. etc 
            )
        {
            string ret = "";

            //Rename Duplicates
            ret += RenameDuplicateFile(astrNewFileNameWithPath, ablnAppendEpochTimeToDuplicateFile);
            
            //Archive other files (even if not duplicate)
            ret += ArchiveFiles(astrNewFileNameWithPath, astrArchiveFolder, true);
            
            // Delete old files.
            ret += CleanOldFiles(astrNewFileNameWithPath, astrArchiveFolder, aintNumDaysToKeepOldFiles);

            return ret;
        }
        private static string RenameDuplicateFile(string astrLogFilePath, bool ablnAppendEpochTimeToDuplicateFile = true)
        {
            string ret = string.Empty;

            if (System.IO.File.Exists(astrLogFilePath))
            {
                // New file name is Epoch milliseconds
                System.IO.FileInfo inf = new System.IO.FileInfo(astrLogFilePath);
                string oldFileName = System.IO.Path.GetFileNameWithoutExtension(astrLogFilePath);

                string newFileName = string.Empty;

                if (ablnAppendEpochTimeToDuplicateFile)
                {
                    long millis = System.Convert.ToInt64(inf.LastWriteTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                    newFileName = oldFileName + millis.ToString();
                }
                else
                {
                    newFileName = oldFileName;
                }

                string logDir = System.IO.Path.GetDirectoryName(astrLogFilePath);
                string newLogPath = System.IO.Path.Combine(logDir, newFileName);

                // - Make sure it doesn't exist
                string newLogPathVerified = newLogPath + ".log";
                int n = 0;
                while (System.IO.File.Exists(newLogPathVerified) == true)
                {
                    newLogPathVerified = newLogPath + "_" + n.ToString() + ".log";
                    n++;
                }

                //Move the file
                ret += "Renaming log " + astrLogFilePath + " to " + newLogPathVerified + "\n";
                System.IO.File.Move(astrLogFilePath, newLogPathVerified);
                
            }
            return ret;
        }
        private static string ArchiveFiles(string astrFileName, 
                                           string astrArchiveFolder, 
                                           bool ablnCreateArchiveFolderIfNotPresent = true)
        {

            string ret = string.Empty;

            string strArchiveDirectory = string.Empty;
            string strDirectory = System.IO.Path.GetDirectoryName(astrFileName);
            string strExtension = System.IO.Path.GetExtension(astrFileName);

            strArchiveDirectory = System.IO.Path.Combine(strDirectory, astrArchiveFolder);

            if (!System.IO.Directory.Exists(strArchiveDirectory))
            {
                if (ablnCreateArchiveFolderIfNotPresent)
                {
                    ret += "Creating " + strArchiveDirectory + "\n";
                    System.IO.Directory.CreateDirectory(strArchiveDirectory);
                }
                else
                    return ret;
            }

            string[] files = System.IO.Directory.GetFiles(strDirectory, "*" + strExtension);

            foreach (string file in files)
            {
                string filename = System.IO.Path.GetFileName(file);
                string newLocation = System.IO.Path.Combine(strArchiveDirectory, filename);
                ret += "moving " + file + " to " + newLocation + "\n";
                
                RenameDuplicateFile(newLocation); // Make sure we have no dupes before moving.
                
                System.IO.File.Move(file, newLocation);
            }

            return ret;
        }
        private static string CleanOldFiles(string astrFileName, 
                                            string astrArchiveFolder = "", 
                                            int aintNumDaysToKeepOldLogs = 4)
        {
            string ret = string.Empty;

            string strDirectory = System.IO.Path.GetDirectoryName(astrFileName);
            string strExtension = System.IO.Path.GetExtension(astrFileName);

            if (astrArchiveFolder != "")
               strDirectory = System.IO.Path.Combine(strDirectory, astrArchiveFolder);

            if (!System.IO.Directory.Exists(strDirectory))
               return ret;
            

            string[] files = System.IO.Directory.GetFiles(strDirectory);
            foreach (string fileName in files)
            {
                string ext = System.IO.Path.GetExtension(fileName);
                if (ext == strExtension)
                {
                    System.IO.FileInfo inf = new System.IO.FileInfo(fileName);
                    TimeSpan ts = DateTime.Now - inf.LastWriteTime;

                    if (ts.TotalDays > aintNumDaysToKeepOldLogs)
                    {
                        ret += "deleting old log.. " + fileName;
                        System.IO.File.Delete(fileName);
                        ret += "..deleted \n";
                    }
                }
            }
            return ret;
        }

        #endregion


        public static DateTime GetLastWriteTime(string path, bool refresh=true)
        {
            System.IO.FileInfo obj = new System.IO.FileInfo(path);
            
            //could be a performance killer
            if(refresh)
                obj.Refresh();
            
            return obj.LastWriteTime;
        }
        public static bool IsValidUncPath(string astrPath)
        {
            string strPath = astrPath;
            if (strPath.StartsWith("\\\\"))
            {
                strPath = strPath.Substring(2);
                int ind = strPath.IndexOf('$');
                if (ind > 1 && strPath.Length >= 2)
                    return true;
            }
            return false;
        }
        public static string MakeDiskRootFromUncRoot(string astrPath)
        {
            string strPath = astrPath;
            if (strPath.StartsWith("\\\\"))
            {
                strPath = strPath.Substring(2);
                int ind = strPath.IndexOf('$');
                if(ind>1 && strPath.Length >= 2)
                {
                    string driveLetter = strPath.Substring(ind - 1,1);
                    strPath = strPath.Substring(ind + 1);
                    strPath = driveLetter + ":" + strPath;
                }

            }
            return strPath;
        }
        public static string MakeUncRoot(string machineName, string path, string defaultRoot = "C:\\")
        {
            // Adds a path root if none specified, or
            // adds the \\ unc path.

            if (path[0] == '\\' && path[1] == '\\')
                return path;    // assume path is valid UNC root.

            if (path[0] == '\\' || path[0] == '/')
                path = path.Substring(1);

            int rlen = System.IO.Path.GetPathRoot(path).Length;
            string root = path.Substring(0, rlen);
            if (string.IsNullOrEmpty(root))
                root = defaultRoot;
            path = path.Substring(rlen, path.Length - rlen);

            //transform to unc path.
            root = "\\\\" + machineName + "\\" + root.Replace(':', '$');

            return System.IO.Path.Combine(root, path);
        }
        public static string CopyFilesToMachine(string machineName,
            List<string> lstFrom,
            List<string> lstTo,
            bool throwIfError = false,
            bool deleteFilesIfPresentInDestination = true,
            int deleteFileRetryCount = 2,
            int deleteFileSpinMillis = 200,
            bool createNonexistentDirectories = true)
        {
            string remotePath;
            string localPath;
            string returnString = string.Empty;

            if (lstFrom.Count != lstTo.Count)
                throw new Exception("File counts in from and to must be the same");


            for (int iFile = 0; iFile < lstFrom.Count; ++iFile)
            {
                localPath = lstFrom[iFile];
                remotePath = FileUtils.MakeUncRoot(machineName, lstTo[iFile]);
                try
                {
                    if (!System.IO.File.Exists(localPath))
                    {
                        returnString +="Error copying file. path="
                            + localPath
                            + ". remotepath= "
                            + remotePath + "\n";
                        if (throwIfError)
                            throw new Exception(returnString);
                        continue;
                    }

                    string remoteDir = System.IO.Path.GetDirectoryName(remotePath);
                    if (!System.IO.Directory.Exists(remoteDir))
                    {
                        if (createNonexistentDirectories == true)
                            System.IO.Directory.CreateDirectory(remoteDir);
                        else if (throwIfError)
                            throw new Exception("The directory " + remoteDir + " was not found");
                    }

                    if (deleteFilesIfPresentInDestination == true)
                    {
                        if (System.IO.File.Exists(remotePath))
                        {
                            int iRetry;
                            for (iRetry = 0; iRetry < deleteFileRetryCount; ++iRetry)
                            {
                                try
                                {
                                    System.IO.File.Delete(remotePath);
                                    break;
                                }
                                catch (Exception)
                                {
                                    System.Threading.Thread.Sleep(deleteFileSpinMillis);
                                }
                            }
                            if(iRetry==deleteFileRetryCount)
                                   returnString += "Could not delete file, retry count = " + iRetry + "\n";
                            
                        }
                        else
                            Globals.Logger.LogWarn("Could not delete remote file. path=" + remotePath + ", not found.");
                    }

                    System.IO.File.Copy(localPath, remotePath);
                }
                catch (Exception ex)
                {
                    returnString += "Error copying file path="
                        + localPath
                        + ". remotepath= "
                        + remotePath
                        + " error:\n"
                        + ex.ToString() + "\n";
                    if(throwIfError)
                        throw new Exception(returnString);
                }
            }


            return returnString;
        }

        public static void WriteBytesViaStream(string strFilePath, byte[] fileBytes)
        {
            System.IO.FileStream fs = null;
            try
            {
                fs = new System.IO.FileStream(strFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fs))
                {
                    fs = null;

                    writer.Write(fileBytes, 0, fileBytes.Length);
                }
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
            System.IO.File.SetAttributes(strFilePath, System.IO.FileAttributes.Normal);
        }

    }
}
