using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public enum  NetworkPacketType
    {
        InvalidPacketType,
        Error,
        Handshake,
        Disconnect,
        TrexExecuteCommand,
        TrexExecutionComplete
    }
    public class NetworkSettings
    {
        public const int DefaultSendAndRecvTimeout                = 10000  ;
        public const int GlobalServerIdleWaitInSeconds            = 20     ; // seconds
        public const int GlobalServerIdleTimeInMilliseconds       = 2000   ; // milliseconds
        public const int GlobalServerIdleStepTimeInMilliseconds   = 10      ; // milliseconds
        public const int HeaderLength = 4;

        private static Dictionary<NetworkPacketType, string> _objPacketHeaderDictionary = new Dictionary<NetworkPacketType, string>();
        private static bool _blnInitialized = false;

        public static void Init()
        {
            if (_blnInitialized)
                return;

            CreateDefaultDictionary();

            _blnInitialized = true; 
        }
        private static string PadHdr(string st)
        {
            //make sure this works
            string ct = "{0:";
            for(int x=0; x<HeaderLength; x++) 
                ct+="0";
            ct += "}";
            return string.Format(ct, st);
        }
        public static void CreateDefaultDictionary()
        {
            _objPacketHeaderDictionary.Add(NetworkPacketType.InvalidPacketType       , PadHdr("INV0") );
            _objPacketHeaderDictionary.Add(NetworkPacketType.Error                   , PadHdr("ERR0") );
            _objPacketHeaderDictionary.Add(NetworkPacketType.Handshake               , PadHdr("HS00") );
            _objPacketHeaderDictionary.Add(NetworkPacketType.Disconnect              , PadHdr("DC00"));
            _objPacketHeaderDictionary.Add(NetworkPacketType.TrexExecuteCommand      , PadHdr("TEX0"));
            _objPacketHeaderDictionary.Add(NetworkPacketType.TrexExecutionComplete   , PadHdr("TCM0"));

        }
        public static string PacketTypeToHeaderString(NetworkPacketType pt)
        {
            string value;
            if (_objPacketHeaderDictionary.TryGetValue(pt, out value) == false)
                throw new NotImplementedException();
            return value;
        }
        public static NetworkPacketType HeaderStringToPacketType(string header)
        {
            NetworkPacketType myValue = _objPacketHeaderDictionary.FirstOrDefault(x => x.Value == header).Key;

            if (myValue == NetworkPacketType.InvalidPacketType)
                throw new NotImplementedException();
            return myValue;
        }
        //TODO:
        //Load these settings from a file.

    }
}
