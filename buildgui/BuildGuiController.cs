using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Proteus;
using Spartan;

namespace BuildGui
{

    public class BuildGuiController
    {
        #region Public: Interop

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        #endregion

        #region Private: Members

        private MainWindow _objWindow;
        private System.Diagnostics.Process _objShellProcess;
        private AppStatus _eAppStatus;
        private LogWatcher _objLogWatcherBuildOutput;
        private LogWatcher _objLogWatcherCoordOutput;
        private System.Windows.Threading.DispatcherTimer _objBuildUpdateTimer;
        private BuildGuiSettings _objSettings;
        private int _intCurrentBuildId = -1;
        private Object _objLogDataLockObject = new Object();
        private Object _objProgressBarLockObject = new Object();
        private AgentVisualData _objAgentVisualData = new AgentVisualData();
        private bool _blnDebugAfterBuild = false;
        private PacketMakerUdp _objPacketMakerUdp;
        private System.Windows.Threading.DispatcherTimer _objDelayedBuildOutputCancelTimer;
        private string _strSolutionName;
        private System.Windows.Controls.Control _objUxThreadControl = new System.Windows.Controls.Control();
        private FileTree _objFileTree;
        private bool _blnAgentsBeingUpdated = false;
        private Object _objAgentsBeingUpdatedLockObject = new Object();
        #endregion

        #region Public: Properties
        public ParsedBuildStatus ParsedBuildStatus = ParsedBuildStatus.Ok;
        public string SolutionName { get { return _strSolutionName; } set { } }
        public Control GetUxThreadControl()
        {
            return _objUxThreadControl;
        }
        public MainWindow GetGuiWindow()
        {
            return _objWindow;
        }
        public BuildGuiSettings Settings
        {
            get
            {
                return _objSettings;
            }
            set
            {
                _objSettings = value;
            }
        }
        public bool IsBuilding = false;

        #endregion

        #region Public: Methods

        public BuildGuiController(MainWindow wind)
        {
            _objWindow = wind;
            _objAgentVisualData.HorizontalAlignment = HorizontalAlignment.Stretch;
            _objAgentVisualData.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            _objAgentVisualData.VerticalAlignment = VerticalAlignment.Stretch;
            _objAgentVisualData.VerticalContentAlignment = VerticalAlignment.Stretch;

            _objWindow._spAgentStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            _objWindow._spAgentStackPanel.VerticalAlignment = VerticalAlignment.Stretch;
            _objWindow._spAgentStackPanel.LastChildFill = true;
            _objWindow._spAgentStackPanel.Children.Add(_objAgentVisualData);

        }
        
