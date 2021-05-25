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
        
        public string Length 
        {
            get
            {
                double size = DataArray.Length;
                string sizeType = AppResources.b;
                if (size >= 1024)
                {
                    size /= 1024;
                    sizeType = AppResources.kb;
                }

                if (size >= 1024)
                {
                    size /= 1024;
                    sizeType = AppResources.mb;
                }

                return Math.Round(size, 2).ToString() + " " + sizeType;
            }
        }

        public FileData(string filePath, string getFileName, Stream func)
        {
            FilePath = filePath;
            GetFileName = getFileName;
            DataArray = func;
        }
        public static byte[] StreamToByteArray(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
            }
            else
            {
                return ReadFully(stream);
            }
        }
        public static string getFileName(string path)
        {
            try
            {
                string[] fileName = path.Split('/');
                return fileName[fileName.Length - 1];
            }
            catch (Exception ex)
            {
                return "filename";
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}