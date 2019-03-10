using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Trex
{
    public partial class TrexService : ServiceBase
    {
        System.ComponentModel.BackgroundWorker _objBackgroundWorker;

        public TrexService()
        {
            InitializeComponent();
        }
        private void LogEventViewerEvent(string str)
        {
            string sSource;
            string sLog;
            string sEvent;

            sSource = System.Reflection.Assembly.GetEntryAssembly().FullName;
            sLog = "Application";
            sEvent = str;

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            EventLog.WriteEntry(sSource, sEvent);
            EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Warning, 234);
        }
        public void ProcessCallback(object sender, DoWorkEventArgs e)
        {
            LogEventViewerEvent("Callbabck found 324567896");
            TrexUtils.RunServer();
        }
        protected override void OnStart(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(
                System.AppDomain.CurrentDomain.BaseDirectory
              );
            _objBackgroundWorker = new BackgroundWorker();
            _objBackgroundWorker.DoWork += new DoWorkEventHandler(ProcessCallback);
            _objBackgroundWorker.WorkerSupportsCancellation = true;
            _objBackgroundWorker.RunWorkerAsync();
            //_objThread = new System.Threading.Thread(ProcessCallback);
            //_objThread.Start();
        }
        protected override void OnStop()
        {
            Globals.Logger.LogInfo("Service is stopping.");
            _objBackgroundWorker.CancelAsync();
            RequestAdditionalTime(3000);
        }
    }
}