        public void Init()
        {
            try
            {
                Globals.InitializeGlobals("Monitor.log");
            }
            catch (Exception ex)
            {
                BuildUtils.ShowErrorMessage("Error initializing application.  The applcation will now exit.\nError Description:\n" + ex.ToString());
                Application.Current.Shutdown();
                return;
            }

            try
            {
                LoadSettings();
            }
            catch (Exception ex)
            {
                BuildUtils.ShowErrorMessage("Error loading settings file.\nError Description:\n" + ex.ToString());
            }

            try
            {
                ListenForCommands();
            }
            catch (Exception ex)
            {
                BuildUtils.ShowErrorMessage("Error listening for commands.\nError Description:\n" + ex.ToString());
            }

            try
            {
                UpdateAgentDisplayBoxesWithAgentStatus();
            }
            catch (Exception ex)
            {
                LogOutput("Exception caught querying agents tatuses \n" + ex.ToString());
            }

            _eAppStatus = AppStatus.Initialized;
        }
        public void StartBuild(bool clean)
        {
            InitBuild();

            if (_objFileTree!=null)
                _objFileTree.Reset(); // - adds scroll event

            if (_eAppStatus == AppStatus.Error)
                return;

            KillCurrentBuildProcesses();

            string exePath = _objWindow._txtSpartanExePath.Text;

            if (!System.IO.File.Exists(exePath))
            {
                BuildUtils.ShowErrorMessage("Spartan executable could not be found.");
                return;
            }

            //Reset
            _objWindow._statusBar.Background = UiBrushes.Brush_BuildColor_Window;
            ParsedBuildStatus = ParsedBuildStatus.Ok;
            _intCurrentBuildId = Settings.BuildId;

            _objLogWatcherBuildOutput.FilePath = BuildUtils.GetBuildLogFilePath(_intCurrentBuildId);
            _objLogWatcherCoordOutput.FilePath = ".\\logs\\coordinator.log";

            // Get args
            string args = GetBuildArgs(clean,
                GetBuildDirectory(),
                Settings.SelectedConfigName,
                Settings.SelectedConfigPlatform,
                _intCurrentBuildId,
                _objSettings.Agents.Select(x => x.Name).ToArray(),
                Settings.MaxErrorLimit
                );

            //Exec
            RunBuildProcess(exePath, args);

            IsBuilding = true;
        }
        public void SetDebugAfterBuild() { _blnDebugAfterBuild = true; }
        public void EndBuild()
        {
            //Avoid calling endbuild multiple times!!
            if (_objDelayedBuildOutputCancelTimer != null)
                return;

            // ** start a 3 second timer to make sure all output is flushed.
            _objDelayedBuildOutputCancelTimer = new System.Windows.Threading.DispatcherTimer();
            _objDelayedBuildOutputCancelTimer.Interval = new TimeSpan(0, 0, 0, 2, 0);
            _objDelayedBuildOutputCancelTimer.Tick += new EventHandler(EndBuildDelayed);
            _objDelayedBuildOutputCancelTimer.Start();
        }
        public void ReloadSettings()
        {
            LoadSettings();
        }
        public void CancelBuildProcess()
        {
            if (_objShellProcess == null || _objShellProcess.HasExited)
                return;

            _objShellProcess.Kill();

            while (!_objShellProcess.HasExited) 
            {
                //Wait for shell process to exit so we can make sure the build log is complete.
                System.Threading.Thread.Sleep(50);
            }
        }
        public void AddAgentDefinition(string name)
        {
            Settings.CreateAgent(name);
            _objWindow._lstAgents.Items.Add(name);
        }
        public void RemoveAgentDefinition()
        {
            if (_objWindow._lstAgents.SelectedItem != null)
            {
                if (Settings.DeleteAgent((string)_objWindow._lstAgents.SelectedItem))
                {
                    _objWindow._lstAgents.Items.Remove(_objWindow._lstAgents.SelectedItem);
                    _objWindow._lstAgents.SelectedItem = null;
                }

            }
        }
        public void LogOutput(string str)
        {
            // - Truncate window buffer size
           int maxCharCount = 3000000; // 3MB

           _objWindow._txtOutput.Dispatcher.BeginInvoke(new Action(() =>
           {
                if (_objWindow._txtOutput.Text.Length > maxCharCount)
                    _objWindow._txtOutput.Text = _objWindow._txtOutput.Text.Substring(maxCharCount);

                _objWindow._txtOutput.Text += str + "\r\n";
                _objWindow._txtOutput.ScrollToEnd();
           }));

          // System.Windows.Forms.Application.DoEvents();
        }
        public void ClearConsoleWindow()
        {
            _objWindow._txtOutput.Text = "";
            System.Windows.Forms.Application.DoEvents();
        }
        public void ResetAllAgents(bool useSc = false)
        {
            VerifySpartanGlobalsInitialized();

            foreach (AgentInfo info in Settings.Agents)
            {
                //**If service is set to restart this will work
                if(useSc == true)
                    // Use service control manager.
                    ResetAgent(info.Name);
                else
                    // kill application and allow it to reboot.
                    SendMsgToAgent(info.Name, SpartanGlobals.PacketTypeToMsgHeader(PacketType.RestartServer));
            }
        }
        public void StopAgent(string agentName)
        {
            string argument = string.Empty;
            argument += "/c ";
            argument += "sc \\\\" + agentName + " stop " + SpartanGlobals.AgentServiceName;
            LogOutput("Stopping.." + agentName);
            string output = ExecCmdSync(argument);
            LogOutput("...done:\n" + output);
        }
        public void StartAgent(string agentName)
        {
            string argument = string.Empty;
            argument += "/c ";
            argument += "sc \\\\" + agentName + " start " + SpartanGlobals.AgentServiceName;
            LogOutput("Starting.." + agentName);
            string output = ExecCmdSync(argument);
            LogOutput("...done:\n" + output);
        }
        public void ResetAgent(string agentName)
        {
            StopAgent(agentName);
            StartAgent(agentName);
        }
        public string ExecCmdSync(string arguments)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();

