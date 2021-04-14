using System;
using System.IO;
using FFImageLoading;

namespace xamarinJKH.Utils.FileUtils
{
    public class FileData
    {
        public string FilePath { get; }
        public string GetFileName { get; }
        public Stream DataArray { get; }
        public byte[] Bytes { get => DataArray.ToByteArray(); }

        public FileData(string filePath, string getFileName, Stream func)
        {
            FilePath = filePath;
            GetFileName = getFileName;
            DataArray = func;
        }
    }
}