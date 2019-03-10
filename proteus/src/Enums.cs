using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public enum BuildTargetType
    {
        Other,  // other command line option
        ObjectFile, //c/cpp
        Library,//lib
        Executable  //exe
    }
    public enum BuildStepStatus
    {
        None,
        Success,
        Pending,
        Failed
    }
    public enum BuildPlatform
    {
        Win32,
        Win64
    }
    //public enum BuildConfiguration
    //{
    //    Debug,
    //    Release
    //}
    public enum ConsoleProcessCommandState
    {
        Idle,
        Running,
        Exited
    }
    public enum ConsoleProcessCommandType
    {
        ExecuteCommandText,
        Exit    // close console
    }
    public enum BuildStatus 
    { 
        None,
        Building, 
        Complete, 
        Failed, 
        CompileErrorLimitReached 
    }
    public class BuildFlags
    {
        //See SpartanGlobals.ParseArgs
        public const string Debug                    = "/d" ;
        public const string AgentProgram             = "/a" ;
        public const string CoordProgram             = "/c" ;
        public const string BuildDir                 = "/o" ;
        public const string ConfigName               = "/cn";
        public const string ConfigPlatform           = "/cp";
        public const string BuildId                  = "/bi";
        public const string AgentName                = "/ag";
        public const string Clean                    = "/x" ;
        public const string MaxErrorLimit            = "/me";
        public const string AttachBuildProcessesToUI = "/ui";
    }


}
