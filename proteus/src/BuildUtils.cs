using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class BuildUtils
    {
        public const string DefaultTargetName = "*default";

        public static string DateTimeString(DateTime dt, bool ms = false)
        {
            if(ms)
                return dt.ToString("yyyyMMdd HH:mm:ss:fff");
            return dt.ToString("yyyyMMdd HH:mm:ss");
        }
        public static string TryAppendUncBranchRoot(string pathIn)
        {
            if (pathIn[0] == '/' || pathIn[0] == '\\')
                pathIn = pathIn.Substring(1);
            return System.IO.Path.Combine(BuildConfig.BranchRootDirectory, pathIn);
        }
        public static string GetBuildLogFilePath(int buildId)
        {
            string ret = System.IO.Path.Combine(BuildConfig.BuildConfigFilePath, GetBuildLogFileName(buildId));
            return ret;
        }
        public static string GetBuildLogFileName(int buildId)
        {
            string fileName = string.Empty;
            if (buildId < 0)
            {
                fileName = "build_" + System.Environment.TickCount + ".log";
            }
            else
            {
                fileName = "build_" + String.Format("{0:0000000000}", buildId) + ".log";
            }
            return fileName;
        }
        public static string[] SplitValues(string str, char ch)
        {
            string[] ret;

            int idx = str.IndexOf(ch);
            if (idx < 0)
            {
                ret = new string[1];
                ret[0] = str;
            }
            else
            {
                ret = new string[2];
                ret[0] = str.Substring(0, idx);
                ret[1] = str.Substring(idx + 1, str.Length - (idx + 1));
            }

            return ret;
        }
        public static string Enquote(string a)
        {
            return StringUtils.Enquote(a);
        }
        public static string Dequote(string a)
        {
            return StringUtils.Dequote(a);
        }
        public static string ParseEatComment(string x, string comment = "#")
        {
            if (!x.Contains(comment))
                return x;
            int ind = x.IndexOf(comment);
            string ret = x.Substring(0, ind);
            return ret.Trim();
        }
        public static void ShowErrorMessage(string msg)
        {
            System.Windows.Forms.MessageBox.Show(
                null,
                msg,
                "Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Exclamation,
                System.Windows.Forms.MessageBoxDefaultButton.Button1
                );
            Globals.Logger.LogError(msg);
        }
    }
}
