using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class BuildStats
    {
        public DateTime BuildStartTime;
        public DateTime BuildEndTime;
        public int TotalFileCount;
        public int NumFilesBuilt;
        public int NumFilesPending;
        public int NumFilesRemaining;
        public List<float> FileBuildTime;
        public int NumErrors;
        public float AvgFilesPerSecond;
    }
}
