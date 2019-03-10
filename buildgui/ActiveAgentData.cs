//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BuildGui
//{
//    public class ActiveAgentData
//    {
//        GridView _objGrid;
//        public AgentInfo AgentInfo { get; set; }
//        System.Windows.Threading.DispatcherTimer _objBuildUpdateTimer;

//        public class ListViewItem
//        {
//            public string FileName { get; set; }
//            public TimeSpan UpTime { get; set; }
//            public DateTime SendTime { get; set; }
//        }
//        //public void ClearStats()
//        //{
//        //    _lsvFileStats.Items.Clear();
//        //}
//        public void StartUpdate()
//        {
//            _objBuildUpdateTimer = new System.Windows.Threading.DispatcherTimer();
//            _objBuildUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
//            _objBuildUpdateTimer.Tick += new EventHandler(UpdateListView);
//            _objBuildUpdateTimer.Start();
//        }
//        public void StopUpdate()
//        {
//            if (_objBuildUpdateTimer!=null)
//                _objBuildUpdateTimer.Stop();
//            _objBuildUpdateTimer = null;

//        }
//        private void AddListViewColumn(string name, string binding, int width_percent)
//        {
//            Binding b = new Binding(binding);
//            b.StringFormat = @"{0:mm\:ss\:fff}";

//            GridViewColumn gv = new GridViewColumn(){
//                Header = name,
//                DisplayMemberBinding = b
//            };

//            float width = 500;

//            gv.Width = (int)((float)width * (float)width_percent/100.0);

//            _objGrid.Columns.Add(gv);
//        }
//        public AgentVisualData()
//        {
//            InitializeComponent();

//            _objGrid = new GridView();
//            _lsvFileStats.View = _objGrid;
//            AddListViewColumn("Item", "FileName", 60);
//            AddListViewColumn("Time", "UpTime", 40);

//            _objBuildUpdateTimer = new System.Windows.Threading.DispatcherTimer();
//            _objBuildUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
//            _objBuildUpdateTimer.Tick += new EventHandler(UpdateListView);
//            _objBuildUpdateTimer.Start();
//        }
//        private void UpdateListView(object sender, EventArgs e)
//        {
//            foreach (ListViewItem item in _lsvFileStats.Items)
//                item.UpTime = DateTime.Now - item.SendTime;
//            _lsvFileStats.Items.Refresh();
//        }
//        public void FileSent(string fileName)
//        {
//            _lsvFileStats.Items.Add(new ListViewItem { FileName = fileName, UpTime = TimeSpan.Zero, SendTime = DateTime.Now });
//        }
//        public void FileComplete(string fileName, string requestId)
//        {
//            ListViewItem foundItem = null;
//            foreach (ListViewItem item in _lsvFileStats.Items)
//            {
//                if (item.FileName == fileName)
//                {
//                    foundItem = item;
//                    break;
//                }
//            }

//            if (foundItem == null)
//                Globals.Logger.LogError("File " + fileName + "not found after being compiled.  there was a problem in the string.",false);

//            _lsvFileStats.Items.Remove(foundItem);

//        }
//        private void _lsvFileStats_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {

//        }
//    }
//}
