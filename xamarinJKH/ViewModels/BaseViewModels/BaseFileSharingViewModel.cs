﻿using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;
using Plugin.Media.Abstractions;
using Plugin.Media;
using Plugin.Permissions.Abstractions;
using Plugin.Permissions;
using System.Threading.Tasks;

namespace xamarinJKH.ViewModels
{
    public class BaseFileSharingViewModel:BaseViewModel
    {
        public virtual Command TakePhoto { get; set; }
        public virtual Command PickPhoto { get; set; }
        public virtual Command PickFile { get; set; }
        public virtual Command SendToServer { get; set; }

        public MediaFile Photo { get; set; }
        public MediaFile File { get; set; }
        public BaseFileSharingViewModel()
        {
            Task.Run(async () => await CrossMedia.Current.Initialize());
            SendToServer = new Command(() => IsLoading = false);
            TakePhoto = new Command(async () =>
            {
                if (!CrossMedia.IsSupported || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    ShowError("Камера недоступна");
                }
                else
                {
                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Directory = "Demo"
                    });

                    if (file == null)
                    {
                        return;
                    }

                    LoadingMessage = "Загрузка фотографии";
                    IsLoading = true;
                    Photo = file;
                    SendToServer.Execute(null);
                }
            });

            PickPhoto = new Command(async () =>
            {
                if (!CrossMedia.IsSupported || !CrossMedia.Current.IsPickPhotoSupported)
                {
                    ShowError("Выбор из галереи недоступен");
                }
                else
                {
                    var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium
                    });

                    if (file == null)
                    {
                        return;
                    }

                    LoadingMessage = "Загрузка фотографии";
                    IsLoading = true;
                    Photo = file;
                    SendToServer.Execute(null);
                }
            });

            PickFile = new Command(async () =>
            {
                var file = await CrossFilePicker.Current.PickFile();
                if (file != null)
                {
                    if (file.DataArray.Length > 10E7)
                    {
                        ShowError("Размер файла превышает 10мб");
                    }
                    else
                    {
                        LoadingMessage = "Загрузка файла";
                        IsLoading = true;
                        SendToServer.Execute(null);
                    }
                }
            });
        }
    }
}
