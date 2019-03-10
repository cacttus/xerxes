using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class BuildCache
    {
        private enum LoadStatus { Ok, Error }

        private string _strCacheRoot;
        private Dictionary<string, DateTime> _objCacheTimes;
        private const string _strCacheFileName = "build.cache";
        private LoadStatus _enumLoadStatus = LoadStatus.Ok;
        private FileDependencyCache _objDependencyTree;

        // ** Header files are cached.
        // ** We don't cahe object files, since we can know if an object file is out of date by simply checking the .obj.

        public BuildCache(string cacheRoot)
        {
            _enumLoadStatus = LoadStatus.Ok;
            _objCacheTimes = new Dictionary<string, DateTime>();

            _objDependencyTree = new FileDependencyCache();
            _strCacheRoot = cacheRoot;
            _strCacheRoot = System.IO.Path.Combine(_strCacheRoot, _strCacheFileName);
        }
        public bool ObjectFileDoesNotExistOrIsOutOfDate(string astrSourceFileLoc, string astrObjectFileLoc, BuildTarget aobjBuildTarget)
        {
            if (_enumLoadStatus == LoadStatus.Error)
                return true;

            bool b = System.IO.File.Exists(astrSourceFileLoc);
            if(!b)
                return true;

            b = System.IO.File.Exists(astrObjectFileLoc);
            if (!b)
                return true;

            if (FileUtils.GetLastWriteTime(astrSourceFileLoc) > FileUtils.GetLastWriteTime(astrObjectFileLoc))
                return true;

            //now for the big guns - look up all header files to see if one changed.
            if (_objDependencyTree.ObjectIsDirty(astrSourceFileLoc, astrObjectFileLoc, this, aobjBuildTarget))
                return true;

            return false;
        }
        public bool HeaderFileDoesNotExistOrIsOutOfDate_Not_Recursive(string astrHeaderFileLoc, DateTime adatObjectFileLastWriteTime)
        {
            if (_enumLoadStatus == LoadStatus.Error)
                return true;

            bool b = System.IO.File.Exists(astrHeaderFileLoc);
            if (!b)
                return true;

            if (FileUtils.GetLastWriteTime(astrHeaderFileLoc) > adatObjectFileLastWriteTime)
                return true;

            return false;
        }
        //public void Load()
        //{
        //    _enumLoadStatus = LoadStatus.Ok;
        //    _objCacheTimes.Clear();

        //    if (!System.IO.File.Exists(_strCacheRoot))
        //    {
        //        Globals.Logger.LogError("Could not find cache file at " + _strCacheRoot + "\n Build will not use cache.");
        //        return;
        //    }
        //    Byte[] data = System.IO.File.ReadAllBytes(_strCacheRoot);

        //    using (BinaryReader reader = new BinaryReader(File.Open(_strCacheRoot, FileMode.Open)))
        //    {
        //        //Header
        //        string strHeader = reader.ReadString();
        //        if (strHeader != "BC")
        //        {
        //            // fail silently
        //            Globals.Logger.LogError("Invalid file format for cache file " + _strCacheRoot + " ", false);
        //            _enumLoadStatus = LoadStatus.Error;
        //            return;
        //        }

        //        int countFiles = reader.ReadInt32();

        //        //Data
        //        for (int n = 0; n < countFiles; n++)
        //        {
        //            string loc;
        //            long binaryTime;
        //            DateTime time;

        //            loc = reader.ReadString();
        //            binaryTime = reader.ReadInt32();
        //            time = DateTime.FromBinary(binaryTime);

        //            _objCacheTimes.Add(loc, time);   
        //        }
        //    }

        //}
        //public void Save()
        //{
        //    //Format
        //    // ["BC"][count - int64]  [strlen - int64][str - char*][datetime - long][....]

        //    string cacheDir = System.IO.Path.GetDirectoryName(_strCacheRoot);
        //    if (!System.IO.Directory.Exists(cacheDir))
        //        System.IO.Directory.CreateDirectory(cacheDir);

        //    using (BinaryWriter writer = new BinaryWriter(File.Open(_strCacheRoot, FileMode.Create)))
        //    {
        //        //Header
        //        writer.Write("BC");
        //        writer.Write(_objCacheTimes.Keys.Count);

        //        //Data
        //        foreach (string key in _objCacheTimes.Keys)
        //        {
        //            DateTime dt = _objCacheTimes[key];
        //            long dtb = dt.ToBinary();

        //            writer.Write(key);
        //            writer.Write(dtb);
        //        }
        //    }
        //}

        //public void CacheFile(string fileLoc)
        //{
        //    DateTime value;
        //    if (!_objCacheTimes.TryGetValue(fileLoc, out value))
        //    {
        //        _objCacheTimes.Add(fileLoc, DateTime.Now);
        //    }
        //    else
        //    {
        //        _objCacheTimes[fileLoc] = DateTime.Now;
        //    }
        //}


    }
}
