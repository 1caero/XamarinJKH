using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using AiForms.Dialogs;
using FFImageLoading.Svg.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Utils;

namespace xamarinJKH.Pays
{
    public class HistoryPaysCell : ViewCell
    {
        Label LabelDate = new Label();
        Label LabelPeriod = new Label();
        Label LabelSum = new Label();
        private SvgCachedImage file = new SvgCachedImage();
        private ActivityIndicator _indicator;
        public HistoryPaysCell()
        {
            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Horizontal;
            container.HorizontalOptions = LayoutOptions.FillAndExpand;

            Grid grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(1.2, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)}
                },
                RowSpacing = 10

            };


            LabelDate.TextColor = Color.Black;
            LabelDate.HorizontalOptions = LayoutOptions.FillAndExpand;
            LabelDate.FontSize = 15;
            LabelDate.VerticalOptions = LayoutOptions.Center;
            LabelDate.HorizontalTextAlignment = TextAlignment.Start;

            LabelPeriod.TextColor = Color.Black;
            LabelPeriod.HorizontalOptions = LayoutOptions.Fill;
            LabelPeriod.FontSize = 15;
            LabelPeriod.VerticalOptions = LayoutOptions.Center;
            LabelPeriod.FontAttributes = FontAttributes.Bold;
            LabelPeriod.HorizontalTextAlignment = TextAlignment.Start;

            LabelSum.TextColor = Color.Black;
            LabelSum.HorizontalOptions = LayoutOptions.EndAndExpand;
            LabelSum.FontSize = 15;
            LabelSum.VerticalOptions = LayoutOptions.Center;
            LabelSum.HorizontalTextAlignment = TextAlignment.Start;

            container.Children.Add(LabelDate);
            container.Children.Add(LabelPeriod);
            container.Children.Add(LabelSum);


            StackLayout stackLayoutFile = new StackLayout
            {
                Padding = 5,
                VerticalOptions = LayoutOptions.Center
            };
            file.HorizontalOptions = LayoutOptions.End;
           
            file.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", $"#FFFFFF"}
            };
            file.HeightRequest = 25;
            file.Margin = new Thickness(0, 0, 0, 0);
            file.VerticalOptions = LayoutOptions.Center;
            file.Source = "resource://xamarinJKH.Resources.ic_file.svg";

            _indicator = new ActivityIndicator
            {
                HeightRequest = 25,
                WidthRequest = 25,
                Color = (Color) Application.Current.Resources["MainColor"],
                VerticalOptions = LayoutOptions.Center,
                IsRunning = true,
                IsVisible = false
            };
            
            stackLayoutFile.Children.Add(file);
            stackLayoutFile.Children.Add(_indicator);
            
            grid.Children.Add(LabelDate, 0, 0);
            grid.Children.Add(LabelPeriod, 1, 0);
            grid.Children.Add(LabelSum, 2, 0);
            grid.Children.Add(stackLayoutFile, 3, 0);
        
            View = grid;
        }

        public static readonly BindableProperty PeriodProperty =
            BindableProperty.Create("Period", typeof(string), typeof(HistoryPaysCell), "");

        public static readonly BindableProperty DatePayProperty =
            BindableProperty.Create("DatePay", typeof(string), typeof(HistoryPaysCell), "");

        public static readonly BindableProperty SumPayProperty =
            BindableProperty.Create("SumPay", typeof(string), typeof(HistoryPaysCell), "");
        
        public static readonly BindableProperty IdPayProperty =
            BindableProperty.Create("IdPay", typeof(int), typeof(HistoryPaysCell), 0);
        
        public static readonly BindableProperty HasCheckProperty =
            BindableProperty.Create("HasCheck", typeof(bool), typeof(HistoryPaysCell), false);

        public bool HasCheck
        {
            get { return (bool) GetValue(HasCheckProperty); }
            set { SetValue(HasCheckProperty, value); }
        }
        public int IdPay
        {
            get { return (int) GetValue(IdPayProperty); }
            set { SetValue(IdPayProperty, value); }
        }
        
        public string Period
        {
            get { return (string) GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }


        public string DatePay
        {
            get { return (string) GetValue(DatePayProperty); }
            set { SetValue(DatePayProperty, value); }
        }

        public string SumPay
        {
            get { return (string) GetValue(SumPayProperty); }
            set { SetValue(SumPayProperty, value); }
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                LabelDate.Text = DatePay;
                string str = Period;
                FontAttributes fontAttributes = FontAttributes.Bold;
                double fontsize = 15;
                if (Period.Equals(""))
                {
                    str = "Обрабатывается";
                    LabelPeriod.FontAttributes = FontAttributes.None;
                    LabelPeriod.FontSize = 13;
                }

                LabelPeriod.Text = str;

                if (HasCheck)
                {
                    file.ReplaceStringMap = new Dictionary<string, string>
                    {
                        {"#000000", $"#{Settings.MobileSettings.color}"}
                    };
                    
                    var openCheck = new TapGestureRecognizer();
                    openCheck.Tapped += async (s, e) =>
                    {
                        try
                        {
                            Device.BeginInvokeOnMainThread((async () =>
                            {
                                
                                // RestClientMP server = new RestClientMP();
                                // byte[] checkPp = await server.GetCheckPP(IdPay.ToString());
                                string link =
                                    $"{RestClientMP.SERVER_ADDR}/Accounting/Check/{IdPay}?acx={Uri.EscapeDataString(Settings.Person.acx ?? string.Empty)}"; 
                                // string fileName = $"check {Period} {IdPay}.png";
                                // if (checkPp != null)
                                // {
                                //     await DependencyService.Get<IFileWorker>().SaveTextAsync(fileName, checkPp);
                                //     await Launcher.OpenAsync(new OpenFileRequest
                                //     {
                                //         File = new ReadOnlyFile(DependencyService.Get<IFileWorker>()
                                //             .GetFilePath(fileName))
                                //     });
                                // }
                                // else
                                // {
                                //     Toast.Instance.Show<ToastDialog>(new {Title = AppResources.ErrorFileLoading, Duration = 1500});
                                // }
                                // string link = RestClientMP.SERVER_ADDR + "/" +
                                //               $"Accounting/Check/{IdPay}?acx={Settings.Person.acx}";
                                await Launcher.OpenAsync(link);
                             
                            }));
                           
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Toast.Instance.Show<ToastDialog>(new {Title = AppResources.ErrorAdditionalLink, Duration = 1500, ColorB = Color.Gray,  ColorT = Color.White});
                        }
                    };
                    file.GestureRecognizers.Add(openCheck);
                }
                
                FormattedString formattedIdent = new FormattedString();
                double sum2;
                var parseSumpayOk = Double.TryParse(SumPay, NumberStyles.Float, new CultureInfo("ru-RU"), out sum2);
                if (parseSumpayOk)
                {
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"{sum2:0.00}".Replace(',', '.'),
                        TextColor = (Color) Application.Current.Resources["MainColor"],
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 15
                    });
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $" {AppResources.Currency}",
                        TextColor = Color.Gray,
                        FontSize = 10
                    });
                }
                else
                {
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"{SumPay}".Replace(',', '.'),
                        TextColor = (Color) Application.Current.Resources["MainColor"],
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 15
                    });
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $" {AppResources.Currency}",
                        TextColor = Color.Gray,
                        FontSize = 10
                    });
                }

                LabelSum.FormattedText = formattedIdent;
            }
        }
    }
}