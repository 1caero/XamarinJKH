﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Main;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RatingAppMarketDialog  : Rg.Plugins.Popup.Pages.PopupPage
    {
        public RatingAppMarketDialog()
        {
            InitializeComponent();
            Analytics.TrackEvent("Диалог оценки приложения");
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
        }

        async void OpenMarket(object sender, EventArgs args)
        {
            int rating = RatingBar.Rating;
            Preferences.Set("rate", false);
            if (rating > 4)
            {

                string uri = string.Empty;
                string app_name = Xamarin.Essentials.AppInfo.PackageName;
                string name = Xamarin.Essentials.AppInfo.Name.ToLower().Trim().Replace(" ", "-");
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
                await PopupNavigation.Instance.PopAsync();
            }
            else
            {
                await PopupNavigation.Instance.PopAsync();
                await Navigation.PushAsync(new AppPage());
            }

           
        }

        private async void BtnNot_OnClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void BtnLater_OnClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}