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

        public RatingBarContentView(Color hexColor, RequestInfo request)
        {
            HexColor = hexColor;
            _Request = request;
            InitializeComponent();
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

        public async Task StartProgressBar(string title = "Закрытие заявки..", double opacity = 0.6)
        {
            // Loading settings
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
                if (!text.Equals(""))
                {
                    CommonResult result =
                        await server.CloseApp(_Request.ID.ToString(), text, RatingBar.Rating.ToString());
                    if (result.Error == null)
                    {
                        await ShowToast("Заявка закрыта");
                        await PopupNavigation.Instance.PopAsync();
                    }
                    else
                    {
                        await ShowToast(result.Error);
                    }
                }
                else
                {
                    await ShowToast("Заполните комментарий к заявке");
                }
            });
        }
    }
}