using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public enum TrexExecutionType
    {
        Sync,
        Async
    }
    public class TrexUtils
    {
        public static int ServerBindPort = 56118;

        public static string SendTrexCommand(string machineName, 
            string command, 
            ref bool success,
            int timeoutMs = 20000,
            TrexExecutionType execType = TrexExecutionType.Sync, 
            bool blnWaitForResponse = true)
        {
            string ret;
            try
            {
                TrexClient client = new TrexClient(machineName);
                client.Connect();

                if (!client.IsConnected())
                    Globals.Logger.LogError("Failed to connect to trex server.", true);

                client.SendCommand(command, execType, timeoutMs);

                if (blnWaitForResponse == true)
                {
                    while (client.TrexClientState != TrexClientState.Success &&
                        client.TrexClientState != TrexClientState.Failure &&
                        client.TrexClientState != TrexClientState.Timedout)
                    {
                        client.Update();
                        System.Windows.Forms.Application.DoEvents();
                    }
                }

                if (client.TrexClientState == TrexClientState.Success)
                    success = true;
                else
                    success = false;

                ret = client.LastReturnValue;
                client.Disconnect();
            }
            catch (Exception ex)
            {
                ret = "Failed to connect to trex service. \n" + ex.ToString();
            }

            return ret;
        }
        public static void RunServer()
        {
            try
            {
                TrexServer _objServer;
                System.IO.Directory.SetCurrentDirectory(
                    System.AppDomain.CurrentDomain.BaseDirectory
                  );

                Globals.InitializeGlobals("TrexService.log");
                Globals.Logger.LogInfo("Starting trex service thread.");

                Globals.Logger.LogInfo("Base directory " + System.AppDomain.CurrentDomain.BaseDirectory);
                Globals.Logger.LogInfo("Creating server.");

                _objServer = new TrexServer(TrexUtils.ServerBindPort);

                Globals.Logger.LogInfo("Begin Listening.");

                _objServer.ListenForData(); //Blocking
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError("Error in trex service, the service will now restart.  Error:\n" + ex.ToString());
                System.IO.File.Create("Trex exited unexpectedly.txt");
                System.Environment.Exit(1);
            }
        }
    }
}
