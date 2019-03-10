using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteus;

namespace Spartan
{
    public class AgentDiagnostics
    {
        public static AgentInfo GetAgentInfo(string name)
        {
            AgentInfo newInf = new AgentInfo();
            Spartan.CoordinatorAgent ag;

            newInf = new AgentInfo();
            newInf.Name = name;//must be set to connect
            ag = new CoordinatorAgent(name);

            try
            {
                // Try to connect without doing a handshake.
                bool? success = ag.Connect(true, true);
                if (success == false)
                {
                    newInf.IsConnected = false;
                }
                else
                {
                    newInf.IsConnected = AgentDiagnostics.GetAgentInfoFromAgent(ag, ref newInf);
                }

                ag.Disconnect();
            }
            catch (System.Net.Sockets.SocketException)
            {
                newInf.IsConnected = false;
            }
            finally
            {
                ag.Disconnect();
            }

            return newInf;
        }
        private static bool GetAgentInfoFromAgent(Spartan.CoordinatorAgent ag,ref AgentInfo newInf)
        {
            int timeout = 1000;
            int tB;
            tB = System.Environment.TickCount;
            bool blnSuccess = false;

            while (true)
            {
                if ((System.Environment.TickCount - tB) > timeout)
                {
                    //exit out.  We are not connected
                    break;
                }

                string packet = ag.Recv();
                if (packet != string.Empty)
                {
                    PacketType npt = SpartanGlobals.GetPacketType(ref packet);
                    if (npt != PacketType.AgentStatusInfo)
                        throw new Exception("Invalid packet type " + npt.ToString() + " returned");
                    string coordName = NetworkUtils.UnpackString(ref packet);

                    string name = newInf.Name;
                    
                    newInf.Deserialize(ref packet);
                   
                    //Save name in case of serialization error, or else it will blow up
                    if (newInf.Name == null)
                        newInf.Name = name;

                    blnSuccess = true;
                    break;
                }

                System.Threading.Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();
            }

            return blnSuccess;

        }
    
        public static string InstallAgent(AgentInfo ag)
        {
            string ret = "";
            string str;
            Dictionary<string, string> paths = new Dictionary<string, string>();

            FileCopier fc = new FileCopier();

            List<string> from = new List<string>
            {
                System.IO.Path.GetFullPath(@"..\bin\AgentService.exe"),                 
                System.IO.Path.GetFullPath(@"..\bin\Proteus.dll"),                      
                System.IO.Path.GetFullPath(@"..\bin\Spartan.dll"),                                
                System.IO.Path.GetFullPath(@"..\svc_install\AgentServiceArguments.txt"),
                System.IO.Path.GetFullPath(@"..\svc_install\installup.txt"),            
                System.IO.Path.GetFullPath(@"..\svc_install\_InstallAgent.bat"),      
            };
            List<string> to = new List<string>
            {
                @"\\" + ag.Name + @"\C$\AgentService\AgentService.exe",
                @"\\" + ag.Name + @"\C$\AgentService\Proteus.dll",
                @"\\" + ag.Name + @"\C$\AgentService\Spartan.dll",
                @"\\" + ag.Name + @"\C$\AgentService\AgentServiceArguments.txt",
                @"\\" + ag.Name + @"\C$\AgentService\installup.txt",
                @"\\" + ag.Name + @"\C$\AgentService\_InstallAgent.bat"
            };

            bool success = false;

            // Stop the service
            str = TrexUtils.SendTrexCommand(ag.Name, "cmd /c net stop AgentService", ref success);
            if (str != string.Empty)
                ret += str;

            if (!success)
                return ret;

            // Copy
            ret += ("[[[[[[[[" + ag.Name + " copying files");
            str = FileUtils.CopyFilesToMachine(ag.Name, from, to);
            if (str != string.Empty)
            {
                ret += str;
                return ret;
            }

            // Install and start service.
            ret += ("[[[[[[[[" + ag.Name + " installing ");
            str = TrexUtils.SendTrexCommand(ag.Name, "cmd /c \\\\" + ag.Name + "\\C$\\AgentService\\_InstallAgent.bat", ref success, 50000);
            if (str != string.Empty)
               ret += (str);

            return ret;
        }
    
    }
}
