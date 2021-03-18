using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.CustomRenderers;
using xamarinJKH.Utils;

namespace xamarinJKH.Main
{
    public class PaysCell : ViewCell
    {
        private Label ident = new Label();
        private Label adress = new Label();
        private StackLayout dell = new StackLayout();
        private Label sumPayDate = new Label();
        private Label sumPay = new Label();

        public PaysCell()
        {
            MaterialFrame frame = new MaterialFrame();
            frame.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"], Color.White);
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frame.SetOnAppTheme(MaterialFrame.ElevationProperty, 0, 20);
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(10, 0, 10, 10);
            frame.Padding = new Thickness(15, 15, 15, 15);
            frame.CornerRadius = 30;

            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Vertical;

            StackLayout dateIdent = new StackLayout();
            dateIdent.Orientation = StackOrientation.Horizontal;

            StackLayout identAdress = new StackLayout();
            identAdress.HorizontalOptions = LayoutOptions.StartAndExpand;
            identAdress.Spacing = 0;
            
            ident.FontSize = 15;
            ident.TextColor = Color.Black;
            adress.FontSize = 12;
            adress.TextColor = Color.Gray;
            adress.FontFamily = "Roboto";

            identAdress.Children.Add(ident);
            identAdress.Children.Add(adress);


            IconView x = new IconView();
            x.Source = "ic_close";
            x.Foreground = (Color)Application.Current.Resources["MainColor"];
            x.HeightRequest = 10;
            x.WidthRequest = 10;

            Label close = new Label();
            close.Text = AppResources.Delete;
            close.TextColor = (Color)Application.Current.Resources["MainColor"];
            close.FontSize = 15;
            close.TextDecorations = TextDecorations.Underline;
            dell.Orientation = StackOrientation.Horizontal;
            dell.HorizontalOptions = LayoutOptions.EndAndExpand;
            dell.VerticalOptions = LayoutOptions.Center;
            dell.MinimumWidthRequest = 80;

            dell.Children.Add(x);
            dell.Children.Add(close);
            
           

            dateIdent.Children.Add(identAdress);
            dateIdent.Children.Add(dell);

            Label separator = new Label();

            separator.HeightRequest = 1;
            separator.BackgroundColor = Color.Gray;
            separator.Margin = new Thickness(0, 5, 0, 5);
            container.Children.Add(dateIdent);
            container.Children.Add(separator);

            StackLayout sums = new StackLayout();
            sums.Orientation = StackOrientation.Horizontal;
            sums.Margin = new Thickness(30, 0, 30, 5);

            sumPayDate.Text = string.Format("Сумма к оплате{0} на 31.05.2020", Environment.NewLine);
            sumPayDate.FontSize = 15;
            sumPayDate.TextColor = Color.Gray;
            sumPayDate.HorizontalTextAlignment = TextAlignment.End;
            sumPayDate.Margin = new Thickness(0, 0, 15, 0);

            sumPay.Text = "4593.01 руб";
            sumPay.TextColor = (Color)Application.Current.Resources["MainColor"];
            sumPay.FontSize = 25;
            sumPay.VerticalOptions = LayoutOptions.Center;

            sums.Children.Add(sumPayDate);
            sums.Children.Add(sumPay);

            container.Children.Add(sums);

            Frame frameBtn = new Frame();
            frameBtn.HorizontalOptions = LayoutOptions.FillAndExpand;
            frameBtn.VerticalOptions = LayoutOptions.Start;
            frameBtn.Padding = 0;
            frameBtn.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            frameBtn.CornerRadius = 10;

            StackLayout containerBtn = new StackLayout();
            containerBtn.Orientation = StackOrientation.Horizontal;
            containerBtn.Spacing = 0;
            containerBtn.HorizontalOptions = LayoutOptions.CenterAndExpand;

            IconView image = new IconView();
            image.Source = "ic_pays";
            image.Foreground = Color.White;
            image.HeightRequest = 30;
            image.WidthRequest = 30;

            Label btn = new Label();
            btn.TextColor = Color.White;
            btn.BackgroundColor = Color.Transparent;
            btn.HorizontalOptions  = LayoutOptions.Center;
            btn.Margin = new Thickness(13,13,0,13);
            btn.FontAttributes = FontAttributes.Bold;
            btn.FontSize = 16;
            btn.Text = AppResources.Pay;

            containerBtn.Children.Add(image);
            containerBtn.Children.Add(btn);

            frameBtn.Content = containerBtn;

            container.Children.Add(frameBtn);

            Label payPeriod = new Label();

            FormattedString formatted = new FormattedString();

            formatted.Spans.Add(new Span
            {
                Text = AppResources.PayProcessing,
                FontSize = 12
            });
            formatted.Spans.Add(new Span
            {
                Text = " 2-3 ",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            });
            formatted.Spans.Add(new Span
            {
                Text = AppResources.WorkDays,
                FontSize = 12
            });

            payPeriod.FormattedText = formatted;
            payPeriod.TextColor = (Color)Application.Current.Resources["MainColor"];
            payPeriod.FontSize = 12;
            payPeriod.HorizontalOptions = LayoutOptions.CenterAndExpand;

            container.Children.Add(payPeriod);

            frame.Content = container;

            View = frame;
        }