            psi.CreateNoWindow = true;
            psi.FileName = "cmd.exe";
            psi.Arguments = arguments;
            psi.Verb = "runas";
            psi.LoadUserProfile = false;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            proc.StartInfo = psi;
            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }
        public void UpdateAgentDisplayBoxesWithAgentStatus()
        {
            VerifySpartanGlobalsInitialized();

            System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += AgentUpdateEventHandler;
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }
        public void ReinstallAllAgents(string agentName = "")
        {
            if (agentName != string.Empty)
            {
                AgentInfo inf = Settings.Agents.Where(x => x.Name.Equals(agentName)).FirstOrDefault();
                if(inf!=null)
                    ReinstallAgent(inf);
            }
            else
            {
                foreach (AgentInfo ag in Settings.Agents)
                {
                    ReinstallAgent(ag);
                }
            }
        }
        
        #endregion

        #region Private: Methods

        private void ListenForCommands()
        {
            _objPacketMakerUdp = new PacketMakerUdp();
            _objPacketMakerUdp.RecvAsync(BuildMonitorGlobals.ProgramCommandUdpPortRecv, CommandReceivedDelegate);
        }
        private void CommandReceivedDelegate(string str)
        {
            str = str.Replace("\0", string.Empty);
            string[] parts = str.Split(':');
            if (parts.Length == 0)
                return;

            LogOutput("Got Exteranal Command " + str);

            string command = parts[0];

            var myDel = new Action<object>(delegate(object param)
            {
                //TODO: more commands here.
                if (command == BuildMonitorGlobals.Commands.Build)
                {
                    MsvcUtils.VisualStudioSaveAllFiles(_strSolutionName);
                    StartBuild(false);

                }
                else if (command == BuildMonitorGlobals.Commands.CancelBuild)
                {
                    EndBuild();
                }
            });

            // ** Execute in the UI thread so we can access all the ui goodies.
            _objUxThreadControl.Dispatcher.BeginInvoke(myDel, command);
        }
        private void ReinstallAgent(AgentInfo ag)
        {
            LogOutput("[[[[[[[[Installing on " + ag.Name);
            LogOutput("[[[[[[[[" + ag.Name + " stopping service ");
            
            LogOutput(AgentDiagnostics.InstallAgent(ag));
        }
        private void InitBuild()
        {
            if (!CheckValidBuildSettings())
                return;

            try
            {
                ResetAllDisplayItems();
                InitializeSpartanBuildSystem();
                InitializeUpdates();
            }
            catch (Exception ex)
            {
                BuildUtils.ShowErrorMessage("Failed to initialize " + BuildMonitorGlobals.AppName + ".  See logfile for more information. Application will now exit.  \n\n\nTechnical information:\n" + StringUtils.MakeParagraph(ex.ToString(), 50, 1));
                Application.Current.Shutdown();
            }
        }
        private void EndBuildDelayed(object sender, EventArgs e)
        {

            if (_objDelayedBuildOutputCancelTimer != null)//**why is this happening not sure.
            {
                _objDelayedBuildOutputCancelTimer.IsEnabled = false;
                _objDelayedBuildOutputCancelTimer.Stop();
            }
            else
            {
                LogOutput("Warning: got false end build signal.");
                //For some reason we are firing cancel builds even after the build timer is deleted.
                return; /// If we created a cancle build timer then delete it.
            }

            _objDelayedBuildOutputCancelTimer = null;

            if (IsBuilding == false)
                return;
            IsBuilding = false;

            if (ParsedBuildStatus == ParsedBuildStatus.Ok)
                _objWindow._statusBar.Background = UiBrushes.Brush_OkColor_Window;
            else if (ParsedBuildStatus == ParsedBuildStatus.Warning)
                _objWindow._statusBar.Background = UiBrushes.Brush_WarnColor_Window;
            else if (ParsedBuildStatus == ParsedBuildStatus.Error)
                _objWindow._statusBar.Background = UiBrushes.Brush_FailColor_Window;

            //Note* this method is also being used to end the build when the gui closes.
            CancelBuildProcess();



            //** do not call.  we must let the build exit on its own or else the linker might not write the exe.
            //KillCurrentBuildProcesses(); // ** tries to kill the system process
            UpdateProgressBar(1, 1);    // 100%

            // enable controls
            _objWindow._btnBuild.IsEnabled = true;
            _objWindow._btnCleanBuild.IsEnabled = true;
            _objWindow._btnCancelBuild.IsEnabled = false;
            _objWindow._btnBarStart.IsEnabled = true;
            _objWindow._btnBarStop.IsEnabled = false;
            _objWindow.EnableUserModification(true);

            //.stop polling
            // * these were causing problems because the build would end and not all the output would flush.
            if (_objLogWatcherBuildOutput != null)
            {
                _objLogWatcherBuildOutput.FlushSync();
                _objLogWatcherBuildOutput.EndPolling();
                //DO NOT SET TO NULL HERE
            }
            if (_objLogWatcherCoordOutput != null)
            {
                _objLogWatcherCoordOutput.FlushSync();
                _objLogWatcherCoordOutput.EndPolling();
                //DO NOT SET TO NULL HERE
            }
            if (_objBuildUpdateTimer != null) 
            {
                _objBuildUpdateTimer.Stop();
                _objBuildUpdateTimer.IsEnabled = false;
                //DO NOT SET TO NULL HERE
            }

            // Update the build a final time to flush the output.
            UpdateBuild();

            //Set objects to null
            _objLogWatcherCoordOutput = null;
            _objLogWatcherCoordOutput = null;
            _objBuildUpdateTimer = null;


            // Add the padded space to the bottom of the treeview so we can scroll up.
            _objFileTree.AddBottomScrollSpaceToTreeView();
            _objFileTree.ScrollToBottomOfFileTree();

            //Debug the project in the open MSVC instance.
            if (ParsedBuildStatus == ParsedBuildStatus.Ok)
            {
                // - Run the program
                if (_blnDebugAfterBuild == true)
                    MsvcUtils.VisualStudioDebugProject(_strSolutionName);

                // - Update Build Id
                Globals.Logger.LogInfo("Build Successful: Incrementing build id to " + (Settings.BuildId + 1).ToString());
                Settings.BuildId++;
                Settings.Save();
            }
            else if (ParsedBuildStatus == ParsedBuildStatus.Error)
            {
                if(Settings.JumpToError)
                    _objFileTree.JumpToNextError();
            }

            // Reset this.
            _blnDebugAfterBuild = false;

        }
        private bool CheckValidBuildSettings()
        {
            if (System.IO.Directory.Exists(GetBuildDirectory()) == false)
            {
                BuildUtils.ShowErrorMessage("Invalid build directory '"
                    + _objWindow._txtBuildDirectory.Text 
                    + "(formatted as " + GetBuildDirectory() + ") "
                    + "'.  Please set the correct buid directory in 'settings' and restart "
                    + BuildMonitorGlobals.AppName);

                _objWindow._tabControl.SelectedItem = _objWindow._tabSettings;
                _eAppStatus = AppStatus.Error;
                return false;
            }
            return true;
        }
        private void InitializeUpdates()
        {
            // ** Logs
            _objLogWatcherBuildOutput = new LogWatcher(
                //Invalid filename at first.
                System.IO.Path.Combine(BuildConfig.GlobalCompileOutputDirectory, "build.log"),
                200
                );
            _objLogWatcherBuildOutput.BeginPolling();

            _objLogWatcherCoordOutput = new LogWatcher(
                //Invalid filename at first.
                System.IO.Path.Combine(BuildConfig.GlobalCompileOutputDirectory, "build.log"),
                200
                );
            _objLogWatcherCoordOutput.BeginPolling();

            // Update loop.
            _objBuildUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            _objBuildUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _objBuildUpdateTimer.Tick += new EventHandler(BuildUpdateCallback);
            _objBuildUpdateTimer.Start();
        }
        private string GetBuildDirectory()
        {
            string uncBuildPath = _objWindow._txtBuildDirectory.Text;

            if (!FileUtils.IsValidUncPath(uncBuildPath))
                uncBuildPath = FileUtils.MakeUncRoot(
                System.Environment.MachineName,
                uncBuildPath);

            return uncBuildPath;
        }
        private void InitializeSpartanBuildSystem()
        {
            string cn = WpfUtils.GetComboItemText(_objWindow._cboConfigName);
            string cp = WpfUtils.GetComboItemText(_objWindow._cboConfigPlatform);


            string[] args = new string[3];
            args[0] = StringUtils.MakeArg("/o", BuildUtils.Enquote(GetBuildDirectory()));//dummy path
            args[1] = StringUtils.MakeArg("/cn", BuildUtils.Enquote(cn));
            args[2] = StringUtils.MakeArg("/cp", BuildUtils.Enquote(cp));

            BuildConfig.ParseArgs(args.ToList());
            BuildConfig.LoadConfig();
        }
        private void ResetAllDisplayItems()
        {
            
            _objWindow._statusBar.Background = UiBrushes.Brush_BuildColor_Window;
            _objWindow._btnBuild.IsEnabled = false;
            _objWindow._btnCleanBuild.IsEnabled = false;
            _objWindow._btnCancelBuild.IsEnabled = true;
            _objWindow._btnBarStart.IsEnabled = false;
            _objWindow._btnBarStop.IsEnabled = true;

            ClearConsoleWindow();

            UpdateAgentDisplayBoxesWithAgentStatus();

            _objFileTree.Reset();
            _objWindow.EnableUserModification(false);
        }

