using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppCompliteDialog : DialogView
    {
        public int Id { get; set; }

        public Color HexColor { get; set; }

        public AppCompliteDialog()
        {
            InitializeComponent();

            if(Device.RuntimePlatform==Device.iOS)
            {
                if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Height < 1200)
                    Frame.Margin = new Thickness(10,15,10,15);
            }
            if (Settings.Person.UserSettings.IsCommnentRequiredOnCompleteRequest)
            {
                AppCompliteEntry.Placeholder = string.Format(AppResources.LettersLimit, 10);
            }

            var close = new TapGestureRecognizer();
            close.Tapped += (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            IconViewClose.GestureRecognizers.Add(close);
            var perform = new TapGestureRecognizer();
            perform.Tapped += (s, e) =>
            {
                BtnConf_Clicked(null, null);
            };
            FrameBtn.GestureRecognizers.Add(perform);
        }

        bool CheckComment()
        {
            bool result = true;
            // Проверяем нужно ли сообщение при переводе заявки
            if (Settings.Person.UserSettings.IsNeedCommnentOnCompleteRequest)
            {
                 result = !string.IsNullOrWhiteSpace(AppCompliteEntry.Text);
                 // Проверяем должно ли оно бьть длиннее 10 символов
                if (Settings.Person.UserSettings.IsCommnentRequiredOnCompleteRequest)
                {
                    result = result && AppCompliteEntry.Text.Length >= 10;
                    if (!result)
                    {
                        var comment = string.Format(AppResources.LettersLimit, 10);
                        Toast.Instance.Show<ToastDialog>(new { Title = comment, Duration = 1500, ColorB = Color.Gray,  ColorT = Color.White });
                    }
                }
                else
                {
                    if(!result)
                        Toast.Instance.Show<ToastDialog>(new { Title = AppResources.ErrorMessageEmpty, Duration = 1500, ColorB = Color.Gray,  ColorT = Color.White });
                }
            }
            return result;
        }

        private void BtnConf_Clicked(object sender, EventArgs e)
        {
            FrameBtn.IsEnabled = false;
            if (CheckComment())
            {
                MessagingCenter.Send<object, KeyValuePair<int, string>>(this, "performApp", new KeyValuePair<int, string>(Id, AppCompliteEntry.Text));
                DialogNotifier.Cancel();
            }
            FrameBtn.IsEnabled = true;
        }


    }
}