using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class BuildConfiguration
    {
        public bool UseCache;
        public string ConfigurationName;
        public string BuildPlatform;
        public override string ToString()
        {
            return ConfigurationName + ", " + BuildPlatform;
        }
        public BuildConfiguration() { }
        public BuildConfiguration(string name, string plat)
        {
            ConfigurationName = name;
            BuildPlatform = plat;
        }

        public string GetPathName()
        {
            return ConfigurationName.Replace(' ', '_') + "\\" + BuildPlatform.Replace(' ','_');
        }
    }
}
