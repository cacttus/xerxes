using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    //Watches logs
    public class LogWatcher
    {
        private string _strFilePath;
        private int _intPollInterval;
        private System.ComponentModel.BackgroundWorker _objBackgroundWorker;
        private System.Windows.Threading.DispatcherTimer _objUpdateTimer;
        private DateTime _datLastFileUpdateTime = DateTime.MinValue;
        private int _intLastFileSize = 0;
        private bool _blnReload = false;
       // private int _maxBufferSize = 256000000; // 256MB

        private Object _objLogDataLockObject = new Object();
        private string _strLogData;
        public string LogData
        {
            get
            {
                lock (_objLogDataLockObject)
                {
                    return _strLogData;
                }
            }
            set
            {
                lock (_objLogDataLockObject)
                {
                    _strLogData = value;
                }
            }

        }
        private bool _blnHasChanged = false;
        private Object _objFilePathLockObject = new Object();
        public string FilePath
        {
            get
            {
                lock (_objFilePathLockObject)
                {
                    return _strFilePath;
                }
            }
            set
            {
                lock (_objFilePathLockObject)
                {
                    _strFilePath = value;
                }
                _intLastFileSize = 0;
            }
        }

        public LogWatcher(string strPath, 
            int pollIntervalMilliseconds = 500, 
            bool reloadWholeFileEachTime = false // Not recommended - reloads file each time we update (if the length is greater) instead of appending new text
            //int maxBufferSizeInMemory = -1  // -1 for unlimited.
            )
        {
            _strFilePath = strPath;
            _intPollInterval = pollIntervalMilliseconds;
            _blnReload = reloadWholeFileEachTime;
           // _maxBufferSize = maxBufferSizeInMemory;
            if (!System.IO.File.Exists(strPath))
            {
                Globals.Logger.LogError(
                    "[LogWatcher] - Log file at path '"
                    + strPath
                    + "' does not exist (yet). "
                    + " Log watcher may not update correctly. ",
                    false
                    );
            }
        }
        private bool _blnFlush = false;
        public Object _objFlushLockObject = new Object();
        private bool IsFlushPending 
        { 
            get
            {
                lock(_objFlushLockObject)
                    return _blnFlush;
            } 
            set
            {
                lock (_objFlushLockObject)
                    _blnFlush = value;
            }
        }
        public void FlushSync()
        {
            //Flush all output when the build finishes so we make sure we get everything.
            IsFlushPending = true;

            while (
                IsFlushPending == true 
                && 
                _objUpdateTimer.IsEnabled) 
            {
                System.Threading.Thread.Sleep(50);
                System.Windows.Forms.Application.DoEvents();
            }
        }
        public string GetChangedData(bool discard)
        {
            string ret = string.Empty;
            if (_blnHasChanged == true)
            {
                ret = LogData;

                if (discard)
                    DiscardAllData();

                _blnHasChanged = false;
            }
            return ret;
        }
        public void DiscardAllData()
        {
            LogData = string.Empty;
            GC.Collect();
        }
        public void BeginPolling()
        {
            DiscardAllData();

            _objUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            _objUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, _intPollInterval);
            _objUpdateTimer.Tick += new EventHandler(PollUpdate);
            _objUpdateTimer.Start();

            //_objBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            //_objBackgroundWorker.DoWork += PollUpdate;
            //_objBackgroundWorker.RunWorkerAsync();
            //_objBackgroundWorker.WorkerSupportsCancellation = true;
            //_objBackgroundWorker.RunWorkerCompleted += WorkerExited;
        }
        public void EndPolling()
        {
           // _objBackgroundWorker.CancelAsync();
            _objUpdateTimer.Stop();
            _objUpdateTimer = null;
        }
        private bool _blnUpdating = false;
        private Object _objUpdatingLockObject = new Object();
        private void PollUpdate(object sender, EventArgs e)
        {
            lock (_objUpdatingLockObject) 
            {
                // Avoid stacking async updates.
                if (_blnUpdating)
                    return;
                _blnUpdating = true;
            }
            CheckFlush();
            UpdateFileData();

            lock (_objUpdatingLockObject) 
            {
                _blnUpdating = false;
            }
        }
        private void CheckFlush()
        {
            if (IsFlushPending) 
            {
                UpdateFileData();// Update one final time.
                IsFlushPending = false;
            }
        }
        private void UpdateFileData()
        {
            if (!System.IO.File.Exists(_strFilePath))
                return;

            System.IO.FileInfo inf = new System.IO.FileInfo(_strFilePath);
            inf.Refresh();

            if (inf.LastWriteTime > _datLastFileUpdateTime)
                TryReadFileWithTimeout(inf, 2000);
        }
        private void TryReadFileWithTimeout(System.IO.FileInfo inf, int timeoutMillis)
        {
            int tA = System.Environment.TickCount;
            while (true)
            {
                int tB = System.Environment.TickCount;
                if ((tB - tA) > timeoutMillis)
                {
                    Console.WriteLine(" Error Could not log to file - file is probably locked.. took more than 2s.");
                    return;
                }

                try
                {
                    TryReadFile(inf);
                    return;
                }
                catch (System.IO.FileNotFoundException)
                {
                    //File not found? return.
                    return;
                }
                catch (System.IO.IOException)
                {
                    int n = 0;
                    n++;
                }
                System.Windows.Forms.Application.DoEvents();
            }
        }
        private void TryReadFile(System.IO.FileInfo inf)
        {
            string str = string.Empty;
            byte[] bytes = null;
            int numBytes = 0;

            if (_blnReload == true)
            {
                //Reload whole file.  Not recommended
                bytes = System.IO.File.ReadAllBytes(_strFilePath);
                numBytes = bytes.Length;
            }
            else
            {
                // Reload only the number of greater bytes in the file.
                System.IO.FileStream fs = System.IO.File.OpenRead(_strFilePath);

                // if we are somehow shorter than the filesize we will read t he whole thing again.
                int size;
                int offset;

                if (inf.Length < _intLastFileSize)
                {
                    // Reset - file log did not grow correctly (for some reason)
                    offset = 0;
                    size = (int)inf.Length;
                    LogData = String.Empty;//reset log data so we dont go cray
                    _intLastFileSize = (int)inf.Length;
                }
                else
                {
                    offset = _intLastFileSize;
                    size = (int)inf.Length - _intLastFileSize;
                }
                if (size < 0 || offset < 0)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new Exception("Failed to read file - offset or size is invalid. offset: " + offset + " size: " + size);
                }
                if (size != 0)
                {
                    bytes = new byte[size];
                    fs.Seek(offset, System.IO.SeekOrigin.Begin);
                    numBytes = fs.Read(bytes, 0, size);
                }
                
                fs.Close();
            }

            if (numBytes != 0)
            {
                str = System.Text.Encoding.ASCII.GetString(bytes, 0, numBytes);
                LogData += str;
                if (LogData.Length > 10000000)
                {
                    System.Diagnostics.Debugger.Break();
                }
                _intLastFileSize += (int)numBytes;
                if (numBytes >= inf.Length)
                    _datLastFileUpdateTime = inf.LastWriteTime;
                _blnHasChanged = true;
            }

        }

    }





}
