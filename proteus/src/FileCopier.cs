using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class FilePair
    {
        public FilePair(string _From, string _To)
        {
            From = _From;
            To = _To;
        }
        public string From;
        public string To;
    }
    public class FileCopier
    {
        public List<FilePair> CopyPairs = new List<FilePair>();


    }
}