        public static readonly BindableProperty IdentProperty =
            BindableProperty.Create("Ident", typeof(string), typeof(PaysCell), "");

        public static readonly BindableProperty AdressIdentProperty =
            BindableProperty.Create("AdressIdent", typeof(string), typeof(PaysCell), "");

        public static readonly BindableProperty DateIdentProperty =
            BindableProperty.Create("DateIdent", typeof(string), typeof(PaysCell), "");

        public static readonly BindableProperty SumPayProperty =
            BindableProperty.Create("SumPay", typeof(string), typeof(PaysCell), "");

        public string Ident
        {
            get { return (string) GetValue(IdentProperty); }
            set { SetValue(IdentProperty, value); }
        }

        public string AdressIdent
        {
            get { return (string) GetValue(AdressIdentProperty); }
            set { SetValue(AdressIdentProperty, value); }
        }

        public string DateIdent
        {
            get { return (string) GetValue(DateIdentProperty); }
            set { SetValue(DateIdentProperty, value); }
        }

        public string SumPay
        {
            get { return (string) GetValue(SumPayProperty); }
            set { SetValue(SumPayProperty, value); }
        }

        int fs = 15;
        

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                if(DeviceDisplay.MainDisplayInfo.Width<700)
                {
                    fs = 13;
                }
                
                var delLs = new TapGestureRecognizer();
                delLs.Tapped += async (s, e) =>
                {
                    ((PaysPage) Settings.paysPage).DellLs(Ident);
                };
                dell.GestureRecognizers.Add(delLs);
                
                FormattedString formattedIdent = new FormattedString();
                formattedIdent.Spans.Add(new Span
                {
                    Text = $"{AppResources.Acc} ",
                    TextColor = Color.Black,
                    FontSize = 15
                });
                formattedIdent.Spans.Add(new Span
                {
                    Text = $"{AppResources.Number} " + Ident,
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 15
                });
                ident.FormattedText = formattedIdent;
                FormattedString formattedPayDate = new FormattedString();

                formattedPayDate.Spans.Add(new Span
                {
                    Text = $"{AppResources.SumToPay}\n",
                    TextColor = Color.Gray,
                    FontSize = fs
                });
                formattedPayDate.Spans.Add(new Span
                {
                    Text = $"{AppResources.By} " + DateIdent + ":",
                    TextColor = Color.Black,
                    FontSize = fs
                });
                adress.Text = AdressIdent;
                sumPayDate.FormattedText = formattedPayDate;
                FormattedString formattedPay = new FormattedString();
                formattedPay.Spans.Add(new Span
                {
                    Text = SumPay,
                    TextColor = (Color)Application.Current.Resources["MainColor"],
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 20
                });
                formattedPay.Spans.Add(new Span
                {
                    Text = $" {AppResources.Currency}",
                    TextColor = Color.Gray,
                    FontSize = 15
                });
                sumPay.FormattedText = formattedPay;
            }
        }
        
        
    }
}