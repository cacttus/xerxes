using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;
using Spartan;
using System.Reflection;
namespace BuildGui
{
    public class BuildGuiSettings
    {
        private class SettingNames
        {
            //Must be same name as variable
            public const string ExePath                = "ExePath";
            public const string BuildPath              = "BuildPath";
            public const string BuildId                = "BuildId";
            public const string Agents                 = "Agents";
            public const string ShowConsole            = "ShowConsole";
            public const string MaxErrorLimit          = "MaxErrorLimit";
            public const string SelectedConfigName     = "SelectedConfigName";
            public const string SelectedConfigPlatform = "SelectedConfigPlatform";
            public const string JumpToError = "JumpToError";
            public const string ShowCommandWindow = "ShowCommandWindow";
        }
        private class SettingDefaults
        {
            public const string ExePath = @"C:\Spartan.exe";
            public const string BuildPath = @"C:\";
            public const string BuildId = "0";
            public const string Agents = "";
            public const string ShowConsole = "false";
            public const string MaxErrorLimit = "1";
            public const string SelectedConfigName = "MyConfig";
            public const string SelectedConfigPlatform = "MyPlatform";
            public const string JumpToError = "false";
            public const string ShowCommandWindow = "false";
        }
        public BuildGuiSettings GetCopy() { return (BuildGuiSettings)this.MemberwiseClone(); }

        #region Public:Members
        public string SelectedConfigName;
        public string SelectedConfigPlatform;
        public int MaxErrorLimit;
        public string ExePath;
        public string BuildPath;
        public int BuildId;
        public bool ShowConsole = true;
        public bool JumpToError = false;//If true we jump to error when build fails
        public bool ShowCommandWindow = true;
        #endregion

        #region Private:Members
        private List<AgentInfo> _lstAgents = new List<AgentInfo>();
        private Object _objAgentsLockObject = new Object();
        private Object _objUpdateAgentsLockObject = new Object();
        private string _strFilePath;
        private bool _blnUpdating = false;
        private Object _objIsUpdatingLockObject = new Object();
        #endregion

        #region Public:Methods
        public List<AgentInfo> Agents
        {
            get
            {
                lock (_objAgentsLockObject)                
                    return _lstAgents;
            }
            set
            {
                lock (_objAgentsLockObject)
                    _lstAgents = value;
            }
        }
        public BuildGuiSettings(string defaultBuildDir = "", string defaultExePath = "", int defaultBuildId = 1, int defaultMaxErrorLimit = 1)
        {
            _strFilePath = ".\\bg_settings.cfg";
            BuildPath = defaultBuildDir;
            ExePath = defaultExePath;
            BuildId = defaultBuildId;
            MaxErrorLimit = defaultMaxErrorLimit;

            if (!System.IO.File.Exists(_strFilePath))
            {
                System.IO.FileStream fs = System.IO.File.OpenWrite(_strFilePath);
                fs.Close();
                fs.Dispose();
                GC.Collect();
                Save();
            }
        }
        public void Save()
        {
            int n = 0;
            string[] lines = new string[60];
            lines[n++] = GetConfigLine(SettingNames.ExePath, ExePath);
            lines[n++] = GetConfigLine(SettingNames.BuildPath, BuildPath);
            lines[n++] = GetConfigLine(SettingNames.BuildId, BuildId.ToString());
            lines[n++] = GetConfigLine(SettingNames.Agents, String.Join(",", Agents.Select(x => x.Name)));
            lines[n++] = GetConfigLine(SettingNames.ShowConsole, ShowConsole.ToString());
            lines[n++] = GetConfigLine(SettingNames.MaxErrorLimit, MaxErrorLimit.ToString());
            lines[n++] = GetConfigLine(SettingNames.SelectedConfigName, SelectedConfigName);
            lines[n++] = GetConfigLine(SettingNames.SelectedConfigPlatform, SelectedConfigPlatform);
            lines[n++] = GetConfigLine(SettingNames.JumpToError, JumpToError.ToString());
            lines[n++] = GetConfigLine(SettingNames.ShowCommandWindow, ShowCommandWindow.ToString());

            System.IO.File.WriteAllLines(_strFilePath, lines);
        }
        public void Load()
        {
            List<string> objFoundSettings = new List<string>();

            string[] lines = System.IO.File.ReadAllLines(_strFilePath);
            foreach (string line in lines)
            {
                if (line.Trim() == string.Empty)
                    continue;

                string[] vals = line.Split(new char[] { '=' });
                vals[0] = BuildUtils.Dequote(vals[0]).Trim();
                vals[1] = BuildUtils.Dequote(vals[1]).Trim();

                objFoundSettings.Add(vals[0]);

                LoadSetting(vals[0], vals[1]);
            }

            CheckMissingSettings(objFoundSettings);

        }

