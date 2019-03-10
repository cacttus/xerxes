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
//using System.Runtime.InteropServices;
//using System.Threading;
//using System.Windows.Forms;

namespace BuildGui
{
    public enum AppStatus { None, Initialized, Error }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region INTEROP
        [System.Runtime.InteropServices.DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        #region Private: Members

        private BuildGuiController _objBuildGuiController;
        private System.Windows.Threading.DispatcherTimer _objTimer;
        private int AgentInfoUpdateIntervalMilliseconds = 400;
        private WebVideo _objWebVideo;
        private bool _blnIsInitializing = false;
        Dictionary<Control, bool> _objDisabledTabPageItems = null;

        private BuildGuiSettings _objTempSettings = null;
        #endregion

        #region Public: Methods

        public MainWindow()
        {
            InitializeComponent();

            //Allow us to pass commands to the running window, if it exists.
            if (!RunCommandsAndCloseIfDuplicate())
                return;

            
            this.Loaded += new RoutedEventHandler(OnLoad);
            

            _objTimer = new System.Windows.Threading.DispatcherTimer();
            _objTimer.Interval = new TimeSpan(0, 0, 0, 0, AgentInfoUpdateIntervalMilliseconds);
            _objTimer.Tick += new EventHandler(BuildWindowUpdateCallback);
            _objTimer.Start();

            ((System.Collections.Specialized.INotifyCollectionChanged)_lstAgents.Items).CollectionChanged += AgentsCollectionChanged;

            _btnCancel_Save.IsEnabled = false;
            _btnSave.IsEnabled = false;
        }

        public void ShowConsole(bool bShow)
        {
            if (bShow == true)
                ShowConsole();
            else
                HideConsole();
        }

        #endregion

        #region Private: Gui Callbacks

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _objBuildGuiController.EndBuild();
            _objBuildGuiController.KillCurrentBuildProcesses();
        }
        private void _btnCleanBuild_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.StartBuild(true);
        }
        private void _btnBuild_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.StartBuild(false);
        }
        private void _btnCancelBuild_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.EndBuild();
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.EndBuild();
            Application.Current.Shutdown();
        }
        private void _btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        private void _btnCancel_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings(true);
        }
        private void _btnRemove_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.RemoveAgentDefinition();
            SettingsChanged();
        }
        private void _btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddAgentText();
        }
        private void MainWindowKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5 || e.Key == Key.F7)
            {
                _objBuildGuiController.StartBuild(false);
                if (e.Key == Key.F5)
                    _objBuildGuiController.SetDebugAfterBuild();
            }
            if (e.Key == Key.F4)
            {
                _objBuildGuiController.ResetAllAgents();
            }
        }
        private void _btnAddAgent_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddAgentText();
            }
        }
        private void _btnCopyAgents_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.ReinstallAllAgents();
        }
        private void _btnInstallAgent_Click(object sender, RoutedEventArgs e)
        {
            if (_lstAgents.SelectedItem != null)
            {
                string str = _lstAgents.SelectedItem.ToString();
                _objBuildGuiController.ReinstallAllAgents(str);
            }
        }
        private void _btnHideOutput_Click(object sender, RoutedEventArgs e)
        {
            if (_txtOutput.IsVisible)
                _txtOutput.Visibility = System.Windows.Visibility.Hidden;
            else
                _txtOutput.Visibility = System.Windows.Visibility.Visible;
        }
        private void _btnRestartAgentService_Click(object sender, RoutedEventArgs e)
        {
            string str ="";
            if (_lstAgents.SelectedItem != null)
            {
                str = _lstAgents.SelectedItem.ToString();

                try
                {
                    _objBuildGuiController.LogOutput("Locating agent " + str);
                    System.ServiceProcess.ServiceController service
                        = new System.ServiceProcess.ServiceController("AgentService", str);
                    service.Stop();
                    service.Start();
                }
                catch (Exception ex)
                {
                    _objBuildGuiController.LogOutput("Could not restart service:\n" + ex.ToString());
                }
            }

        }
        private void _btnStopAgentService_Click(object sender, RoutedEventArgs e)
        {
            string str = _lstAgents.SelectedItem.ToString();
            try
            {
                _objBuildGuiController.LogOutput("Locating agent " + str);
                System.ServiceProcess.ServiceController service
                    = new System.ServiceProcess.ServiceController("AgentService", str);
                service.Stop();
            }
            catch (Exception ex)
            {
                _objBuildGuiController.LogOutput("Could not stop service:\n" + ex.ToString());
            }
        }
        private void _btnStartAgentService_Click(object sender, RoutedEventArgs e)
        {
            string str = _lstAgents.SelectedItem.ToString();
            

                Task.Factory.StartNew(() => {
                    try
                    {
                        _objBuildGuiController.LogOutput("Locating agent: " + str);
                        System.ServiceProcess.ServiceController service
                            = new System.ServiceProcess.ServiceController("AgentService", str);
                        service.Start();
                        _objBuildGuiController.LogOutput("Agent started: " + str);
                    }
                    catch (Exception ex)
                    {
                        _objBuildGuiController.LogOutput("Could not start service for " + str + "\n" + ex.ToString());
                    }
                });
            
        }
        private void _btnResetAllAgents_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.ResetAllAgents(true);
        }
        private void _btnResetAllAgentsFast_Click(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.ResetAllAgents(false);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.StartBuild(false);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.ResetAllAgents(false);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _objBuildGuiController.EndBuild();
        }
        private void _chkShowConsole_Checked(object sender, RoutedEventArgs e)
        {
            //BGC is null when window loads and this checked is called by default
            if (_objBuildGuiController == null)
                return;
            GetTempSettings().ShowConsole = true;
            ShowConsole();

            SettingsChanged();

        }
        private void _chkShowConsole_Unchecked(object sender, RoutedEventArgs e)
        {
            //BGC is null when window loads and this checked is called by default
            if (_objBuildGuiController == null)
                return;
            GetTempSettings().ShowConsole = false;
            HideConsole();
            SettingsChanged();
        }
        private void _chkShowCommandWindow_Checked(object sender, RoutedEventArgs e)
        {
            //BGC is null when window loads and this checked is called by default
            if (_objBuildGuiController == null)
                return;
            GetTempSettings().ShowCommandWindow = true;
            ShowConsole();
            SettingsChanged();

        }
        private void _chkShowCommandWindow_Unchecked(object sender, RoutedEventArgs e)
        {
            //BGC is null when window loads and this checked is called by default
            if (_objBuildGuiController == null)
                return;
            GetTempSettings().ShowCommandWindow = false;
            SettingsChanged();
        }
        private void _chkJumpToError_Checked(object sender, RoutedEventArgs e)
        {
            //BGC is null when window loads and this checked is called by default
            if (_objBuildGuiController == null)
                return;
            GetTempSettings().JumpToError = true;
            SettingsChanged();
        }
        private void _chkJumpToError_Unchecked(object sender, RoutedEventArgs e)
        {
            //BGC is null when window loads and this checked is called by default
            if (_objBuildGuiController == null)
                return;
            GetTempSettings().JumpToError = false;
            SettingsChanged();
        }

        private void _cboConfigName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = WpfUtils.GetComboItemText(_cboConfigName);
            if (SettingsLoaded())
            {
                GetTempSettings().SelectedConfigName = str;
                SaveSettings();
            }
        }
        private void _cboConfigPlatform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = WpfUtils.GetComboItemText(_cboConfigPlatform);
            if (SettingsLoaded())
            {
                GetTempSettings().SelectedConfigPlatform = str;
                SaveSettings();
            }
        }
        private void _txtSpartanExePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SettingsLoaded())
                SettingsChanged();
        }
        private void _txtBuildDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SettingsLoaded())
                SettingsChanged();
        }
        private void _lstAgents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion
        #region Private: Methods
        
        private bool RunCommandsAndCloseIfDuplicate()
        {
            //** return true if this window is not a duplicate (it is a main window).
            string[] args = Environment.GetCommandLineArgs();
            System.Diagnostics.Process[] plist = System.Diagnostics.Process.GetProcessesByName("BuildGui");

            //if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Process[] plist2 = System.Diagnostics.Process.GetProcessesByName("BuildGui.vshost");
                plist = plist.Concat(plist2).ToArray();
            }

            foreach (System.Diagnostics.Process proc in plist)
            {
                //If we are current process
                if (proc.Id == System.Diagnostics.Process.GetCurrentProcess().Id)
                    continue;

                // *** args must be in format: /command:parameter1:parameter2, no spaces
                foreach (string arg in args)
                {
                    PacketMakerUdp objUdp = new PacketMakerUdp();
                    objUdp.SendAsync(arg, System.Environment.MachineName, BuildMonitorGlobals.ProgramCommandUdpPortRecv);
                }

                // Exit because we don't want more than one instance up.
                System.Environment.Exit(0);
                return false;
            }

            return true;
        }
        private void BuildWindowUpdateCallback(object sender, EventArgs e)
        {
          //  _objBuildGuiController.CheckCommands();
            _objBuildGuiController.UpdateAgentDisplayBoxesWithAgentStatus();
        }
        private void OnLoad(object sender, RoutedEventArgs e)
        {

            _blnIsInitializing = true;
            
            _objBuildGuiController = new BuildGuiController(this);
            _objBuildGuiController.Init();

            if (_objBuildGuiController.Settings.SelectedConfigName.Trim() == string.Empty)
            {
                string nameText = WpfUtils.GetComboItemText(_cboConfigName);
                _objBuildGuiController.Settings.SelectedConfigName = nameText;
                Globals.Logger.LogInfo("Settings configuration name not set.  Setting to default of :" + nameText);
                _objBuildGuiController.Settings.Save();
            }
            if (_objBuildGuiController.Settings.SelectedConfigPlatform.Trim() == string.Empty)
            {
                string platText = WpfUtils.GetComboItemText(_cboConfigPlatform);
                _objBuildGuiController.Settings.SelectedConfigPlatform = platText;
                Globals.Logger.LogInfo("Settings platform not set.  Setting to default of :" + platText);
                _objBuildGuiController.Settings.Save();
            }

            _blnIsInitializing = false;

            // Start web video
            //_objWebVideo = new WebVideo(_objMediaElement);
            //_objWebVideo.Start();
           

            base.OnActivated(e);
        }
        private void AddAgentText()
        {
            if (!string.IsNullOrEmpty(_txtAgentName.Text))
                _objBuildGuiController.AddAgentDefinition(_txtAgentName.Text);
            _txtAgentName.Text = "";
            SettingsChanged();
        }
        private Point GetPos(Visual vo)
        {
            return vo.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
        }
        private void ShowConsole()
        {
            if (_txtOutput.Visibility == Visibility.Visible)
                return;

            _txtOutput.Visibility = Visibility.Visible;
            this.Height += _txtOutput.ActualHeight;
            _tabControl.Height = Double.NaN; // Microsoft's "unique" way of setting auto properties
            Thickness th = _tabControl.Margin;
            _tabControl.Margin = new Thickness(th.Left, th.Top, th.Right, _txtOutput.Height + _statusBar.Height + 20);

        }
        private void HideConsole()
        {
            if (_txtOutput.Visibility == Visibility.Hidden)
                return;

            _txtOutput.Visibility = Visibility.Hidden;
            this.Height -= _txtOutput.ActualHeight;
            _tabControl.Height = Double.NaN; // Microsoft's "unique" way of setting auto properties
            Thickness th = _tabControl.Margin;
            _tabControl.Margin = new Thickness(th.Left, th.Top, th.Right, _statusBar.Height + 20);


        }

        #region Private: Methods: Settings
        public void EnableUserModification(bool blnEnabled)
        {
            _objDisabledTabPageItems = WpfUtils.EnableTabPage("Settings", _tabControl, blnEnabled, _objDisabledTabPageItems, true);
        }
        private void SettingsChanged()
        {
            if (_blnIsInitializing)
                return;
            _btnSave.Foreground = Brushes.Red;
            _btnSave.FontWeight = FontWeights.Bold;
            _btnCancel_Save.IsEnabled = true;
            _btnSave.IsEnabled = true;

            TabItem selected = (TabItem)_tabControl.SelectedItem;
            string selectedHeader = (string)selected.Header;
            foreach (TabItem ti in _tabControl.Items)
            {
                string strHeader = (string)ti.Header;
                if (!strHeader.Equals(selectedHeader))
                    ti.IsEnabled = false;
            }
            _statusBar.IsEnabled = false;
        }
        private bool SettingsLoaded()
        {
            return _objBuildGuiController != null && _objBuildGuiController.Settings != null;
        }
        private void SettingsSavedOrDiscarded()
        {
            //Reenable everything.
            _btnSave.Foreground = Brushes.Black;
            _btnSave.FontWeight = FontWeights.Normal;
            _btnCancel_Save.IsEnabled = false;
            _btnSave.IsEnabled = false;
            foreach (TabItem ti in _tabControl.Items)
                    ti.IsEnabled = true;
            _statusBar.IsEnabled = true;
        }
        private void SaveSettings(bool blnDiscard = false)
        {
            if (_objTempSettings == null)
            {
                SettingsSavedOrDiscarded();
                _objBuildGuiController.ReloadSettings();
                return;
            }

            //Commit & dispose
            if (blnDiscard != true)
            {
                _objBuildGuiController.Settings = _objTempSettings;
                _objBuildGuiController.Settings.Save();
                _objBuildGuiController.LogOutput("Settings saved");
            }

            _objTempSettings = null;

            _objBuildGuiController.ReloadSettings();
            SettingsSavedOrDiscarded();
        }
        public BuildGuiSettings GetTempSettings()
        {
            if (_objTempSettings == null)
                _objTempSettings = _objBuildGuiController.Settings.GetCopy();
            return _objTempSettings;
        }

        #endregion
        private void _txtBuildDirectory_preview(object sender, TextCompositionEventArgs e)
        {
            GetTempSettings().BuildPath = _txtBuildDirectory.Text.Trim();
            SettingsChanged();
        }
        private void _txtSpartanExePath_preview(object sender, TextCompositionEventArgs e)
        {
            GetTempSettings().ExePath = _txtSpartanExePath.Text.Trim();
            SettingsChanged();
        }
        private string _txtMaxErrorLimitLastValue = "";
        private void _txtMaxErrorLimit_preview(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)sender;
            _txtMaxErrorLimitLastValue = _txtMaxErrorLimit.Text;
        }
        private void _txtMaxErrorLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_objBuildGuiController == null)
                return;

            string strDefault = "1";
            if (_txtMaxErrorLimit.Text == strDefault)
            {
                GetTempSettings().MaxErrorLimit = Convert.ToInt32(_txtMaxErrorLimit.Text.Trim());
                SettingsChanged();
                return;
            }

            if (WpfUtils.VerifyNumericText(_txtMaxErrorLimit.Text) == false) 
            {
                if (_txtMaxErrorLimit.Text == _txtMaxErrorLimitLastValue) 
                    _txtMaxErrorLimit.Text = strDefault;
                else
                    _txtMaxErrorLimit.Text = _txtMaxErrorLimitLastValue;
            }
            else 
            {
                GetTempSettings().MaxErrorLimit = Convert.ToInt32(_txtMaxErrorLimit.Text.Trim());
                SettingsChanged();
            }
        }
        private void NumericTextPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!WpfUtils.VerifyNumericText(text))
                    e.CancelCommand();
                else
                    SettingsChanged();
            }
            else
                e.CancelCommand();
        }
        private void AgentsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_objBuildGuiController == null)
                return;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldStartingIndex == -1)
                    return;
                if (e.OldItems.Count != 1)
                    return;
                string agentName = (string)e.OldItems[0];
                GetTempSettings().DeleteAgent(agentName);
                SettingsChanged();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex== -1)
                    return;
                if (e.NewItems.Count != 1)
                    return;
                string agentName = (string)e.NewItems[0];
                GetTempSettings().CreateAgent(agentName);
                SettingsChanged();
            }
        }

        #endregion




    }
}




//private static bool QuickBestGuessAboutAccessibilityOfNetworkPath(string path)
//{
//    string output;
//    System.Diagnostics.ProcessStartInfo pinfo;
//    string pathRoot;

//    if (string.IsNullOrEmpty(path))
//        return false;

//    pathRoot = System.IO.Path.GetPathRoot(path);

//    if (string.IsNullOrEmpty(pathRoot))
//        return false;

//    pinfo = new System.Diagnostics.ProcessStartInfo("net", "use");

//    pinfo.CreateNoWindow = true;
//    pinfo.RedirectStandardOutput = true;
//    pinfo.UseShellExecute = false;


//    using (System.Diagnostics.Process p = System.Diagnostics.Process.Start(pinfo))
//    {
//        output = p.StandardOutput.ReadToEnd();
//    }
//    foreach (string line in output.Split('\n'))
//    {
//        if (line.Contains(pathRoot) && line.Contains("OK"))
//        {
//            return true; // shareIsProbablyConnected
//        }
//    }
//    return false;
//}