        private string GetBuildArgs(bool clean, string strSpartanBuildDir, string configName, string configPlatform, int buildId, string[] agentNames, int maxErrorLimit)
        {
            string strArgs = string.Empty;
            strArgs += StringUtils.MakeArg(BuildFlags.AttachBuildProcessesToUI, "true");
            strArgs += StringUtils.MakeArg(BuildFlags.CoordProgram);
            strArgs += StringUtils.MakeArg(BuildFlags.BuildDir, BuildUtils.Enquote(strSpartanBuildDir));
            strArgs += StringUtils.MakeArg(BuildFlags.ConfigName, BuildUtils.Enquote(configName));
            strArgs += StringUtils.MakeArg(BuildFlags.ConfigPlatform, BuildUtils.Enquote(configPlatform));
            strArgs += StringUtils.MakeArg(BuildFlags.BuildId, buildId.ToString());
            strArgs += StringUtils.MakeArg(BuildFlags.MaxErrorLimit, BuildUtils.Enquote(maxErrorLimit.ToString()));

            foreach(string agent in agentNames)
                strArgs += StringUtils.MakeArg(BuildFlags.AgentName, BuildUtils.Enquote(agent));

            if (clean == true)
                strArgs += StringUtils.MakeArg(BuildFlags.Clean);

            return strArgs;
        }
        private System.Diagnostics.Process GetCoordinatorProcess()
        {
            System.Diagnostics.Process[] px = System.Diagnostics.Process.GetProcessesByName(
                SpartanGlobals.CoordinatorExeName);
            if (px.Length == 0)
                return null;
            return px[0];
        }
        private void RunBuildProcess(string name, string args)
        {

            _objShellProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo inf = new System.Diagnostics.ProcessStartInfo(name, args);
            _objShellProcess.StartInfo = inf;


            if (_objWindow._chkShowCommandWindow != null &&
                _objWindow._chkShowCommandWindow.IsChecked != null &&
                _objWindow._chkShowCommandWindow.IsChecked == true)
            {
               // ShowWindow(_objShellProcess.MainWindowHandle, 2);
                inf.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            }
            else
            {
                inf.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
               // ShowWindow(_objShellProcess.MainWindowHandle, 0);
            }
           
            _objShellProcess.Start();
        }
        public void KillCurrentBuildProcesses()
        {
            System.Diagnostics.Process[] procs;

            try
            {
                //Note - let this fail - - the coordinator now terminates itself if it is attached to the UI
                procs = System.Diagnostics.Process.GetProcessesByName(SpartanGlobals.CoordinatorExeName);
                foreach (System.Diagnostics.Process proc in procs)
                {
                    Globals.Logger.LogInfo("Killing existing build process.");

                    proc.Kill();
                }

                procs = System.Diagnostics.Process.GetProcessesByName(SpartanGlobals.RomulusExeName);
                foreach (System.Diagnostics.Process proc in procs)
                {
                    Globals.Logger.LogInfo("Killing existing build process.");

                    proc.Kill();
                }
            }
            catch (Exception)
            {
                //Globals.Logger.LogError("Failed to kill build process: " + Ex.ToString()) ;
            }
        }
        private void LoadSettings()
        {
            // Pre cleanup
            KillCurrentBuildProcesses();
            
            // ** reset all dependent settings values.
            _objWindow._lstAgents.Items.Clear();
            //_lstAgentVisualData.Clear();

            _objSettings = new BuildGuiSettings(_objWindow._txtBuildDirectory.Text, _objWindow._txtSpartanExePath.Text);
            _objSettings.Load();

            //Get the sln. name of the project
            GetSolutionName();

            _objFileTree = new FileTree(this);

            _objWindow._txtMaxErrorLimit.Text = _objSettings.MaxErrorLimit.ToString();

            // GUI Objects
            _objWindow._txtBuildDirectory.Text = _objSettings.BuildPath;
            _objWindow._txtSpartanExePath.Text = _objSettings.ExePath;
            foreach (AgentInfo inf in _objSettings.Agents)
            {
                _objWindow._lstAgents.Items.Add(inf.Name);
            }

            if (!string.IsNullOrEmpty(_strSolutionName))
                _objWindow._lblVisualStudioSolutionName.Content = _strSolutionName;
            else
                _objWindow._lblVisualStudioSolutionName.Content = "(Not Found)";
            
            _objWindow._chkJumpToError.IsChecked = _objSettings.JumpToError;
            _objWindow._chkShowCommandWindow.IsChecked = _objSettings.ShowCommandWindow;
            _objWindow._chkShowConsole.IsChecked = _objSettings.ShowConsole;
            _objWindow.ShowConsole(_objSettings.ShowConsole);

            CreateControlsFromSettings();
        }
        private void GetSolutionName()
        {
            _strSolutionName = "";
            string strPath = System.IO.Path.Combine(_objSettings.BuildPath, "projects.cfg");
            try
            {
                ProjectsFile pf = new ProjectsFile();
                pf.Load(strPath);
                _strSolutionName = pf.GlobalConfig.VsSolutionName;
                pf = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                BuildUtils.ShowErrorMessage("Error initializing application.  Could Not Load Projects File, or VS Solution name was invalid." 
                    +"  Check projects.cfg and its location.  Visual Studio integration may fail.\n\nError Description:\n" + ex.ToString());
            }
        }
        private void CreateControlsFromSettings()
        {
            RefreshAgentDataControls();
        }
        private void BuildUpdateCallback(object sender, EventArgs e)
        {
            UpdateBuild();
        }
        private void UpdateBuild()
        {
            //synclock outside the other
            string strTemp;

            //Updates and parses output into junk
            strTemp = _objLogWatcherBuildOutput.GetChangedData(true);
            strTemp = strTemp.Trim();
            if (!string.IsNullOrEmpty(strTemp))
                UpdateOutputAsync(strTemp);

            //Shows in the output window
            strTemp = _objLogWatcherCoordOutput.GetChangedData(true);
            if (!string.IsNullOrEmpty(strTemp))
                UpdateCoordOutputAsync(strTemp);

            UpdateCheckCoordinatorIsRunning();

            System.Windows.Forms.Application.DoEvents();
        }
        private void UpdateCheckCoordinatorIsRunning()
        {
            //If the coordinator is dead than kill our build.
            if (!SpartanGlobals.CoordinatorIsRunning() && IsBuilding)
            {
                Globals.Logger.LogInfo("Coordinator exited unexpectedly.  Terminating build process.");
                EndBuild();
            }
        }
        private void RefreshAgentDataControls()
        {
            _objAgentVisualData.RefreshAgentDisplay(Settings.Agents);
        }
        private void UpdateOutputAsync(string str)
        {
            lock (_objLogDataLockObject)
            {
                string[] lines = str.Split('\n');
                for (int iLine = 0; iLine < lines.Length; iLine++)
                {
                    string line = lines[iLine].Trim();
                    if (line == string.Empty)
                        continue;

                    _objFileTree.UpdateTreeViewByLogLine(line);
                }

                _objFileTree.ScrollToBottomOfFileTree();
            }
        }
        private void UpdateProgress(string line)
        {
            System.Text.RegularExpressions.MatchCollection matches;

            matches = System.Text.RegularExpressions.Regex.Matches(
                                                            line,
                                                            "\\([0-9]+\\/[0-9]+\\)"
                                                            );
            if (matches.Count > 0)
            {
                    
                int a, b;
                try
                {
                    string str = matches[0].Groups[0].Value.Substring(1, matches[0].Groups[0].Value.Length - 2);
                    string[] x = str.Split('/');

                    a = Int32.Parse(x[0]);
                    b = Int32.Parse(x[1]);
                    UpdateProgressBar(a, b);
                }
                catch (Exception ex)
                {
                    //swallow parse failures
                }
                
            }
           // int lineCount = System.IO.File.ReadLines("C:\\p4\\dev\\bro\\tmp\\debug\\win32\\make.bat").Count();
          //  UpdateProgressBar(_intTreeViewOutputFileNodeCount, lineCount);

        }
        private void UpdateCoordOutputAsync(string data)
        {
            LogOutput(data);

            string[] lines = data.Split('\n');
            for (int iLine = 0; iLine < lines.Length; iLine++)
            {
                string line = lines[iLine].Trim();
                if (line == string.Empty)
                    continue;
                UpdateBuildInformationFromLogData(line);
                UpdateProgress(line);
            }
        }
        private void UpdateProgressBar(int a, int b)
        {
            lock (_objProgressBarLockObject)
            {
                _objWindow._pgbBuildProgress.Minimum = 0;
                _objWindow._pgbBuildProgress.Maximum = b;
                _objWindow._pgbBuildProgress.Value = a;
            }
        }
        private void UpdateBuildInformationFromLogData(string line)
        {
            string fileName, agentName, reservedId;
            string cpuId;
            
            // %< or %>  AgentName|ProcessorId|FileName|ReservationID
            if (line.Contains("%<"))
            {
                string[] values = line.Split('|');
                agentName = values[0].Split('<')[1];
                cpuId = values[1];
                fileName = values[2];
                reservedId = values[3];
                //TODO: we don't need this anymore, but it is good to have this information just in case.
                //AgentDataFileComplete(agentName, cpuId, fileName, reservedId);
            }
            else if (line.Contains("%>"))
            {
                string[] values = line.Split('|');
                agentName = values[0].Split('>')[1];
                cpuId = values[1];
                fileName = values[2];
                reservedId = values[3];
                //TODO: we don't need this anymore, but it is good to have this information just in case.
                //AgentDataFileSent(agentName, cpuId, fileName, reservedId);
            }
            else if (line.Contains("%*"))
            {
                EndBuild();
            }
        }
        private void VerifySpartanGlobalsInitialized()
        {
            if (SpartanGlobals.GlobalsInitialized == false)
                SpartanGlobals.InitializeGlobals(new List<string>() { "/a", "/o:\"\\\\DP0001\\C$\\p4\\dev\\bro\\bld\"" });
        }
        private void SendMsgToAgent(string name, string data)
        {
            VerifySpartanGlobalsInitialized();
            Spartan.CoordinatorAgent ag = new CoordinatorAgent(name);

            try
            {
                bool? success = ag.Connect(true);

                if (success == true)
                {
                    LogOutput("Sent Reset to " + name);
                    ag.Send(data);
                }
                else if (success == false)
                {
                    LogOutput("Failed to restrt agent " + name);
                }
                else
                {
                    LogOutput("Failed to restrt agent, and there was a null value: " + name);
                }
                ag.Disconnect();
                ag = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                LogOutput("Could not reset agent " + name + ":\n" + ex.ToString());
            }

        }

        private void AgentUpdateEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (_objAgentsBeingUpdatedLockObject)
            {
                if (_blnAgentsBeingUpdated)
                    return;
                _blnAgentsBeingUpdated = true;
            }

            VerifySpartanGlobalsInitialized();
            Settings.UpdateAgents();
            RefreshAgentDataControls();

            lock (_objAgentsBeingUpdatedLockObject)
              _blnAgentsBeingUpdated = false;

            
        }

        #endregion

    }
}
