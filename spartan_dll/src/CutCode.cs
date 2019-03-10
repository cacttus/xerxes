using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spartan
{
    class CutCode
    {

        //public void CheckForDeadShellProcess()
        //{
        //    //if (ShellProcess.HasExited)
        //  //  {
        //        Globals.Logger.LogInfo("Shell process has died..restarting");
        //        //ShellProcess = null;
        //        GC.Collect();
        //        System.Threading.Thread.Sleep(200);//fuck idk this is probably a huge fuck up
        //        System.Windows.Forms.Application.DoEvents();
        //        this.CreateCompilerProcess();
        //  //  }
        //}

        //private void (int timeout = (10*60*1000))
        //{
        //    int a = System.Environment.TickCount;
        //    while (ProcessRunning == true)
        //    {
        //        // - If we are supposed to abort then quit.
        //        if (PeekNextThreadMessage() == ThreadMessageType.ThreadAbortCompilation)
        //        {
        //            ProcessNextThreadMessage();
        //            break;
        //        }


        //        int b = System.Environment.TickCount;
        //        if ((b - a) > timeout)
        //        {
        //            CompilerOutput += " [ERROR] Process exceeded max timout limit of " + timeout.ToString() + "ms";
        //            return;
        //        }
        //        System.Windows.Forms.Application.DoEvents();
        //    }
        //    //Note we set the processor state ADFTER we have sent the file.
        //}

        //ShellProcess.StandardOutput.BaseStream.Flush();
        // while (!ShellProcess.StandardOutput.EndOfStream)
        //     output += ShellProcess.StandardOutput.ReadLine();
        // ShellProcess.ProcessorAffinity = new IntPtr(_iProcessorAffinityBitMask);
        // ShellProcess.StandardInput.WriteLine(AgentCommand.CommandText);
        // WriteComplete();
        //WaitForLastExecutedCommand();
        //        private void SetThreadProcessorAffinity(int iCpuId)
        //        {

        //            // Supports up to 64 processors
        //            long cpuMask = 1L << iCpuId;

        //            // Ensure managed thread is linked to OS thread; does nothing on default host in current .Net versions
        //            System.Threading.Thread.BeginThreadAffinity();

        //#pragma warning disable 618
        //            // The call to BeginThreadAffinity guarantees stable results for GetCurrentThreadId,
        //            // so we ignore the obsolete warning
        //            int osThreadId = AppDomain.GetCurrentThreadId();
        //#pragma warning restore 618

        //            // Find the ProcessThread for this thread.
        //            System.Diagnostics.ProcessThread thread = 
        //                System.Diagnostics.Process.GetCurrentProcess()
        //                .Threads
        //                .Cast<System.Diagnostics.ProcessThread>()
        //                .Where(t => t.Id == osThreadId).Single();

        //            // Set the thread's processor affinity
        //            thread.ProcessorAffinity = new IntPtr(cpuMask);
        //        }
        //private void CompileSourceFileToObject(BroSourceFile file)
        //{
        //    LogMsg(System.IO.Path.GetFileName(file.FileBranchName));

        //    file.CompilerInputFileName = BroCompilerUtils.BranchPathToFullPathWithFileName(file.FileBranchName);
        //    file.CompilerOutputFileName = BroCompilerUtils.AgentBranchFileNameToBinaryFileNameWithPath(file.FileBranchName, file.BinOutputRootDirectory);

        //    // - CHeck the input fn
        //    if (!System.IO.File.Exists(file.CompilerInputFileName))
        //        throw new Exception("Could not find input source file.");

        //    // - Create dirs if they dont exist
        //    string dirWithoutFn = System.IO.Path.GetDirectoryName(file.CompilerOutputFileName);
        //    if (!System.IO.Directory.Exists(dirWithoutFn))
        //        System.IO.Directory.CreateDirectory(dirWithoutFn);

        //    string compilerArgs = " /c \"" + file.CompilerInputFileName + "\" /Fo\"" + file.CompilerOutputFileName + "\" ";

        //    compilerArgs += file.CompilerArgs;

        //    _strInvokedProcessName = "cl.exe";
        //    _blnProcessRunning = true;
        //    BroCompilerUtils.RunVcCompiler(ShellProcess, compilerArgs);

        //    WriteComplete();
        //}
        //private void LinkSourcesToExe(BroSourceFile file)
        //{
        //    //TODO:
        //    string linkerArgs = "/OUT:\"c:/test.exe\" \"c:/test.obj\" /MANIFEST /NXCOMPAT /DYNAMICBASE  \"kernel32.lib\" \"user32.lib\" \"gdi32.lib\" \"winspool.lib\" \"comdlg32.lib\" \"advapi32.lib\" \"shell32.lib\" \"ole32.lib\" \"oleaut32.lib\" \"uuid.lib\" \"odbc32.lib\" \"odbccp32.lib\"";
        //    _strInvokedProcessName = "link.exe";
        //    _blnProcessRunning = true;
        //    BroCompilerUtils.RunVcLinker(ShellProcess, linkerArgs);

        //    WriteComplete();
        //}
        //case PacketType.CheckFileUpdate:

        //    int dirOrFile = BroNetworkUtils.UnpackInt(ref buffer);
        //    DateTime dt = BroNetworkUtils.UnpackDateTime(ref buffer);
        //    string fn;
        //    try
        //    {
        //        fn = BroNetworkUtils.UnpackString(ref buffer);
        //    }
        //    catch (System.FormatException fe)
        //    {
        //        Console.WriteLine("Failed to read file - there was corrupt data sent. Original Data:\n" + buffer + "  \nException: " + fe);
        //        return;
        //    }
        //    int ret;
        //    if(dirOrFile==1)
        //        ret = CheckUpdateSourceFile(fn, dt);
        //    else
        //        ret = CheckUpdateDirectory(fn, dt);
        //    System.IO.DirectoryInfo di;


        //    data = 
        //        BroNetworkUtils.PacketTypeToMsgHeader(PacketType.CheckFileUpdate)
        //        + BroNetworkUtils.PackInt(ret);

        //    sc.Send(data);

        //    //Deprecated
        //    //throw new NotImplementedException();
        //    break;
        //case PacketType.FileUpdate:
        //    try
        //    {
        //        sf = BroNetworkUtils.ReadFileFromStream(ref buffer);
        //    }
        //    catch (System.FormatException fe)
        //    {
        //        Console.WriteLine("Failed to read file - there was corrupt data sent. Original Data:\n" + buffer + "  \nException: " + fe);
        //        return;
        //    }

        //    UpdateSourceFile(sf);

        //    data = BroNetworkUtils.PacketTypeToMsgHeader(PacketType.FileUpdate);
        //    sc.Send(data);
        //    break;
        //case PacketType.UpdateFileList:
        //    data = GetNeededFileUpdates(ref buffer);

        //    sc.Send(data);
        //    break;
        //private int CheckUpdateSourceFile(string fileName, DateTime dt)
        //{
        //    string filePath = BroCompilerUtils.BranchPathToFullPath(fileName);

        //    if (System.IO.File.Exists(filePath) == false)
        //        return 1;

        //    System.IO.FileInfo inf = new System.IO.FileInfo(filePath);
        //    inf.Refresh();

        //    if (inf.LastWriteTime < dt)
        //        return 1;

        //    return 0;
        //}
        //private int CheckUpdateDirectory(string dirName, DateTime dt)
        //{
        //    string dPath = BroCompilerUtils.BranchPathToFullPath(dirName);

        //    if (System.IO.Directory.Exists(dPath) == false)
        //        return 1;// Must move

        //    System.IO.DirectoryInfo inf = new System.IO.DirectoryInfo(dPath);
        //    inf.Refresh();

        //    if (inf.LastWriteTime < dt)
        //        return 1;// Must move

        //    return 0; // Dont move
        //}
        //private string GetNeededFileUpdates(ref string buffer)
        //{
        //    string ret = BroNetworkUtils.PacketTypeToMsgHeader(PacketType.UpdateFileList);
        //    //List<string> files = new List<string>();
        //    while (buffer.Length > 0)
        //    {
        //        DateTime dt = BroNetworkUtils.UnpackDateTime(ref buffer);
        //        string fileNameAndPath = BroNetworkUtils.UnpackString(ref buffer);

        //        string branchPath = BroCompilerUtils.BranchPathToFullPath(fileNameAndPath);


        //        string filename = System.IO.Path.GetFileName(fileNameAndPath);
        //        branchPath = System.IO.Path.Combine(branchPath, filename);

        //        if (fileNameAndPath.Contains("AnimationFactory"))
        //        {
        //            int n = 0;
        //        }

        //        if (System.IO.File.Exists(branchPath) == false)
        //        {
        //            ret += BroNetworkUtils.PackString(fileNameAndPath);
        //        }
        //        else
        //        {
        //            System.IO.FileInfo inf = new System.IO.FileInfo(branchPath);
        //            if (inf.LastWriteTime < dt)
        //                ret += BroNetworkUtils.PackString(fileNameAndPath);
        //        }
        //    }
        //    return ret;
        //}

        //private void UpdateSourceFile(BroSourceFile bfile)
        //{
        //    string filePath = BroCompilerUtils.BranchPathToFullPath(bfile);
        //    string fileName = System.IO.Path.GetFileName(bfile.FileBranchName);
        //    Console.WriteLine("GOT: " + fileName);

        //    //Create dir if not exist.
        //    try
        //    {
        //        if (!System.IO.Directory.Exists(filePath))
        //            System.IO.Directory.CreateDirectory(filePath);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Failed to create directory: " + filePath + "....Exception=" + e.ToString());
        //    }

        //    // add file name
        //    filePath = System.IO.Path.Combine(filePath, fileName);

        //    //save file
        //    try
        //    {
        //        System.IO.File.WriteAllBytes(filePath, System.Text.Encoding.ASCII.GetBytes(bfile.FileData));
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Failed to save fuckinf file: " + e.ToString());
        //    }
        //}
        //private void SendFileToProcessor(BroSourceFile nf, int reservationId)
        //{
        //    System.Diagnostics.Debug.Assert(reservationId != -1);

        //    for (int n = 0; n < CompilerThreads.Count(); n++)
        //    {
        //        //Note we mutex here.
        //        if (CompilerThreads[n].ReservationId == reservationId)
        //        {
        //            if (CompilerThreads[n].ProcessorState != ProcessorState.Reserved)
        //            {
        //                LogError("Tried to compile file " + nf.FileBranchName + " but the processor was not reserved state "
        //                    + CompilerThreads[n].ProcessorState.ToString() + " relying on server to resend file.");
        //                break;
        //            }
        //            //System.Diagnostics.Debug.Assert(CompilerThreads[n].ProcessorState == ProcessorState.Reserved);

        //            CompilerThreads[n].ProcessorState = ProcessorState.Working;
        //            BroCommand cmd = new BroCommand();
        //            cmd.ThreadMessageType = ThreadMessageType.ThreadCompileFile;

        //            cmd.SourceFile = nf;

        //            CompilerThreads[n].SendMessageToThread(cmd);
        //            break;
        //        }
        //    }

        //}
        //BroSourceFile CreateBroSource(BroProject proj, string filename)
        //{
        //    BroSourceFile bs = new BroSourceFile();
        //    bs.FileBranchName = filename;
        //    bs.CompilerArgs = proj.GetCompilerFlags();
        //    bs.ProjectDirectory = BroCompilerUtils.FullSourcePathToBranchPath(proj.SolutionFileDirectory);

        //    bs.BinOutputRootDirectory = BroCompilerUtils.GetBinPathFromBuildConfiguration(_objCompilerManager.BuildConfiguration);

        //    // THis makes sure it is a dir.
        //    if (bs.ProjectDirectory[bs.ProjectDirectory.Length - 1] != '\\' &&
        //        bs.ProjectDirectory[bs.ProjectDirectory.Length - 1] != '/')
        //        bs.ProjectDirectory += "\\";

        //    return bs;
        //}
        //private void GatherSources()
        //{
        //    foreach (BroProject proj in _objCompilerManager.Projects)
        //    {
        //        if (proj.ProjectOutputType == BroProjectOutputType.Exe)
        //            continue;

        //        foreach (string fileName in proj.ObjectFiles)
        //        {

        //            _objFileQueue.Enqueue(CreateBroSource(proj,fileName));
        //            //DistributeFileAndPollAgents(fileName, proj.GetCompilerFlags());

        //            //TODO: add linker coimmands here
        //        }
        //    }

        //    //Exe Last
        //    foreach (BroProject proj in _objCompilerManager.Projects)
        //    {
        //        if (proj.ProjectOutputType == BroProjectOutputType.Exe)
        //        {
        //            foreach (string fileName in proj.ObjectFiles)
        //            {
        //                _objFileQueue.Enqueue(CreateBroSource(proj, fileName));
        //            }
        //            //throw new NotImplementedException();

        //            //TODO: add linker coimmands here
        //        }
        //    }
        //}
        //public void GetLatestForAgent(CoordinatorAgent ag)
        //{
        //    try
        //    {
        //        int nSent = 0;
        //        string lastDir = System.IO.Directory.GetCurrentDirectory();
        //        System.IO.Directory.SetCurrentDirectory(BroCompilerUtils.ServerBranchDirectory);
        //        SendAllSourceFilesToAgent(ag, ref nSent);
        //        System.IO.Directory.SetCurrentDirectory(lastDir);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError("Error Sending Source files: " + ex.ToString());
        //        DropAgent(ag);
        //    }
        //}
        //public void SendAllSourceFilesToAgent(CoordinatorAgent ag, ref int nSent)
        //{
        //    bool useFileList = false;

        //    try
        //    {
        //        Console.WriteLine("  Getting Latest for " + ag.ComputerName);

        //        if (useFileList)
        //        {
        //            string allFiles = String.Empty;

        //            allFiles += BroNetworkUtils.PacketTypeToMsgHeader(PacketType.UpdateFileList);

        //            LogInfo("Creating massive Data Chunk.");
        //            GatherAllSourceFiles(ref allFiles);

        //            BroNetworkUtils.SendFileUpdateToAgent(ag, allFiles);
        //        }
        //        else
        //        {
        //            RecursiveSendFilesToClient(ag);
        //        }
        //    }
        //    catch (System.Net.Sockets.SocketException se)
        //    {
        //        LogError("[Sockets] Network error for agent " + ag.ComputerName + " dropping agent:ex= " + se.ToString());
        //        DropAgent(ag);
        //    }
        //}
        //public void GatherAllSourceFiles(ref string allFiles)
        //{
        //    // **this is only called if usefIleLIst is true
        //    string curDir = System.IO.Directory.GetCurrentDirectory();
        //    string[] files = System.IO.Directory.GetFiles(curDir);

        //    foreach (string cf in files)
        //    {
        //        string ext = System.IO.Path.GetExtension(cf);
        //        string fileName = System.IO.Path.GetFileName(cf);

        //        if ((ext.Length > 0)
        //            && GetLatestFileExtensions.Contains(ext.ToLower())
        //            && fileName.Length > 0)
        //        {
        //            System.IO.FileInfo inf = new System.IO.FileInfo(cf);

        //            allFiles += BroNetworkUtils.PackDateTime(inf.LastWriteTime);
        //            string bf = BroCompilerUtils.GetBranchLocalFileName(cf);
        //            allFiles += BroNetworkUtils.PackString(bf);
        //        }
        //    }

        //    // - Keep Traversing
        //    string[] dirs = System.IO.Directory.GetDirectories(curDir);
        //    foreach (string d in dirs)
        //    {
        //        System.IO.Directory.SetCurrentDirectory(d);
        //        GatherAllSourceFiles(ref allFiles);
        //        System.IO.Directory.SetCurrentDirectory(curDir);
        //    }

        //}
        //public void RecursiveSendFilesToClient(CoordinatorAgent ag)
        //{
        //    string curDir = System.IO.Directory.GetCurrentDirectory();

        //    // * First check directory has changed
        //    if (BroNetworkUtils.QueryAgentUpdateDirectory(ag, curDir))
        //    {

        //        string[] files = System.IO.Directory.GetFiles(curDir);
        //        foreach (string cf in files)
        //        {
        //            string ext = System.IO.Path.GetExtension(cf);
        //            string fileName = System.IO.Path.GetFileName(cf);

        //            if ((ext.Length > 0)
        //                && GetLatestFileExtensions.Contains(ext.ToLower())
        //                && fileName.Length > 0)
        //            {

        //                //allFiles += BroNetworkUtils.PackDateTime(inf.LastWriteTime);
        //                //string bf = BroCompilerUtils.GetBranchLocalFileName(cf);
        //                BroNetworkUtils.QueryAgentUpdateFile(ag, cf);
        //                BroNetworkUtils.SendSingleFileUpdateToAgent(ag, cf);
        //            }
        //        }
        //    }

        //    // - Keep Traversing
        //    string[] dirs = System.IO.Directory.GetDirectories(curDir);
        //    foreach (string d in dirs)
        //    {
        //        System.IO.Directory.SetCurrentDirectory(d);
        //        RecursiveSendFilesToClient(ag);
        //        System.IO.Directory.SetCurrentDirectory(curDir);
        //    }

        //}

        //const string FooterStr = "80085";

        //public static void SendData(System.Net.Sockets.Socket s, string str)
        //{
        //    //*Add Footer
        //    str += FooterStr;
        //    byte[] b = System.Text.Encoding.ASCII.GetBytes(str);

        //    IAsyncResult res = s.BeginSend(b, 0, b.Length,System.Net.Sockets.SocketFlags.None, SendCallback, s);
        //    bool success = res.AsyncWaitHandle.WaitOne(20000, true);
        //    System.Diagnostics.Debug.Assert(success == true);
        //}
        //private static void SendCallback(IAsyncResult ar)
        //{
        //    System.Net.Sockets.Socket s = (System.Net.Sockets.Socket)ar.AsyncState;
        //    System.Net.Sockets.SocketError errcode;

        //    s.EndSend(ar, out errcode);

        //    if (errcode != System.Net.Sockets.SocketError.Success)
        //        Console.WriteLine(" SEND GOT ERROR: " + errcode.ToString());

        //    //??
        //    int n = 0;
        //    n++;
        //}
        //private static bool HasFooter(string str)
        //{
        //    if (str == null || str.Length < FooterStr.Length)
        //        return false;

        //    string st = str.Substring(str.Length - FooterStr.Length, FooterStr.Length);
        //    return st == FooterStr;
        //}
        // public static string RecvData(System.Net.Sockets.Socket s, bool block = false, int iTimeout = 100000)
        // {
        //    // return RecvDataSync(s, block, iTimeout);
        //     return RecvDataSync2(s, block, iTimeout);
        //     //return RecvDataAsync(s, block, iTimeout);
        // }

        // static Object _lockObject = new Object();
        //// public static byte[] staticBuffer = new byte[10000000];
        // public static string RecvDataAsync(System.Net.Sockets.Socket s, bool block = false, int iTimeout = 100000)
        // {
        //     //byte[] buf = new byte[2048];
        //     StateObj st = new StateObj();

        //     System.Net.Sockets.SocketError errCode;

        //     IAsyncResult res = s.BeginReceive(st.buf, 0, st.buf.Length, System.Net.Sockets.SocketFlags.None, out errCode, RecvCallback, st);
        //     bool success = res.AsyncWaitHandle.WaitOne(20000, true);
        //     System.Diagnostics.Debug.Assert(success == true);

        //     string str;
        //     //lock (_lockObject)
        //     {
        //         str = st.appstr;
        //     }

        //     if (!HasFooter(str))
        //         throw new Exception(" rcv invlaid packet 234");

        //     return "";
        // }
        // private static void RecvCallback(IAsyncResult ar)
        // {
        //    // lock (_lockObject)
        //     {
        //         StateObj st = (StateObj)ar.AsyncState;
        //         System.Net.Sockets.Socket s = st.sock;
        //         System.Net.Sockets.SocketError errcode;

        //         int bytesGot = s.EndReceive(ar, out errcode);

        //         if (errcode != System.Net.Sockets.SocketError.Success)
        //             Console.WriteLine("RCV GOT ERROR: " + errcode.ToString());

        //         st.appstr += System.Text.Encoding.ASCII.GetString(st.buf, 0, bytesGot);
        //     }

        //     //We need to call again pry

        //     //??
        // }


        //public static string RecvDataSync(System.Net.Sockets.Socket s, bool block = false, int iTimeout = 100000)
        //{
        //    byte[] buf = new byte[2048];
        //    int rcvLen = 0;
        //    int rcvTotal = 0;
        //    string str = "";

        //    if (!s.Connected)
        //        throw new Exception("Socket wasn't connected for read operation.");

        //    //This is causing errors with async sockets.
        //    //s.Blocking = false;

        //    int da = System.Environment.TickCount;
        //    while (true)
        //    {
        //        System.Windows.Forms.Application.DoEvents();
        //        int db = System.Environment.TickCount;

        //        if (block == false)
        //            if (db - da > iTimeout)
        //                throw new Exception(" rcv data from client tiemd out");

        //        System.Net.Sockets.SocketError errorCode;
        //        try
        //        {
        //            if (s.Available > 0)
        //            {
        //                // Main  Non-Blocking Rcv
        //                rcvLen = s.Receive(buf, 0, 2048, System.Net.Sockets.SocketFlags.None, out errorCode);

        //                if (errorCode != System.Net.Sockets.SocketError.Success)
        //                    Console.WriteLine("RCV ERROR: " + errorCode.ToString());

        //                if (rcvLen == 0 && HasFooter(str))
        //                    break;

        //                rcvTotal += rcvLen;
        //                str += System.Text.Encoding.ASCII.GetString(buf, 0, rcvLen);
        //            }
        //            else if (str.Length > 0)//( (s.Available && (str.Length > 0) && ) || (s.Available==0))
        //            {
        //                if (HasFooter(str))
        //                    break; //we are the end of packet.
        //            }
        //            else if (block==false && s.Available == 0)
        //                break;
        //        }
        //        catch (System.Net.Sockets.SocketException sx)
        //        {
        //            if (block == false && sx.ErrorCode == 10035)
        //            {
        //                ; // Swallow - this si 
        //            }
        //            else
        //                throw sx;
        //        }

        //    }

        //    // *Footer Check
        //    if (rcvTotal > 0)
        //    {
        //        if (!HasFooter(str))
        //            throw new Exception("invlaid packet 1");
        //        if (str.Length == 0)
        //            throw new Exception("invlaid packet 2");

        //        if (s.Available > 0)
        //            throw new Exception("Socket still had data but we abandoned it.");
        //        if (str.Length != rcvTotal)
        //            throw new Exception("Invalid recv count.");

        //        str = str.Substring(0, str.Length - FooterStr.Length);
        //    }

        //    if (String.IsNullOrEmpty(str))
        //    {
        //        int n = 0;
        //    }

        //    return str;
        //}
        //public static void SendFileUpdateToAgent(CoordinatorAgent agent, string allFiles)
        //{
        //    int rcvTimeout = 60000;

        //    string strBuf;
        //    // ***1 Send Update block

        //    Console.WriteLine("Sending to Client..");
        //    agent.Send(allFiles);

        //    // ***2 Read 
        //    Console.WriteLine("Waiting for files..");
        //    strBuf = agent.Recv(rcvTimeout);
        //    PacketType pt = GetPacketType(ref strBuf);

        //    Console.WriteLine("Sending individual files..");
        //    while (strBuf.Length > 0)
        //    {
        //        string fileLoc = BroNetworkUtils.UnpackString(ref strBuf);

        //        fileLoc = BroCompilerUtils.BranchPathToServerPath(fileLoc);
        //        SendSingleFileUpdateToAgent(agent, fileLoc);
        //    }
        //}
        //public static void SendSingleFileUpdateToAgent(CoordinatorAgent agent, string fileLoc)
        //{

        //    // ***3 Send File
        //    // Send file if out of date.
        //    string fileData;
        //    try
        //    {
        //        fileData = System.Text.Encoding.ASCII.GetString(System.IO.File.ReadAllBytes(fileLoc));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(" ERROR: could not open file.  The file may be Readonly: " + ex.ToString());
        //        return;
        //    }

        //    string fileSubDir = BroCompilerUtils.GetBranchLocalFileName(fileLoc);
        //    string dataBuf = BroNetworkUtils.PacketTypeToMsgHeader(PacketType.FileUpdate)
        //        + BroNetworkUtils.PackString(fileSubDir)
        //        + BroNetworkUtils.PackInt(fileData.Length)
        //        + BroNetworkUtils.PackString(fileData)
        //        ;
        //    agent.Send(dataBuf);

        //    string strBuf = agent.Recv(RcvTimeout);

        //    if (BroNetworkUtils.GetPacketType(ref strBuf) != PacketType.FileUpdate)
        //        throw new Exception("Failed to get correct packet type when updating file.");
        //}
        //public static void SendSourceFileToAgent(CoordinatorAgent agent, BroSourceFile bf)
        //{
        //    string fileName     = bf.FileBranchName;
        //    string compileFlags = bf.CompilerArgs;
        //    string projectDir   = bf.ProjectDirectory;
        //    string binDir       = bf.BinOutputRootDirectory;

        //    System.Diagnostics.Debug.Assert(agent.LastReservationId != -1);
        //    System.Diagnostics.Debug.Assert(agent.LastReservationId != 0);

        //    WipFile wf = new WipFile(System.Environment.TickCount, agent.LastReservationId, bf);
        //    agent.WipFiles.Add(wf);

        //    string dataBuf =
        //          PacketTypeToMsgHeader(PacketType.OkToCompileFile)
        //        + PackInt(agent.LastReservationId) //we need this to confirm the correct file that was sent.
        //        + PackString(fileName)
        //        + PackString(compileFlags)
        //        + PackString(projectDir)
        //        + PackString(binDir)
        //        ;

        //    agent.Send(dataBuf);
        //}

        //public static string PackSourceFile(BroSourceFile bsf, string compileOutput)
        //{
        //    // compiled object file
        //    // [header[8]] [file name[8][x]] [success/failure[8]] [compiler output[8][x]] [obj file data[8][x]] 
        //    string data = String.Empty;
        //    bool blnSuccess;

        //    blnSuccess = System.IO.File.Exists(bsf.CompilerOutputFileName);

        //    data += BroNetworkUtils.PacketTypeToMsgHeader(PacketType.CompileRequestComplete);
        //    data += BroNetworkUtils.PackInt(bsf.ClientReservationId);
        //    data += BroNetworkUtils.PackString(bsf.CompilerInputFileName);

        //    string outName = BroCompilerUtils.AgentFullBinPathToBinOutputPath(bsf.CompilerOutputFileName);

        //    if (outName.Substring(0, 3) == "bin")
        //        outName = outName.Substring(3);
        //    BroCompilerUtils.MakeDirectoryPretty(ref outName);

        //    data += BroNetworkUtils.PackString(outName);

        //    if (blnSuccess == true)
        //        data += BroNetworkUtils.PackInt(1);
        //    else
        //        data += BroNetworkUtils.PackInt(0);

        //    data += BroNetworkUtils.PackString(compileOutput);

        //    if (blnSuccess == false)
        //        return data;

        //    // pack .obj file.
        //    byte[] objData = System.IO.File.ReadAllBytes(bsf.CompilerOutputFileName);

        //    data += PackInt(objData.Length);
        //    data += PackBytes(objData);

        //    //File must be object file or explode
        //    System.Diagnostics.Debug.Assert(
        //        System.IO.Path.GetExtension(bsf.CompilerOutputFileName).ToLower() == ".obj" ||
        //        System.IO.Path.GetExtension(bsf.CompilerOutputFileName).ToLower() == ".exe" ||
        //        System.IO.Path.GetExtension(bsf.CompilerOutputFileName).ToLower() == ".lib"
        //        );

        //    //Then delete... pray to glob
        //    Console.WriteLine("Deleting " + bsf.CompilerOutputFileName);

        //    //We might need to make sure the file is not locked.
        //    System.IO.File.Delete(bsf.CompilerOutputFileName);

        //    return data;
        //}

        //public static bool QueryAgentUpdateDirectory(CoordinatorAgent ag, string dirLoc)
        //{
        //    // IF true then we must update.
        //    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dirLoc);
        //    di.Refresh();

        //    string dir = BroCompilerUtils.GetBranchLocalFileName(dirLoc);
        //    string dataBuf = 
        //        BroNetworkUtils.PacketTypeToMsgHeader(PacketType.CheckFileUpdate)
        //        + BroNetworkUtils.PackInt(0)// ?? 
        //        + BroNetworkUtils.PackDateTime(di.LastWriteTime)
        //        + BroNetworkUtils.PackString(dir)
        //        ;
        //    ag.Send(dataBuf);

        //    string strBuf = ag.Recv(RcvTimeout);

        //    PacketType pt = GetPacketType(ref strBuf);
        //    if (pt != PacketType.CheckFileUpdate)
        //        throw new Exception("Failed to get correct packet type when updating file.");

        //    int ret = UnpackInt(ref strBuf);

        //    return (ret == 1);

        //}
        //public static bool QueryAgentUpdateFile(CoordinatorAgent ag, string dirLoc)
        //{
        //    // IF true then we must update.
        //    System.IO.FileInfo inf = new System.IO.FileInfo(dirLoc);
        //    inf.Refresh();

        //    string dir = BroCompilerUtils.GetBranchLocalFileName(dirLoc);
        //    string dataBuf = BroNetworkUtils.PacketTypeToMsgHeader(PacketType.CheckFileUpdate)
        //        + BroNetworkUtils.PackInt(1)
        //        + BroNetworkUtils.PackDateTime(inf.LastWriteTime)
        //        + BroNetworkUtils.PackString(dir)
        //        ;

        //    ag.Send(dataBuf);

        //    string strBuf = ag.Recv(RcvTimeout);

        //    PacketType pt = GetPacketType(ref strBuf);
        //    if (pt != PacketType.CheckFileUpdate)
        //        throw new Exception("Failed to get correct packet type when updating file.");

        //    int ret = UnpackInt(ref strBuf);

        //    return (ret == 1);

        //}
        //private void SendCompiledFileToServer(BroCompilerThread th, BroCommand cmd)
        //{
        //    string source = BroNetworkUtils.PackSourceFile(
        //        cmd.SourceFile,
        //        th.CompilerOutput
        //        );

        //    //Add to all files
        //    CompiledFiles.Add(cmd.SourceFile);

        //    GetPrimaryServerConnection().Send(source);

        //    // * set process
        //    FreeProcessor(th);
        //}
        //public static BroSourceFile ReadBuildableSourceFileFromStream(ref string buffer)
        //{
        //    BroSourceFile f = new BroSourceFile();

        //    f.FileBranchName = BroNetworkUtils.ReadStringFromStream(ref buffer).Trim();
        //    f.CompilerArgs = BroNetworkUtils.ReadStringFromStream(ref buffer).Trim();
        //    //we dont send data duh
        //    //f.FileData = "";// BroNetworkUtils.ReadStringFromStream(ref buffer).Trim();
        //    f.ProjectDirectory = BroNetworkUtils.ReadStringFromStream(ref buffer).Trim();
        //    f.BinOutputRootDirectory = BroNetworkUtils.ReadStringFromStream(ref buffer).Trim();

        //    f.ProjectDirectory = BroCompilerUtils.BranchPathToFullPath(f.ProjectDirectory);

        //    return f;
        //}
        //public static List<BroSourceFile> ParseBuildableFileList(ref string data)
        //{
        //    List<BroSourceFile> list = new List<BroSourceFile>();
        //    while (data.Length > 0)
        //    {
        //        BroSourceFile f = ReadBuildableSourceFileFromStream(ref data);
        //        list.Add(f);
        //    }
        //    return list;
        //}

        //public const string SpartanBuildInstallPath = "C:\\SpartanBuild\\";
        // Dependency cache where we copy files in order to determine whether they have been modified
        //public const string ServerDepCacheDirectory = SpartanBuildInstallPath + "Server\\DependencyCache";
        // object build cache for .obj and .lib files.
        //public const string ServerBuildCacheDirectory = SpartanBuildInstallPath + "Server\\BuildCache";
        //public const string ServerLocalBinDirectory = SpartanBuildInstallPath + "Server\\Bin";

        //public const string ClientLocalBranchDirectory = SpartanBuildInstallPath + "Client\\BranchCache";
        //public const string ClientLocalBinDirectory = SpartanBuildInstallPath + "Client\\Bin";

        //c:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
        //*** MUST RUN THIS WHEN STARTING

        //  public const string SourcePath = "C:/p4/derek.page/C++/borealis/src";
        //C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\bin
        //public const String StrCompilerName = "cl.exe";
        //public const String StrLinkerName = "ml.exe";
        //public const String ObjDirectoryName = ".\\obj";

        // public const string EngineDirectory = "borealis\\"; // man idk why

        //"Solution" i.e. directories.
        //public const string ServerSourceDirectory_Bro = "borealis\\src\\";
        //public const string ServerSourceDirectory_Dmc = "dmc\\";

        //public const string BinPathDebug = "bin\\debug";
        //public const string BinPathRelease = "bin\\release";

        //public static void RunVcCompiler(System.Diagnostics.Process p, string exeArgs)
        //{
        //    string strExeName = "cl.exe";
        //    string exePath = System.IO.Path.Combine(BroCompilerUtils.VcBinPath, strExeName);

        //    p.StandardInput.WriteLine("\"" + exePath + "\"" + " " + exeArgs);
        //}
        //public static void RunVcLinker(System.Diagnostics.Process p, string exeArgs)
        //{
        //    string strExeName = "link.exe";
        //    string exePath = System.IO.Path.Combine(BroCompilerUtils.VcBinPath, strExeName);

        //    p.StandardInput.WriteLine("\"" + exePath + "\"" + " " + exeArgs);

        //}
        //public static string GetRootedAdditionalIncludePath(string path)
        //{
        //    string pathName = System.IO.Path.Combine(BroCompilerUtils.ServerBranchDirectory, BroCompilerUtils.EngineDirectory);
        //    pathName = System.IO.Path.Combine(pathName, path);
        //    return pathName;
        //}
        //public static string BranchPathToFullPath(BroSourceFile bf)
        //{
        //    return BranchPathToFullPath(bf.FileBranchName);
        //}
        //public static string BranchPathToFullPathWithFileName(BroSourceFile bf)
        //{
        //    return BranchPathToFullPath(bf);
        //}
        //public static string BranchPathToFullPathWithFileName(string filename)
        //{
        //    string bp = BranchPathToFullPath(filename);
        //    string fn = System.IO.Path.GetFileName(filename);
        //    string ret = System.IO.Path.Combine(bp, fn);
        //    return ret;
        //}
        //public static void MakeDirectoryPretty(ref string st)
        //{
        //    if (st.Length > 0) // WE CAN HAVE EMPTY DIRS
        //    {
        //        if (st[0] == '\\' || st[0] == '/')
        //            st = st.Substring(1);
        //    }
        //    else
        //    {
        //        st = "\\";  // we need this when we send files that truncate the root.
        //    }

        //}
        //public static string BranchPathToFullPath(string filePath)
        //{
        //    if (!(filePath == "\\" || filePath == "/"))
        //    {
        //        filePath = System.IO.Path.GetDirectoryName(filePath);
        //        MakeDirectoryPretty(ref filePath);
        //        filePath = System.IO.Path.Combine(BroCompilerUtils.ClientLocalBranchDirectory, filePath);
        //    }
        //    else
        //        filePath = BroCompilerUtils.ClientLocalBranchDirectory;

        //    return filePath;
        //}
        //public static string BranchPathToServerPath(string filePath)
        //{
        //    string filePath2 = System.IO.Path.GetDirectoryName(filePath);
        //    string fileName = System.IO.Path.GetFileName(filePath);

        //    MakeDirectoryPretty(ref filePath2);
        //    MakeDirectoryPretty(ref fileName);

        //    filePath = System.IO.Path.Combine(BroCompilerUtils.ServerBranchDirectory, filePath2);
        //    filePath = System.IO.Path.Combine(filePath, fileName);

        //    return filePath;
        //}
        //public static string FullSourcePathToBranchPath(string str)
        //{
        //    return GetBranchLocalFileName(str);
        //}
        //public static string GetBranchLocalFileName(string str)
        //{
        //    if (str.ToLower().IndexOf("c++") < 0)
        //        throw new Exception("Invalid file directory name:'" + str + "'");

        //    string ret = str.Substring(str.ToLower().IndexOf("c++") + 3);

        //    MakeDirectoryPretty(ref ret);

        //    return ret;
        //}
        //public static string AgentFullBinPathToBinOutputPath(string str)
        //{
        //    string fn = System.IO.Path.GetFileName(str);
        //    string pn = System.IO.Path.GetDirectoryName(str);

        //    pn = pn.ToLower();

        //    int a = pn.IndexOf(BinPathDebug);
        //    int b = pn.IndexOf(BinPathRelease);

        //    if(a!=-1)
        //        pn = pn.Substring(a);
        //    else if(b!=-1)
        //        pn = pn.Substring(b);
        //    else
        //        throw new Exception("Failed to parse bin output path.");

        //    MakeDirectoryPretty(ref pn);

        //    // - Add file back so we don't tolower the filename.
        //    pn = System.IO.Path.Combine(pn, fn);

        //    return pn;
        //}
        //public static string AgentBranchFileNameToBinaryFileNameWithPath(string branchLocation, string binOutputRootDirectory)
        //{
        //    return BranchFileNameToBinaryFileNameWithPath(BroCompilerUtils.ClientLocalBinDirectory, branchLocation, binOutputRootDirectory);
        //}
        //public static string BranchFileNameToBinaryFileNameWithPath(string clientOrServerRootPath, string branchLocation, string binOutputRootDirectory)
        //{
        //    string objFileName = System.IO.Path.GetFileNameWithoutExtension(branchLocation) + ".obj";

        //    if (branchLocation[0] == '\\' || branchLocation[0] == '/')
        //        branchLocation = branchLocation.Substring(1);

        //    string dirName = System.IO.Path.GetDirectoryName(branchLocation);

        //    MakeDirectoryPretty(ref binOutputRootDirectory);

        //    string ret = System.IO.Path.Combine(clientOrServerRootPath, binOutputRootDirectory);
        //    ret = System.IO.Path.Combine(ret, dirName);
        //    ret = System.IO.Path.Combine(ret, objFileName);

        //    MakeDirectoryPretty(ref ret);

        //    return ret;
        //}
        //public static string ServerGetLocalBinFileNameWithPath(string branchLocation)
        //{
        //    string st = System.IO.Path.Combine(BroCompilerUtils.ServerLocalBinDirectory, branchLocation);
        //    return st;
        //}
      //  public BuildConfiguration BuildConfiguration { get; set; }
      ////  public List<BroProject> Projects = new List<BroProject>();
      //  public int NumSourceFiles { get; set; }

      //  private HashSet<string> _objCachedHasFiles;
      //  private HashSet<string> _objCachedHasFilesNot;

       // public BroCompilerManager()
     //   {
            //_objCachedHasFiles = new HashSet<string>();
            //_objCachedHasFilesNot = new HashSet<string>();

            //BuildConfiguration = BuildConfiguration.Debug;
            //NumSourceFiles = 0;

            //string projectFileLocation_Bro = "C:\\p4\\derek.page\\c++\\borealis\\vc\\";
            //string projectFileLocation_Dmc = "C:\\p4\\derek.page\\c++\\borealis\\vc\\";

            //string solutionFileLocation_Bro = "C:\\p4\\derek.page\\c++\\borealis\\";
            //string solutionFileLocation_Dmc = "C:\\p4\\derek.page\\c++\\borealis\\";

            ////fuck some tree shit well just build them in order.
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"acct"    ) ,"acct_d"      ,"acct"      ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"ai"      ) ,"ai_d"        ,"ai"        ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"audio"   ) ,"audio_d"     ,"audio"     ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"base"    ) ,"base_d"      ,"base"      ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"dev"     ) ,"dev_d"       ,"dev"       ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"display" ) ,"display_d"   ,"display"   ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"event"   ) ,"event_d"     ,"event"     ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"game"    ) ,"game_d"      ,"game"      ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"gpu"     ) ,"gpu_d"       ,"gpu"       ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"hardware") ,"hardware_d"  ,"hardware"  ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"img"     ) ,"img_d"       ,"img"       ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"input"   ) ,"input_d"     ,"input"     ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"library" ) ,"library_d"   ,"library"   ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"material") ,"material_d"  ,"material"  ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"math"    ) ,"math_d"      ,"math"      ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"mem"     ) ,"mem_d"       ,"mem"       ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"menu"    ) ,"menu_d"      ,"menu"      ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"model"   ) ,"model_d"     ,"model"     ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"net"     ) ,"net_d"       ,"net"       ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"physics" ) ,"physics_d"   ,"physics"   ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"repos"   ) ,"repos_d"     ,"repos"     ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"scene"   ) ,"scene_d"     ,"scene"     ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);
            //AddProject(System.IO.Path.Combine(BroCompilerUtils.ServerSourceDirectory_Bro,"topo"    ) ,"topo_d"      ,"topo"      ,BroProjectOutputType.Lib, projectFileLocation_Bro, solutionFileLocation_Bro);

            ////Add final DMC project
            //BroProject bp = AddProject(BroCompilerUtils.ServerSourceDirectory_Dmc, "dmc", "dmc", BroProjectOutputType.Exe, projectFileLocation_Dmc, solutionFileLocation_Dmc);

            //// Add all dependencies to the dmc project
            //bp.ProjectDependencies= Projects;

            //BuildAdditionalIncludeDirectories();
            //BuildAdditionalLibraryDirectories();
            //BuildAdditionalDependencies();

            //ValidateDirectoriesExist();
       // }
        //private void ValidateDirectoriesExist()
        //{
        //    try
        //    {
        //        foreach (BroProject p in Projects)
        //        {
        //            foreach (string additionalInclude in p.AdditionalIncludeDirectories)
        //            {
        //                string pathName = BroCompilerUtils.GetRootedAdditionalIncludePath(additionalInclude);
        //                System.Diagnostics.Debug.Assert(System.IO.Directory.Exists(pathName));
        //            }
        //            foreach (string additionalInclude in p.AdditionalLibraryDirectories)
        //            {
        //                string pathName = BroCompilerUtils.GetRootedAdditionalIncludePath(additionalInclude);
        //                System.Diagnostics.Debug.Assert(System.IO.Directory.Exists(pathName));
        //            }
        //        }
        //    }
        //    catch(System.IO.DirectoryNotFoundException dx)
        //    {
        //        Console.WriteLine("FAILED TO FIND ADDITIONAL INCLUDE/LIBRARY DIRECTORY " + dx + " app will now exit..");
        //        throw dx;
        //    }
        //}
        //private void BuildAdditionalIncludeDirectories()
        //{
        //   // Copied from VS
        //    string strAInc = "..\\api\\OpenGL\\INCLUDE;..\\Borealis\\src;..\\api\\bullet-2.82-r2704\\src;..\\api\\OpenCL\\inc;..\\api\\dirent\\inc;..\\api\\directx_jun_2010\\Include;..\\api\\vorbis\\inc;..\\api\\SDL-1.2.15\\include";
        //    string[] inc = strAInc.Split(';');

        //    foreach(BroProject p in Projects)
        //    {
        //        p.AdditionalIncludeDirectories = new List<string>();
        //        p.AdditionalIncludeDirectories.AddRange(inc);

        //       //If using mysql..
        //        //p.AdditionalIncludeDirectories.Add("..\\..\\api\\mysql-5.1.31-win32\\include");
        //    }
        //}
        //private void BuildAdditionalLibraryDirectories()
        //{
        //    // Copied from VS
        //    string strALib = "..\\api\\zlib-1.2.3.win32\\lib;..\\api\\SDL-1.2.15\\lib\\x86;..\\api\\OpenGL\\LIB;..\\Borealis\\lib\\win32;..\\api\\directx_jun_2010\\Lib\\x86;..\\api\\bullet-2.82-r2704\\lib;..\\api\\vorbis\\LIB;..\\api\\OpenCL\\lib;..\\api\\vorbis\\lib";
        //    string[] inc = strALib.Split(';');
        //    foreach(BroProject p in Projects)
        //    {
        //        p.AdditionalLibraryDirectories = new List<string>();
        //        if(p.ProjectOutputType==BroProjectOutputType.Exe)
        //            p.AdditionalLibraryDirectories.AddRange(inc);
        //    }

        //}
        //private void BuildAdditionalDependencies()
        //{
        //    // Copied from VS
        //    string strADep = "ai_d.lib;audio_d.lib;base_d.lib;dev_d.lib;display_d.lib;event_d.lib;game_d.lib;gpu_d.lib;hardware_d.lib;img_d.lib;input_d.lib;library_d.lib;material_d.lib;mathlib_d.lib;menu_d.lib;model_d.lib;mm_memory_d.lib;net_d.lib;physics_d.lib;repos_d.lib;scene_d.lib;topo_d.lib;glew32sd.lib;zlib.lib;libogg_static_d.lib;libvorbis_static_d.lib;libvorbisfile_static_d.lib;dinput8.lib;DxErr.lib;dxgi.lib;dxguid.lib;";
        //    string[] inc = strADep.Split(';');
        //    foreach (BroProject p in Projects)
        //    {
        //        p.AdditionalDependencies = new List<string>();
        //        if (p.ProjectOutputType == BroProjectOutputType.Exe)
        //            p.AdditionalDependencies.AddRange(inc);
        //    }
        //}

        //private BroProject AddProject(  string strSourceFolderName,
        //                                string strOutputNameDebug,
        //                                string strOutputNameRelease,
        //                                BroProjectOutputType eOutputType,
        //                                string strProjectFileLocation,
        //                                string strSolutionFileLocation
        //    )
        //{
        //    BroProject bp = new BroProject(this);

        //    bp.ProjectSourceDirectory = strSourceFolderName;
        //    bp.OutputNameDebug = strOutputNameDebug;
        //    bp.OutputNameRelease = strOutputNameRelease;
        //    bp.ProjectOutputType = eOutputType;
        //    bp.ProjectFileDirectory = strProjectFileLocation;
        //    bp.SolutionFileDirectory = strSolutionFileLocation;
        //    Projects.Add(bp);

        //    return bp;
        //}

        //public void GatherProjectFiles()
        //{
        //    NumSourceFiles = 0;

        //    foreach (BroProject proj in Projects)
        //    {
        //        if (proj.ProjectOutputType == BroProjectOutputType.Exe)
        //            continue;

        //        proj.GatherAllClientObjectFileNames();
        //        NumSourceFiles += proj.ObjectFiles.Count;
        //    }

        //    //Exe Last
        //    foreach (BroProject proj in Projects)
        //    {
        //        if (proj.ProjectOutputType == BroProjectOutputType.Exe)
        //        {
        //            proj.GatherAllClientObjectFileNames();
        //            NumSourceFiles += proj.ObjectFiles.Count;
        //            break;
        //        }
        //    }
        //}
 
        //public bool SolutionContainsFile(string branchLocation, bool FuckingSearchProjectsSlowly = false)
        //{
        //    // - Check cache first.
        //    string found = _objCachedHasFiles.FirstOrDefault( x => x.Equals(branchLocation));
        //    if (found != null)
        //        return true;

        //    string notfound = _objCachedHasFilesNot.FirstOrDefault(x => x.Equals(branchLocation));
        //    if (notfound != null)
        //        return false;

        //    //TODO: hash map or similar
        //    foreach(BroProject pr in Projects)
        //    {
        //        // project included cpp files
        //        foreach (string st in pr.ObjectFiles)
        //        {
        //            if (st.Contains("EngineSystem3D"))
        //            {

        //                int n = 0;
        //                n++;
        //            }
        //            if (st.Equals(branchLocation) == true)
        //            {
        //                _objCachedHasFiles.Add(branchLocation);
        //                return true;
        //            }
        //        }

        //        if (FuckingSearchProjectsSlowly == true)
        //        {
        //            //Search additional includes
        //            foreach (string additionalPath in pr.AdditionalIncludeDirectories)
        //            {
        //                string pathName = additionalPath;

        //                // - Append project directory if we are not rooted.
        //                if (System.IO.Path.IsPathRooted(additionalPath) == false)
        //                    pathName = BroCompilerUtils.GetRootedAdditionalIncludePath(additionalPath);

        //                string fileName = System.IO.Path.GetFileName(branchLocation);

        //                bool bFound = false;
        //                RecusriveFindFileInDirectory(pathName, fileName, ref bFound);
        //                if (bFound)
        //                {
        //                    _objCachedHasFiles.Add(branchLocation);
        //                    return true;
        //                }
        //            }
        //        }

        //    }
        //    _objCachedHasFilesNot.Add(branchLocation);
        //    return false;
        //}

        //public void RecusriveFindFileInDirectory(string curDir, string fileName, ref bool bFound)
        //{

        //    //NOTE: this should not throw because we validate all includes in the CTOR of this class
        //    string[] files = System.IO.Directory.GetFiles(curDir);

        //    foreach (string cf in files)
        //    {
        //        string curFile = System.IO.Path.GetFileName(cf);
        //        if (curFile.Equals(fileName))
        //        {
        //            bFound = true;
        //            return;
        //        }
        //    }

        //    // - Keep Traversing for .C or .CPP files
        //    string[] dirs = System.IO.Directory.GetDirectories(curDir);
        //    foreach (string d in dirs)
        //    {
        //        System.IO.Directory.SetCurrentDirectory(d);
        //        RecusriveFindFileInDirectory(d,fileName,ref bFound);
        //        System.IO.Directory.SetCurrentDirectory(curDir);
        //    }
        //}
        // **No longer valid since Agents use services.
        // We could though copy and deploy the services.

        //string exePath = _txtSpartanExePath.Text;
        //string dllPath = "Proteus.dll";

        //try
        //{
        //    if (!System.IO.File.Exists(exePath))
        //        throw new Exception("Spartan EXE Path " + exePath + " does not exist");

        //    string agentExePath;
        //    string agentDllPath;

        //    agentExePath = "C:\\AgentCmd.exe";
        //    agentDllPath = "C:\\Proteus.dll";

        //    if (System.IO.File.Exists(agentExePath))
        //    {
        //        _objBuildGuiController.LogOutput("Deleting ");
        //        System.IO.File.Delete(agentExePath);
        //    }
        //    _objBuildGuiController.LogOutput("Copying ");
        //    System.IO.File.Copy(exePath, agentExePath);
        //    _objBuildGuiController.LogOutput("Success");


        //    if (System.IO.File.Exists(agentDllPath))
        //    {
        //        _objBuildGuiController.LogOutput("Deleting ");
        //        System.IO.File.Delete(agentDllPath);
        //    }
        //    _objBuildGuiController.LogOutput("Copying ");
        //    System.IO.File.Copy(dllPath, agentDllPath);
        //    _objBuildGuiController.LogOutput("Success");

        //    //foreach (AgentInfo inf in _objBuildGuiController.Settings.Agents)
        //    //{
        //    //    string agentPath =
        //    //        "\\\\" + inf.Name + "\\C$";

        //    //    string agentExePath = agentPath + "\\" + "Spartan.exe";
        //    //    try
        //    //    {
        //    //        if (System.IO.File.Exists(agentExePath))
        //    //        {
        //    //            _objBuildGuiController.LogOutput("Deleting " + inf.Name);
        //    //            System.IO.File.Delete(agentExePath);
        //    //        }
        //    //        _objBuildGuiController.LogOutput("Copying " + inf.Name);
        //    //        System.IO.File.Copy(exePath, agentExePath);
        //    //        _objBuildGuiController.LogOutput("Success");
        //    //    }
        //    //    catch(Exception ex)
        //    //    {
        //    //        _objBuildGuiController.LogOutput("Could not copy file to agent " + inf.Name );
        //    //    }

        //    //}
        //}
        //catch (Exception ex)
        //{
        //    _objBuildGuiController.LogOutput("File Copy failed\n" + ex.ToString());
        //}
    }
}
