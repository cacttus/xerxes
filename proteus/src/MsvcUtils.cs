using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Proteus
{
    public class MsvcUtils
    {
        #region INTEROP

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetLongPathName(string ShortPath, StringBuilder sb, int buffer);

        [DllImport("kernel32.dll")]
        static extern uint GetShortPathName(string longpath, StringBuilder sb, int buffer);
        
        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx pctx);

        #endregion

        #region VISUAL_STUDIO_UTILS

        public static void VisualStudioDebugProject(string solutionName = "")
        {
            VisualStudioRunCommand("Debug.Start", solutionName);
        }
        public static void VisualStudioSaveAllFiles(string solutionName = "")
        {
            VisualStudioRunCommand("File.SaveAll", solutionName);
        }
        public static void VisualStudioOpenFileAtLine(string fileName, int fileLine, string solutionName = "")
        {
            try
            {
                //** New method which uses COM tables
                EnvDTE80.DTE2 dte2 = GetVsInstance(solutionName);
                if (dte2 != null)
                {
                    dte2.MainWindow.Activate();
                    EnvDTE.Window w = dte2.ItemOperations.OpenFile(fileName);
                    ((EnvDTE.TextSelection)dte2.ActiveDocument.Selection).GotoLine(fileLine, true);
                }
                else
                    Globals.Logger.LogError("Could not get DTE for the given solution.");
            }
            catch (Exception e)
            {
                Globals.Logger.LogError("Exception getting DTE:\n " + e.ToString());

            }
        }
        public static void VisualStudioRunCommand(string strCommand, string solutionName = "")
        {
            try
            {
                EnvDTE80.DTE2 dte2 = GetVsInstance(solutionName);
                if (dte2 != null)
                {
                    dte2.MainWindow.Activate();
                    EnvDTE.Debugger debugger;
                    debugger = dte2.Debugger;
                    dte2.ExecuteCommand(strCommand);
                }
                else
                    Globals.Logger.LogError("Could not get DTE for the given solution.");
            }
            catch (Exception e)
            {
                Globals.Logger.LogError("Exception getting DTE:\n " + e.ToString());
            }
        }

        #endregion

        #region WINDOWS
        public static string GetWindowsPhysicalPath(string path)
        {
            StringBuilder builder = new StringBuilder(255);

            // names with long extension can cause the short name to be actually larger than
            // the long name.
            GetShortPathName(path, builder, builder.Capacity);

            path = builder.ToString();

            uint result = GetLongPathName(path, builder, builder.Capacity);

            if (result > 0 && result < builder.Capacity)
            {
                //Success retrieved long file name
                builder[0] = char.ToLower(builder[0]);
                return builder.ToString(0, (int)result);
            }

            if (result > 0)
            {
                //Need more capacity in the buffer
                //specified in the result variable
                builder = new StringBuilder((int)result);
                result = GetLongPathName(path, builder, builder.Capacity);
                builder[0] = char.ToLower(builder[0]);
                return builder.ToString(0, (int)result);
            }

            return null;
        }
        public static void SetMsvcVars(int intMsvcVersion, int intMsSdkVersion)
        {
            string strVcIdePath = String.Empty;


            if (intMsvcVersion == 11)
            {
                strVcIdePath = "C:\\Program Files (x86)\\Microsoft Visual Studio 11.0\\Common7\\IDE";
            }
            else
                throw new NotImplementedException();

            AddToPath(strVcIdePath, "PATH");


        }
        private static void AddToPath(string strVarValue, string strVarName)
        {
            EnvironmentVariableTarget target = EnvironmentVariableTarget.Process;

            string strEnvValue = System.Environment.GetEnvironmentVariable(strVarName, target);
            if (strEnvValue == null)
            {
                strEnvValue = strVarValue;
                System.Environment.SetEnvironmentVariable(strVarName, strEnvValue, target);
            }
            else if (!strEnvValue.Contains(strVarValue))
            {
                strEnvValue += ";" + strVarValue;
                System.Environment.SetEnvironmentVariable(strVarName, strEnvValue, target);
            }

            string test = System.Environment.GetEnvironmentVariable(strVarName, target);

        }
        public static bool ParseClCompilerOutputFileLine(string line,
                                                   ref string strFile,
                                                   ref int intLine,
                                                   bool blnRemoveUncPath = true
                                                   )
        {
            //Reutrns false if the given line didn't match the syntax

            strFile = "";
            string strLine = "";

            string[] strarr;

            strarr = line.Split(':');
            if (strarr.Length < 2)
                return false;

            strarr = strarr[0].Split('(');
            if (strarr.Length != 2)
                return false;

            strFile = strarr[0].Trim();
            strLine = strarr[1].Substring(0, strarr[1].Length - 2).Trim();

            if (strFile == "" || strLine == "")
                return false;

            if (blnRemoveUncPath)
            {
                int ipos = strFile.LastIndexOf('\\');
                if (ipos > 0)
                    strFile = MsvcUtils.GetWindowsPhysicalPath(strFile);
                strFile = strFile.Trim();
                strLine = strLine.Trim();
                
                // Remove the UNC root.
                //    This is because when we open up files in VS. if they have different path name
                //    they will be treated as different files.
                strFile = FileUtils.MakeDiskRootFromUncRoot(strFile);
            }

            if (!Int32.TryParse(strLine, out intLine))
                return false;

            return true;
        }
        #endregion

        #region ANNOYING_DTE_AND_COM_CRAP

        private static EnvDTE80.DTE2 GetVsInstance(string slnName = "")
        {
            //**If slnName is null we get the first instance in the COM table.
            // Otherwise search for the instance.
            if (slnName == "")
               return (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");

            return (EnvDTE80.DTE2)TryGetVsInstanceBySolutionName(new List<string> { slnName });
        }
        private static EnvDTE._DTE TryGetVsInstanceBySolutionName(List<string> slnNames)
        {
            EnvDTE._DTE dte;
            System.Diagnostics.Process[] procs;
            
            procs = System.Diagnostics.Process.GetProcessesByName("devenv");

            foreach (System.Diagnostics.Process p in procs)
            {
                if (TryGetVSInstance(p.Id, out dte) == false)
                    continue;

                string dteSlnName = dte.Solution.FullName;
                dteSlnName = System.IO.Path.GetFileName(dteSlnName);

                foreach (string slnName in slnNames)
                    if (dteSlnName.ToLower().Equals(slnName.ToLower()))
                        return dte;
            }

            return null;
        }
        private static bool TryGetVSInstance(int processId, out EnvDTE._DTE instance)
        {
            IntPtr numFetched = IntPtr.Zero;
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                object runningObjectVal;
                runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                if (runningObjectVal is EnvDTE._DTE && runningObjectName.StartsWith("!VisualStudio"))
                {
                    int currentProcessId = int.Parse(runningObjectName.Split(':')[1]);

                    if (currentProcessId == processId)
                    {
                        instance = (EnvDTE._DTE)runningObjectVal;
                        return true;
                    }
                }
            }

            instance = null;
            return false;
        }
        //private static object GetRunningCOMObjectByName(string objectDisplayName)
        //{
        //    IRunningObjectTable runningObjectTable = null;
        //    IEnumMoniker monikerList = null;

        //    try
        //    {
        //        if (GetRunningObjectTable(0, out runningObjectTable) != 0 || runningObjectTable == null) return null;

        //        runningObjectTable.EnumRunning(out monikerList);

        //        monikerList.Reset();

        //        IMoniker[] monikerContainer = new IMoniker[1];

        //        IntPtr pointerFetchedMonikers = IntPtr.Zero;

        //        while (monikerList.Next(1, monikerContainer, pointerFetchedMonikers) == 0)
        //        {
        //            IBindCtx bindInfo;

        //            string displayName;

        //            CreateBindCtx(0, out bindInfo);

        //            monikerContainer[0].GetDisplayName(bindInfo, null, out displayName);

        //            Marshal.ReleaseComObject(bindInfo);

        //            if (displayName.IndexOf(objectDisplayName) != -1)
        //            {
        //                object comInstance;

        //                runningObjectTable.GetObject(monikerContainer[0], out comInstance);

        //                return comInstance;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        // Nichts zurückgeben
        //        return null;
        //    }
        //    finally
        //    {

        //        if (runningObjectTable != null) 
        //            Marshal.ReleaseComObject(runningObjectTable);
        //        if (monikerList != null) 
        //            Marshal.ReleaseComObject(monikerList);
        //    }
        //    // Nichts zurückgeben
        //    return null;
        //}
        //private static IList<string> GetRunningCOMObjectNames()
        //{
        //    IList<string> result = new List<string>();
        //    IRunningObjectTable runningObjectTable = null;
        //    IEnumMoniker monikerList = null;

        //    try
        //    {
        //        if (GetRunningObjectTable(0, out runningObjectTable) != 0 || runningObjectTable == null) 
        //            return null;

        //        runningObjectTable.EnumRunning(out monikerList);

        //        monikerList.Reset();

        //        IMoniker[] monikerContainer = new IMoniker[1];
        //        IntPtr pointerFetchedMonikers = IntPtr.Zero;

        //        while (monikerList.Next(1, monikerContainer, pointerFetchedMonikers) == 0)
        //        {
        //            IBindCtx bindInfo;

        //            string displayName;

        //            CreateBindCtx(0, out bindInfo);

        //            monikerContainer[0].GetDisplayName(bindInfo, null, out displayName);

        //            Marshal.ReleaseComObject(bindInfo);

        //            result.Add(displayName);
        //        }
        //        // Auflistung zurückgeben
        //        return result;
        //    }
        //    catch
        //    {
        //        // Nichts zurückgeben
        //        return null;
        //    }
        //    finally
        //    {
        //        // Ggf. COM-Verweise entsorgen
        //        if (runningObjectTable != null) Marshal.ReleaseComObject(runningObjectTable);
        //        if (monikerList != null) Marshal.ReleaseComObject(monikerList);
        //    }
        //}


        #endregion


    }
}