        public void UpdateAgents()
        {
            lock (_objIsUpdatingLockObject)
            {
                if (_blnUpdating == true)
                    return;
                _blnUpdating = true;
            }

            List<AgentInfo> newAgents = new List<AgentInfo>();
            List<string> names = new List<string>();

            lock (_objUpdateAgentsLockObject)
            {
                foreach (AgentInfo inf in Agents)
                    names.Add(inf.Name);

                foreach (string infName in names)
                {
                    AgentInfo inf = AgentDiagnostics.GetAgentInfo(infName);
                    newAgents.Add(inf);
                }

                Agents = newAgents;

            }//end lock

            lock (_objIsUpdatingLockObject)
            {
                _blnUpdating = false;
            }
        }

        #endregion

        #region Private:Methods
        private void CheckMissingSettings(List<string> aobjFoundSettings)
        {
            List<string> missing = new List<string>();
            List<string> settingnames = ReflectionUtils.GetAllPublicStringFields(new SettingNames());

            foreach (string str in settingnames)
            {
                if (!aobjFoundSettings.Contains(str))
                    missing.Add(str);
            }

            if (missing.Count > 0)
            {
                foreach (string st in missing)
                    Globals.Logger.LogWarn("Did not find setting, " + st + " setting to default value.");
                Save();
            }
        }
        private string GetConfigLine(string name, string data)
        {
            return name + " = " + BuildUtils.Enquote(data);
        }
        private void LoadSetting(string key, string value)
        {
            switch (key)
            {
                case SettingNames.JumpToError:
                    JumpToError = Convert.ToBoolean(value);
                    break;
                case SettingNames.SelectedConfigName:
                    SelectedConfigName = value;
                    break;
                case SettingNames.SelectedConfigPlatform:
                    SelectedConfigPlatform = value;
                    break;
                case SettingNames.ExePath:
                    ExePath = value;
                    break;
                case SettingNames.MaxErrorLimit:
                    MaxErrorLimit = Convert.ToInt32(value);
                    break;
                case SettingNames.BuildPath:
                    BuildPath = value;
                    break;
                case SettingNames.BuildId:
                    BuildId = Convert.ToInt32(value);
                    break;
                case SettingNames.ShowConsole:
                    ShowConsole = Convert.ToBoolean(value);
                    break;
                case SettingNames.ShowCommandWindow:
                    ShowCommandWindow = Convert.ToBoolean(value);
                    break;
                case SettingNames.Agents:
                    Agents.Clear();
                    string[] vaar = value.Split(',');
                    foreach (string a in vaar)
                    {
                        if (a == string.Empty)
                            continue;
                        if (Agents.Where(x => x.Name.ToLower() == a.ToLower()).FirstOrDefault() == null)
                        {
                            CreateAgent(a);
                        }
                    }
                    break;
                default:
                    //throw new NotImplementedException();
                    Globals.Logger.LogError("Invalid setting name " + key + " ignoring.");
                    break;
            }
        }
        public void CreateAgent(string name)
        {
            lock (_objUpdateAgentsLockObject)
            {
                if (Agents.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault() == null)
                {
                    Agents.Add(
                        new AgentInfo
                        {
                            Name = name
                        }
                    );
                }

            }//end lock
        }
        public bool DeleteAgent(string name)
        {
            lock (_objUpdateAgentsLockObject)
            {
                AgentInfo inf = Agents.Where(x => x.Name.Equals(name)).FirstOrDefault();
                if (inf != null)
                {
                    Agents.Remove(inf);
                    return true;
                }
                else
                {
                    Globals.Logger.LogWarn("Could not find agent to remove.");
                    return false;
                }
            }//end lock
        }
        #endregion


    }
}
