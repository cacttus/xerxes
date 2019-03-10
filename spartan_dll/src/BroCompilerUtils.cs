using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spartan
{
    public class BroCompilerUtils
    {
        #region STATIC_MEMBERS
       
        public static int VcVersion = 11;

       // public const string SpartanBuildInstallDir  = "C:\\SpartanBuild\\";
        //public const string ServerBranchDirectory   = "C:\\p4\\derek.page\\C++";
        //public const string RomulusBatchPath        = ServerBranchDirectory + "\\borealis\\temp\\win32\\bro_make.bat";
       // public const String VcBinPath               = "C:\\Program Files (x86)\\Microsoft Visual Studio 11.0\\VC\\bin";
        public const string RomulusBinPath            = ".\\Romulus.exe";
        
        //Clean pths.
        //public const string CleanDr_Tmp = "C:\\p4\\derek.page\\c++\\borealis\\tmp\\debug_win32";
        //public const string CleanDr_Lib = "C:\\p4\\derek.page\\c++\\borealis\\lib\\debug_win32";
        //public const string CleanFl_Pdb = "C:\\p4\\derek.page\\c++\\borealis\\bin\\win32\\dmc.pdb";
        //public const string CleanFl_Exe = "C:\\p4\\derek.page\\c++\\borealis\\bin\\win32\\dmc.exe";
        //public const string CleanFl_Ilk = "C:\\p4\\derek.page\\c++\\borealis\\bin\\win32\\dmc.ilk";

        #endregion

        public static void SplitCommand(string cmdIn, ref string exeOut, ref string argsOut)
        {
            //split command into exe and args
            int exeIdx = cmdIn.IndexOf("exe\"") + 4;

            exeOut = cmdIn.Substring(0, exeIdx);
            argsOut = cmdIn.Substring(exeIdx, cmdIn.Length - exeIdx );
        }

        #region STATIC_METHODS
        
        //public static string GetVcVarsStr()
        //{
        //    string strVcVars;
        //    if (BroCompilerUtils.VcVersion == 11)
        //        strVcVars = "vcvars32.bat";
        //    else
        //        // ** vars are different for different vsrions.
        //        throw new Exception("Invalid VC Vers");

        //    strVcVars = System.IO.Path.Combine(BroCompilerUtils.VcBinPath, strVcVars);

        //    return "\"" + strVcVars + "\"";
        //}
        //public static void RunVcVars(System.Diagnostics.Process p)
        //{
        //    p.StandardInput.WriteLine(GetVcVarsStr());
        //}
        public static string GetBinPathFromBuildConfiguration(BuildConfiguration bc)
        {
            if (bc == BuildConfiguration.Debug)
                return "\\Debug";
            else if (bc == BuildConfiguration.Release)
                return "\\Release";
            else 
                throw new Exception("Invalid build configuration");
        }
        
        #endregion
    }
}



//Do we need this???
//shellProcess.StandardInput.Close();

//ShellProcess.StandardInput.Write("\r\n");
//string str = ShellProcess.StandardOutput.ReadToEnd();

// /c - do not link
// /EHsc - multi thread
// /MDd multithraed debug dll
// /D Defines

//System.IO.Directory.SetCurrentDirectory(BroCompilerUtils.ClientLocalBranchDirectory);
/// Write file to disk
// BroSourceFile sourceFile = (BroSourceFile)e.Argument;
// string strFileName = System.IO.Path.Combine(BroCompilerUtils.ClientLocalBranchDirectory, sourceFile.FileBranchName);

//  System.IO.File.WriteAllBytes(strFileName, System.Text.Encoding.ASCII.GetBytes(sourceFile.FileData));

// Start CL.exe
//System.Diagnostics.ProcessStartInfo objStartInfo = new System.Diagnostics.ProcessStartInfo();
//objStartInfo.Arguments = " " + sourceFile.CompilerArgs + " " + strFileName;
//objStartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
//objStartInfo.FileName = System.IO.Path.Combine(BroCompilerUtils.VcBinPath, BroCompilerUtils.StrCompilerName);
//System.Diagnostics.Process.Start(objStartInfo);

//Worked:for referenc
//string compilerArgs = " /c /EHsc /MDd \"C:/test.cpp\" /D WIN32 /D _DEBUG /D _CONSOLE /D _MBCS  ";

// -/c - do not link
// EHsc - use exception handlign
///D WIN32 /D _DEBUG /D _CONSOLE /D _MBCS /link /out:\"c:\\test.exe\" /OPT:NOREF \"c:\\test.obj\"