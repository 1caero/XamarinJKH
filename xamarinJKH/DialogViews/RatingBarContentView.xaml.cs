﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RatingBarContentView : Rg.Plugins.Popup.Pages.PopupPage
    {
        private RestClientMP server = new RestClientMP();
        public Color HexColor { get; set; }
        public RequestInfo _Request { get; set; }
        bool IsConst = false;

        public RatingBarContentView(Color hexColor, RequestInfo request, bool isConst)
        {
            HexColor = hexColor;
            _Request = request;
            InitializeComponent();
            Frame.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.Transparent);
            if(Device.RuntimePlatform==Device.iOS)
            {
                commentFrame.BackgroundColor = Color.White;
                if(Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width<700)
                {
                    //Frame.Margin = new Thickness(15, 80, 15, 15);
                    Frame.Padding = new Thickness(15, 15, 15, 0);
                    LabelDate.FontSize = 12;
                    ls1.FontSize = 10;
                    ls2.FontSize = 10;
                    ls3.FontSize = 10;
                    BordlessEditor.FontSize = 10;
                    commentFrame.Margin = new Thickness(0, 10, 0, 0);
                }
                if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
                {
                    //Frame.Margin = new Thickness(15, 100, 15, 15);
                    Frame.Padding = new Thickness(15, 20, 15, 0);
                    
                    LabelDate.FontSize = 16;
                    ls1.FontSize = 14;
                    ls2.FontSize = 14;
                    ls3.FontSize = 14;
                    BordlessEditor.FontSize = 14;
                    commentFrame.Margin = new Thickness(0, 10, 0, 0);
                }

            }

            IsConst = isConst;
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
            BindingContext = this;
        }

        private async void CloseApp(object sender, EventArgs e)
        {
            await StartProgressBar();
        }

        public async Task ShowToast(string title)
        {
            Toast.Instance.Show<ToastDialog>(new {Title = title, Duration = 1500});
            // Optionally, view model can be passed to the toast view instance.
        }

        public async Task StartProgressBar(string title = "", double opacity = 0.6)
        {
            // Loading settings
            if (string.IsNullOrEmpty(title))
                title = AppResources.ClosingApp;
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = HexColor,
                OverlayColor = Color.Black,
                Opacity = opacity,
                DefaultMessage = title,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                string text = BordlessEditor.Text;
                if (IsConst)
                {
                    CommonResult result = await server.CloseAppConst(_Request.ID.ToString());
                    if (result.Error == null)
                    {
                        await ShowToast(AppResources.AppClosed);
                        await PopupNavigation.Instance.PopAsync();
                    }
                    else
                    {
                        await ShowToast(result.Error);
                    }
                }
                else if (!text.Equals("") && !IsConst)
                {
                    CommonResult result = await server.CloseApp(_Request.ID.ToString(), text, RatingBar.Rating.ToString());
                    if (result.Error == null)
                    {
                        await ShowToast(AppResources.AppClosed);
                        await PopupNavigation.Instance.PopAsync();
                    }
                    else
                    {
                        await ShowToast(result.Error);
                    }                   
                }
                else
                {
                    await ShowToast(AppResources.ErrorFillCommant);
                }
            });
        }

        Thickness frameMargin = new Thickness();

        private void BordlessEditor_Focused(object sender, FocusEventArgs e)
        {
            if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            {
               frameMargin = Frame.Margin;
               Device.BeginInvokeOnMainThread(()=> { Frame.Margin = new Thickness(15, 0, 15, 15); });
            }
        }

        private void BordlessEditor_Unfocused(object sender, FocusEventArgs e)
        {
            if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                Device.BeginInvokeOnMainThread(() => { Frame.Margin = frameMargin; }) ;
            }
        }
    }
}