//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;

//namespace DistBuild
//{

//    public class BroProject
//    {
//        public BroProjectConfiguration ProjectConfiguration { get; set; }
//        public BroProjectOutputType ProjectOutputType { get; set; }

//        public string OutputNameDebug { get; set; }
//        public string OutputNameRelease { get; set; }
//        public string OutputPath { get; set; }

//        public string ProjectSourceDirectory { get; set; } // the source root for the project. acct game audio etc.
//        public string ProjectFileDirectory { get; set; } // The .vcxproj location
//        public string SolutionFileDirectory { get; set; }

//        public List<string> CompilerDefines { get; set; }
//        public List<string> AdditionalIncludeDirectories { get; set; }
//        public List<string> AdditionalLibraryDirectories { get; set; }
//        public List<string> AdditionalDependencies { get; set; }

//       // public List<string> LinkerIncludeDirectories { get; set; }
//        public List<BroProject> ProjectDependencies { get; set; }

//        public List<string> ObjectFiles { get; set; }//CPP /C files **Note: these are ALL object files in the project not just the ones we build.

//        BroCompilerManager _objManager;

//        public BroProject(BroCompilerManager man)
//        {
//            _objManager = man;
//            CompilerDefines = new List<string>();
//           // CompilerIncludes = new List<string>();
//        }

//        /// <summary>
//        /// Add the object file names to the given project based on whether
//        /// they are missing from the build cache
//        /// </summary>
//        /// <param name="bc"></param>
//        public void GatherAllClientObjectFileNames()
//        {
//            ObjectFiles = new List<string>();

//            string strDIr = System.IO.Path.Combine(BroCompilerUtils.ServerBranchDirectory, ProjectSourceDirectory);

//            string[] files = System.IO.Directory.GetFiles(strDIr);
//            foreach (string file in files)
//            {
//                string ext = System.IO.Path.GetExtension(file);
//                if (ext.ToLower() == ".c" || ext.ToLower() == ".cpp")
//                {
//                    // add dumb file.
//                    string subDir = BroCompilerUtils.GetBranchLocalFileName(file);
//                  //  if(dt.GetHasOutdatedFileByName(file)==true)
//                        ObjectFiles.Add(subDir);
//                }
//            }

//        }

//        public string GetBuildOutputFileName()
//        {
//            if (_objManager.BuildConfiguration == BuildConfiguration.Debug)
//                return OutputNameDebug;
//            else
//                return OutputNameRelease;
//        }

//        //DEBUG
//        //  /Od /I "..\api\OpenGL\INCLUDE" /I "..\Borealis\src" /I "..\api\bullet-2.82-r2704\src" /I "..\api\OpenCL\inc" /I "..\api\dirent\inc" /I "..\api\directx_jun_2010\Include" /I "..\api\vorbis\inc" /I "../api/SDL-1.2.15/include" 
//        // /D "WIN32" /D "_DEBUG" /D "_CONSOLE" /D "_MBCS" 
//        // /Gm /EHsc /RTC1 /MDd /fp:fast /Fo"Debug\\" /Fd"Debug\vc90.pdb" /W3 /nologo /c /ZI /TP /errorReport:prompt

//        // Linker flags (DEBUG)
//        //  /OUT:"..\Borealis\bin\win32\dmc.exe" /INCREMENTAL /LIBPATH:"..\api\zlib-1.2.3.win32\lib" /LIBPATH:"..\api\SDL-1.2.15\lib\x86" /LIBPATH:"..\api\OpenGL\LIB" /LIBPATH:"..\Borealis\lib\win32" /LIBPATH:"..\api\directx_jun_2010\Lib\x86" /LIBPATH:"..\api\bullet-2.82-r2704\lib" /LIBPATH:"..\api\vorbis\LIB" /LIBPATH:"..\api\OpenCL\lib"
//        // /MANIFEST /MANIFESTFILE:"Debug\dmc.exe.intermediate.manifest" /MANIFESTUAC:"level='asInvoker' uiAccess='false'" 
//        // /DEBUG /PDB:"c:\p4\derek.page\C++\borealis\bin\win32\dmc.pdb" 
//        // /SUBSYSTEM:CONSOLE /DYNAMICBASE /NXCOMPAT /MACHINE:X86 
//        // ai_d.lib audio_d.lib base_d.lib dev_d.lib display_d.lib event_d.lib game_d.lib gpu_d.lib hardware_d.lib img_d.lib input_d.lib library_d.lib material_d.lib mathlib_d.lib menu_d.lib model_d.lib mm_memory_d.lib net_d.lib physics_d.lib repos_d.lib scene_d.lib topo_d.lib sdl.lib zlib.lib d3dx10d.lib d3d10.lib DxErr.lib dinput8.lib d3d9.lib d3dx9d.lib dxgi.lib dxguid.lib BulletCollision_vs2008_debug.lib BulletDynamics_vs2008_debug.lib ConvexDecomposition_vs2008_debug.lib LinearMath_vs2008_debug.lib libogg_static_d.lib libvorbis_static_d.lib libvorbisfile_static_d.lib opencl.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib "..\borealis\lib\win32\acct_d.lib"

//        public string GetLinkerFlags()
//        {
//            string ret = "";

//            ret += "/OUT:" + System.IO.Path.Combine(OutputPath, GetBuildOutputFileName()) + " ";

//            ret += "/INCREMENTAL ";
//            ret += "/SUBSYSTEM:CONSOLE ";
//            ret += "/DYNAMICBASE ";
//            ret += "/NXCOMPAT ";
//            ret += "/MACHINE:X86 ";

//            for (int n = 0; n < AdditionalLibraryDirectories.Count; n++)
//            {
//                ret += "/LIBPATH:" + "\"" + AdditionalLibraryDirectories[n] + "\" ";
//            }

//            for (int n = 0; n < AdditionalDependencies.Count; n++)
//            {
//                ret += "\"" + AdditionalDependencies[n] + "\" ";
//            }

//            return ret;
//        }

//        public string GetCompilerFlags()
//        {
//            string r = "";

//            if (ProjectConfiguration == BroProjectConfiguration.Debug)
//            {
//                r += "/Od ";  // optimization
//            }
//            else
//            {
//                //TODO: Optimization
//            }
//            r += "/Gm ";
//            r += "/EHsc ";
//            r += "/RTC1 ";
            
//            if (ProjectConfiguration == BroProjectConfiguration.Debug)
//                r += "/MDd ";
//            else
//                r += "/MD ";

//            r += "/fp:fast ";
//            //r += "/Fo\"Debug\\\" ";
//            //r += "/Fd\"Debug\vc90.pdb\" ";
//            r += "/W3 ";
//            r += "/nologo ";
//            r += "/ZI ";
//            r += "/TP ";
//            //r += "/errorReport:prompt ";

//            //INCLUDES
//            for (int n = 0; n < AdditionalIncludeDirectories.Count; n++)
//            {
//                r += "/I" + "\"" + AdditionalIncludeDirectories[n] + "\" ";
//            }
            
//            // DEFINES
//            if (ProjectConfiguration == BroProjectConfiguration.Debug)
//                r += "/D_DEBUG ";
//            else
//                r += "/D_NDEBUG ";
            
//            r += "/D_WIN32 ";

//            for (int n = 0; n < CompilerDefines.Count; n++)
//            {
//                r += "/D" + CompilerDefines[n] + " ";
//            }

//            return r;
//        }

//    }
//}
