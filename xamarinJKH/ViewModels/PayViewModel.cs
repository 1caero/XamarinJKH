using System;
using System.IO;
using Xamarin.Forms;
using xamarinJKH.InterfacesIntegration;
using System.Net;
using Microsoft.AppCenter.Analytics;

namespace xamarinJKH.ViewModels
{
    public class PayViewModel : BaseViewModel
    {
        public FileStream Stream { get; set; }
        string filename;
        string ID;
        public Command LoadFile { get; set; }
        public PayViewModel(string file, string ID, bool insurance = false)
        {
            filename = file;
            this.ID = ID;
            LoadFile = new Command(() =>
            {
                
                System.Threading.Tasks.Task.Run(async () =>
                {
                    Device.BeginInvokeOnMainThread(() => IsBusy = true);
                    Analytics.TrackEvent("Проверка существования файла квитанции " + filename);
                    if (await DependencyService.Get<IFileWorker>().ExistsAsync(filename))
                    {
                        IsBusy = false;
                        Analytics.TrackEvent("Файл уже скачан");
                    }
                    else
                    {
                        Analytics.TrackEvent("Файл не скачан");
                        byte[] stream;
                        if (!insurance)
                        stream = await Server.DownloadFileAsync(ID);
                        else
                        {
                            Analytics.TrackEvent("Файл условия страхования");
                            using (var client = new WebClient())
                            {
                                stream = client.DownloadData("https://sm-center.ru/vsk_polis.pdf");
                            }
                        }
                        if (stream != null)
                        {
                            Analytics.TrackEvent("Сохраняем файл " + filename);
                            await DependencyService.Get<IFileWorker>().SaveTextAsync(filename, stream);
                        }
                        else
                        {
                            Analytics.TrackEvent("Не удалось скачать файл " + filename);
                            base.ShowError("Не удалось скачать файл");
                        }
                    }
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Analytics.TrackEvent("Получаем поток файла " + filename);
                        Stream = File.OpenRead(DependencyService.Get<IFileWorker>().GetFilePath(filename));
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(200));
                        MessagingCenter.Send<Object, FileStream>(this, "SetFileStream", Stream);
                        IsBusy = false;
                    });
                });
                
            });
        }
    }
}
