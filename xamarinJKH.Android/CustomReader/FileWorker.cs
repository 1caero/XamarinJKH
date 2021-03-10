using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using xamarinJKH.Droid.CustomReader;
using xamarinJKH.InterfacesIntegration;

[assembly: Dependency(typeof(FileWorker))]
namespace xamarinJKH.Droid.CustomReader
{
    public class FileWorker : IFileWorker
    {
        public Task<bool> ExistsAsync(string filename)
        {
            // получаем путь к файлу
            string filepath = GetFilePath(filename);
            // существует ли файл
            bool exists = File.Exists(filepath);
            return Task<bool>.FromResult(exists);
        }

        public async Task SaveTextAsync(string filename, byte[] text)
        {
            string filepath = GetFilePath(filename);
            using (FileStream writer = new FileStream(filepath, FileMode.OpenOrCreate))
            {
                await writer.WriteAsync(text);
            }
        }

        public async Task<string> LoadTextAsync(string filename)
        {
            string filepath = GetFilePath(filename);
            using (StreamReader reader = File.OpenText(filepath))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public Task<IEnumerable<string>> GetFilesAsync()
        {
            // получаем все все файлы из папки
            IEnumerable<string> filenames = from filepath in Directory.EnumerateFiles(GetDocsPath())
                select Path.GetFileName(filepath);
            return Task<IEnumerable<string>>.FromResult(filenames);
        }

        public Task DeleteAsync(string filename)
        {
            // удаляем файл
            File.Delete(GetFilePath(filename));
            return Task.FromResult(true);
        }

        // вспомогательный метод для построения пути к файлу
        public string GetFilePath(string filename)
        {
            return Path.Combine(GetDocsPath(), filename);
        }

        // получаем путь к папке MyDocuments
        string GetDocsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}