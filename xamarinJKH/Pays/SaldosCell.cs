using System;
using System.Collections.Generic;
using System.Globalization;
using FFImageLoading.Svg.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.Utils;

namespace xamarinJKH.Pays
{
    public class SaldosCell : ViewCell
    {
        private Label identDate = new Label();
        private Label sum = new Label();
        private SvgCachedImage file = new SvgCachedImage();

        private Label accuralSum = new Label();

        public SaldosCell()
        {
            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Horizontal;
            container.Margin = new Thickness(0, 0, 0, 10);
            container.Spacing = 0;
            identDate.HorizontalOptions = LayoutOptions.FillAndExpand;
            sum.HorizontalOptions = LayoutOptions.End;
            sum.HorizontalTextAlignment = TextAlignment.End;
            accuralSum.HorizontalOptions = LayoutOptions.End;
            accuralSum.HorizontalTextAlignment = TextAlignment.End;
            accuralSum.Margin = new Thickness(10, 0);
            file.HorizontalOptions = LayoutOptions.End;
            file.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", $"#{Settings.MobileSettings.color}"}
            }; 
            file.HeightRequest = 40;
            file.Margin = new Thickness(10, 0, 0, 0);
            file.VerticalOptions = LayoutOptions.End;
            file.Source = "resource://xamarinJKH.Resources.ic_file.svg";
            container.Children.Add(identDate);
            container.Children.Add(sum);
            container.Children.Add(accuralSum);
            container.Children.Add(file);

            View = container;
        }


        public static readonly BindableProperty IdentProperty =
            BindableProperty.Create("Ident", typeof(string), typeof(SaldosCell), "");

        public static readonly BindableProperty HasImageProperty =
            BindableProperty.Create("HasImage", typeof(bool), typeof(SaldosCell), false);

        public static readonly BindableProperty DateIdentProperty =
            BindableProperty.Create("DateIdent", typeof(string), typeof(SaldosCell), "");

        public static readonly BindableProperty SumPayProperty =
            BindableProperty.Create("SumPay", typeof(string), typeof(SaldosCell), "");

        public static readonly BindableProperty AccuralProperty =
            BindableProperty.Create("Accural", typeof(string), typeof(SaldosCell), "");
        

        public string Ident
        {
            get { return (string) GetValue(IdentProperty); }
            set { SetValue(IdentProperty, value); }
        }

        public bool HasImage
        {
            get { return (bool) GetValue(HasImageProperty); }
            set { SetValue(HasImageProperty, value); }
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

        public string Accural
        {
            get { return (string)GetValue(AccuralProperty); }
            set { SetValue(AccuralProperty, value); }
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                var fs = 15;
                if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    fs = 12;

                FormattedString formattedIdent = new FormattedString();
                DateIdent = FirstLetterToUpper(DateIdent);

                DateTime dtView;
                string spanTextField;
                var dateCorrect = DateTime.TryParseExact(DateIdent.Replace("г.", " ").Trim(), "MMMM yyyy", new CultureInfo("ru-RU"), DateTimeStyles.None , out dtView);
                if (dateCorrect)
                {
                    spanTextField = dtView.ToString("MMMM yyyy") + (CultureInfo.CurrentCulture.Name.Contains("en") ? string.Empty : " г.");
                }
                else
                    spanTextField = DateIdent;
                formattedIdent.Spans.Add(new Span
                {
                   
                    Text=spanTextField,
                    TextColor = Color.Black,
                    FontSize = fs
                });
                formattedIdent.Spans.Add(new Span
                {
                    Text = $"\n{AppResources.Acc} ",
                    TextColor = Color.Gray,
                    FontSize = fs
                });
                formattedIdent.Spans.Add(new Span
                {
                    Text = " " + Ident,
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = fs
                });
                identDate.FormattedText = formattedIdent;

                formattedIdent = new FormattedString();

                double sum2;
                var parseSumpayOk = Double.TryParse(SumPay, NumberStyles.Float, new CultureInfo("ru-RU"), out sum2);
                if(parseSumpayOk)
                {
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"{sum2:0.00}".Replace(',', '.'),
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = fs
                    });
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"\n{AppResources.Currency}",
                        TextColor = Color.Gray,
                        FontSize = fs - 2
                    });

                }
                else
                {
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"{SumPay}".Replace(',', '.'),
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = fs
                    });
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"\n{AppResources.Currency}",
                        TextColor = Color.Gray,
                        FontSize = fs - 2
                    });
                }

                sum.FormattedText = formattedIdent;

                #region Accural
                var formattedAccural = new FormattedString();
                double sumAccural;
                var parsesumAccuralOk = Double.TryParse(Accural, NumberStyles.Float, new CultureInfo("ru-RU"), out sumAccural);
                if (parsesumAccuralOk)
                {
                    formattedAccural.Spans.Add(new Span
                    {
                        Text = $"{sumAccural:0.00}".Replace(',', '.'),
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = fs
                    });
                    formattedAccural.Spans.Add(new Span
                    {
                        Text = $"\n{AppResources.Currency}",
                        TextColor = Color.Gray,
                        FontSize = fs - 2
                    });

                }
                else
                {
                    formattedAccural.Spans.Add(new Span
                    {
                        Text = $"{Accural}".Replace(',', '.'),
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = fs
                    });
                    formattedAccural.Spans.Add(new Span
                    {
                        Text = $"\n{AppResources.Currency}",
                        TextColor = Color.Gray,
                        FontSize = fs - 2
                    });
                }

                accuralSum.FormattedText = formattedAccural;
                #endregion

                if (!HasImage)
                {
                    file.IsVisible = false;
                }


            }
        }

        public static string FirstLetterToUpper(string str)
        {
            if (!string.IsNullOrWhiteSpace(str) && str.Length > 0)
            {
                return Char.ToUpper(str[0]) + str.Substring(1);
            }

            return "";
        }
    }
}