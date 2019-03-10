using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
namespace Proteus
{
    public class NetworkUtils
    {
        #region PARSING

        public const string IntegerSizeMask = "000000000000";

        public static string PackCrc32(string packet)
        {
            uint crc = Crc32.Compute(packet);
            return PackUint(crc);
        }
        public static bool UnpackAndCheckCrc32(ref string packetWithCrc32Appended)
        {
            // Returns false if the CRC32 failed, or the packet was malformed.

            if (packetWithCrc32Appended.Length - IntegerSizeMask.Length < 0)
                return false;

            string CrcUInt = packetWithCrc32Appended.Substring(packetWithCrc32Appended.Length - IntegerSizeMask.Length, IntegerSizeMask.Length);
            string packetWithoutCrc = packetWithCrc32Appended.Substring(0, packetWithCrc32Appended.Length - IntegerSizeMask.Length);
            uint crcUnpacked = System.Convert.ToUInt32(CrcUInt);
            uint crcComputed = Crc32.Compute(packetWithoutCrc);

            if (crcUnpacked != crcComputed)
                return false;

            return true;
        }
        public static NetworkPacketType UnpackPacketType(ref string data)
        {
            string strCode = ParseDataChunk(ref data, NetworkSettings.HeaderLength);

            return NetworkSettings.HeaderStringToPacketType(strCode);
        }
        public static string PackPacketType(NetworkPacketType npt)
        {
            return NetworkSettings.PacketTypeToHeaderString(npt);
        }
        public static string PackString(string str)
        {
            if (str == null)
                str = "";
            return GetStringHeader(str) + str;
        }
        public static string PackBytes(byte[] data)
        {
            string str = System.Text.Encoding.ASCII.GetString(data);
            return PackString(str);
        }
        public static string GetStringHeader(string str)
        {
            return str.Length.ToString(IntegerSizeMask);
        }
        public static string ParseDataChunk(ref string strBuffer, int iChunkSize)
        {
            string ret;

            if (strBuffer.Length < iChunkSize)
                iChunkSize = strBuffer.Length;

            ret = strBuffer.Substring(0, iChunkSize);
            strBuffer = strBuffer.Substring(iChunkSize, strBuffer.Length - iChunkSize);

            return ret;
        }
        public static string UnpackString(ref string buffer)
        {
            return ReadStringFromStream(ref buffer);
        }
        public static string ReadStringFromStream(ref string buffer)
        {
            //Read Len
            string strLen = ReadFixedStringFromStream(ref buffer, IntegerSizeMask.Length);
            int iStringLen;
            if (strLen == "")
            {
                int n = 0;
                n++;
            }
            iStringLen = Convert.ToInt32(strLen);

            // Read Data
            return ReadFixedStringFromStream(ref buffer, iStringLen);
        }
        public static string ReadFixedStringFromStream(ref string buffer, int len)
        {
            return NetworkUtils.ParseDataChunk(ref buffer, len);
        }
        public static string PackUint(uint i)
        {
            return i.ToString(IntegerSizeMask);
        }
        public static string PackInt(int i)
        {
            return i.ToString(IntegerSizeMask);
        }
        public static int UnpackInt(ref string str)
        {
            string istr = ParseDataChunk(ref str, IntegerSizeMask.Length);

            return System.Convert.ToInt32(istr);
        }
        public static uint UnpackUint(ref string str)
        {
            string istr = ParseDataChunk(ref str, IntegerSizeMask.Length);

            return System.Convert.ToUInt32(istr);
        }
        public static string PackDateTime(DateTime dt)
        {
            return String.Format("{0,48:F}", dt);
        }
        public static DateTime UnpackDateTime(ref string buffer)
        {
            DateTime dt;
            string part = ParseDataChunk(ref buffer, 48);
            dt = DateTime.Parse(part.Trim());
            return dt;
        }

        #endregion

        #region TCPIP
        public static System.Net.IPAddress GetIpAddress(string machineName = "")
        {
            System.Net.IPAddress ip = null;
            System.Net.IPHostEntry hostEntry;

            if (machineName == "")
                machineName = System.Environment.MachineName;

            hostEntry = System.Net.Dns.GetHostEntry(machineName);

            if (hostEntry == null)
                Globals.Logger.LogError("Failed to find host enrty for computer name:" + machineName);

            for (int n = 0; n < hostEntry.AddressList.Length; n++)
                if (hostEntry.AddressList[n].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ip = hostEntry.AddressList[n];

            if (ip == null)
                Globals.Logger.LogError("Failed to find ip address for computer host:" + machineName);

            return ip;
        }
        #endregion


        #region HTML
        public static string GetHtmlDataFromUrlViaSockets(string url)
        {
            Socket socket;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(url, 80);
            string GETrequest = "GET / HTTP/1.1\r\nHost: " + GetIpAddress().ToString() + "\r\nConnection: keep-alive\r\nAccept: text/html\r\nUser-Agent: Mozilla-Firefox\r\n\r\n";
            socket.Send(Encoding.ASCII.GetBytes(GETrequest));

            bool flag = true; // just so we know we are still reading
            string headerString = ""; // to store header information
            int contentLength = 0; // the body length
            byte[] bodyBuff = new byte[0]; // to later hold the body content
            while (flag)
            {
                // read the header byte by byte, until \r\n\r\n
                byte[] buffer = new byte[1];
                socket.Receive(buffer, 0, 1, 0);
                headerString += Encoding.ASCII.GetString(buffer);
                if (headerString.Contains("\r\n\r\n"))
                {
                    // header is received, parsing content length
                    // I use regular expressions, but any other method you can think of is ok
                    Regex reg = new Regex("\\\r\nContent-Length: (.*?)\\\r\n");
                    Match m = reg.Match(headerString);
                    contentLength = int.Parse(m.Groups[1].ToString());
                    flag = false;
                    // read the body
                    bodyBuff = new byte[contentLength];
                    socket.Receive(bodyBuff, 0, contentLength, 0);
                }
            }
            return Encoding.ASCII.GetString(bodyBuff);
        }
        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 3000;
                return w;
            }
        }
        public static string GetHtmlDataFromUrl(string url)
        {
            string ContentHtml = string.Empty ;
            if (!NetworkUtils.ValidateUrl(url))
                return "";
            try
            {
                //ContentHtml = new System.Net.WebClient().DownloadString(url);

                MyWebClient wc = new MyWebClient();
                wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0");
                
                System.IO.Stream resStream = wc.OpenRead(url);
                System.IO.StreamReader sr = new System.IO.StreamReader(resStream, System.Text.Encoding.Default);
                ContentHtml = sr.ReadToEnd();

                resStream.Close();
                //sr.Close();
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError(ex.ToString(),false);
            }
            return ContentHtml;
        }

        public static bool ValidateUrl(string url)
        {
            Uri result = null;
            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out result);
            if (result != null)
            {
                try
                {
                    string scheme = result.Scheme;
                    return (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        #endregion
    }
}
