using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppCompliteDialog : DialogView
    {
        public int Id { get; set; }

        public Color HexColor { get; set; }

        public AppCompliteDialog(Color color, int id)
        {
            InitializeComponent();

            Id = id;
            AppCompliteEntry.Placeholder = string.Format(AppResources.LettersLimit, 10);

            var close = new TapGestureRecognizer();
            close.Tapped += (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            IconViewClose.GestureRecognizers.Add(close);
        }


        public AppCompliteDialog()
        {
            InitializeComponent();

            //Id = Id;
            AppCompliteEntry.Placeholder = string.Format(AppResources.LettersLimit, 10);

            var close = new TapGestureRecognizer();
            close.Tapped += (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            IconViewClose.GestureRecognizers.Add(close);
        }


        private void BtnConf_Clicked(object sender, EventArgs e)
        {
            if (AppCompliteEntry.Text!=null && AppCompliteEntry.Text.Length >= 10)
            {
                MessagingCenter.Send<object, KeyValuePair<int, string>>(this, "performApp", new KeyValuePair<int, string>(Id, AppCompliteEntry.Text));
                DialogNotifier.Cancel();
            }
            else
            {
                var comment = string.Format(AppResources.LettersLimit, 10);
                Toast.Instance.Show<ToastDialog>(new { Title = comment, Duration = 1500, ColorB = Color.Gray,  ColorT = Color.White });
            }
        }


    }
}