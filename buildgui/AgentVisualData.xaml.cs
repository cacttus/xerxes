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
    /// <summary>
    /// Interaction logic for AgentVisualData.xaml
    /// </summary>
    public partial class AgentVisualData : UserControl
    {
        GridView _objGrid;
        System.Windows.Threading.DispatcherTimer _objTimer;
        private Object _updateBoolLockObject = new Object();
        private bool _blnUpdating = false;

        public class ListViewItem
        {
            public string AgentName { get; set; }
            public string AgentCpuId { get; set; }
            public string FileName { get; set; }
            public string UpTime { get; set; }
            public DateTime SendTime { get; set; }
            public bool IsConnected { get; set; }
        }
        private bool IsUpdating
        {
            get { lock (_updateBoolLockObject) return _blnUpdating; }
            set { lock (_updateBoolLockObject) _blnUpdating = value; }
        }
        public void RefreshAgentDisplay(List<AgentInfo> agents)
        {
            // Called by the build gui controller every time we query the agent
            //which should happen every 500ms or so.
            List<AgentInfo> copied = new List<AgentInfo>(agents);

            _lsvFileStats.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (IsUpdating == true)
                    return;

                IsUpdating = true;

                ListViewItem lvi;

                _lsvFileStats.Items.Clear();
                _lsvFileStats.HorizontalAlignment           = HorizontalAlignment.Stretch;
                _lsvFileStats.HorizontalContentAlignment    = HorizontalAlignment.Stretch;
                _lsvFileStats.VerticalAlignment             = VerticalAlignment  .Stretch;
                _lsvFileStats.VerticalContentAlignment      = VerticalAlignment  .Stretch;

                foreach (AgentInfo inf in copied)
                {
                    if (inf.IsConnected)
                    {
                        foreach (AgentCpuInfo cpu in inf.Cpus)
                        {
                            lvi = new ListViewItem
                            {
                                AgentName = inf.Name,
                                AgentCpuId = cpu.CpuId.ToString(),
                                FileName = cpu.WorkingFileName,
                                UpTime = cpu.FileUpTime,
                                SendTime = cpu.FileSendTime,
                                IsConnected = inf.IsConnected
                            };
                            _lsvFileStats.Items.Add(lvi);
                        }
                    }
                    else
                    {
                        lvi = new ListViewItem
                        {
                            AgentName = inf.Name,
                            AgentCpuId = "",
                            FileName = "Not Connected",
                            UpTime = "",
                            SendTime = DateTime.MinValue,
                            IsConnected = inf.IsConnected
                        };
                        _lsvFileStats.Items.Add(lvi);

                    }

                }
                

                UpdateListView();

                IsUpdating = false;
            }));
        }
        private void AddListViewColumn(string name, string binding, int width_percent, string format)
        {
            Binding b   = new Binding(binding);
            float width = 500;

            b.StringFormat = format;

            GridViewColumn gv = new GridViewColumn()
            {
                Header               = name,
                DisplayMemberBinding = b
            };

            gv.Width = (int)((float)width * (float)width_percent/100.0);

            _objGrid.Columns.Add(gv);
        }
        public AgentVisualData()
        {
            InitializeComponent();

            _objGrid = new GridView();

            _lsvFileStats.View = _objGrid;

            AddListViewColumn("Agent", "AgentName", 12, "{0}");
            AddListViewColumn("Cpu", "AgentCpuId", 5, "{0}");
            AddListViewColumn("Item", "FileName", 40, "{0}");
            AddListViewColumn("Time", "UpTime", 30, "{0}");
            
            _lsvFileStats.ItemContainerGenerator.ItemsChanged += ItemsChangedCallback;
        }
        private void ItemsChangedCallback(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            // Sets colors of agents
            
            foreach (ListViewItem lsv in _lsvFileStats.Items)
            {
                _lsvFileStats.UpdateLayout();
             //   _lsvFileStats.ScrollIntoView(lsv);
                ListBoxItem lbi = (ListBoxItem)_lsvFileStats.ItemContainerGenerator.ContainerFromItem(lsv);
                if (lbi != null)
                {
                    if (lsv.IsConnected)
                    {
                        lbi.Foreground = Brushes.Green;
                        lbi.Background = Brushes.Green;
                    }
                    else
                    {
                        lbi.Foreground = Brushes.Red;
                        lbi.Background = Brushes.Red;
                    }
                }

            }
        }
        private void UpdateListView()
        {
            foreach (ListViewItem item in _lsvFileStats.Items)
            {
                if (item.SendTime == DateTime.MinValue)
                    item.UpTime = "";
                else
                    item.UpTime = (DateTime.Now - item.SendTime).ToString(@"mm\:ss");
            }
            _lsvFileStats.Items.Refresh();
        }



    }
}
