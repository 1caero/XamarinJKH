﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using xamarinJKH.ViewModels;
using AiForms.Dialogs.Abstractions;
using xamarinJKH.Utils;
using xamarinJKH.InterfacesIntegration;
using AiForms.Dialogs;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateNotificationDialog : DialogView
    {
        public UpdateNotificationDialog()
        {
            InitializeComponent();
            IsCanceledOnTouchOutside = false;
            BindingContext = new UpdateNotificationDialogViewModel();
        }

        async void CloseDialog(object sender, EventArgs args)
        {
            DialogNotifier.Cancel();
        }

        async void OpenMarket(object sender, EventArgs args)
        {
            string uri = string.Empty;
            string app_name = Xamarin.Essentials.AppInfo.PackageName;
            string name = Xamarin.Essentials.AppInfo.Name.ToLower().Trim().Replace(" ","-");
            if (Device.RuntimePlatform == "Android")
            {
                // uri = $"market://details?{app_name}";
                uri = $"https://play.google.com/store/apps/details?id={app_name}";
            }

            if (Device.RuntimePlatform == "iOS")
            {
                uri = Settings.MobileSettings.appLinkIOS;
            }

            if (!string.IsNullOrEmpty(uri))
            {
                await Xamarin.Essentials.Launcher.OpenAsync(uri);
            }
        }
    }

    public class UpdateNotificationDialogViewModel : BaseViewModel
    {
        public Color ButtonColor
        {
            get
            {
                try
                {
                    return (Color)Application.Current.Resources["MainColor"];
                }
                catch
                {
                    return Color.Blue;
                }
            }
                 
        }
    }
}