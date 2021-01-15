using System;
using System.Globalization;
using Xamarin.Forms;

namespace xamarinJKH.Pays
{
    public class HistoryPaysCell : ViewCell
    {
        Label LabelDate = new Label();
        Label LabelPeriod = new Label();
        Label LabelSum = new Label();
        
        public HistoryPaysCell()
        {
            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Horizontal;
            container.HorizontalOptions = LayoutOptions.FillAndExpand;
            
            Grid grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };
            
            
            LabelDate.TextColor = Color.Black;
            LabelDate.HorizontalOptions = LayoutOptions.FillAndExpand;
            LabelDate.FontSize = 15;
            LabelDate.HorizontalTextAlignment = TextAlignment.Start;
            
            LabelPeriod.TextColor = Color.Black;
            LabelPeriod.HorizontalOptions = LayoutOptions.Fill;
            LabelPeriod.FontSize = 15;
            LabelPeriod.FontAttributes = FontAttributes.Bold;
            LabelPeriod.HorizontalTextAlignment = TextAlignment.Start;
            
            LabelSum.TextColor = Color.Black;
            LabelSum.HorizontalOptions = LayoutOptions.EndAndExpand;
            LabelSum.FontSize = 15;
            LabelSum.HorizontalTextAlignment = TextAlignment.Start;
            
            container.Children.Add(LabelDate);
            container.Children.Add(LabelPeriod);
            container.Children.Add(LabelSum);

            grid.Children.Add(LabelDate, 0, 0);
            grid.Children.Add(LabelPeriod, 1, 0);
            grid.Children.Add(LabelSum, 2, 0);
            
            View = grid;

        }

        public static readonly BindableProperty PeriodProperty =
            BindableProperty.Create("Period", typeof(string), typeof(HistoryPaysCell), "");
        
        public static readonly BindableProperty DatePayProperty =
            BindableProperty.Create("DatePay", typeof(string), typeof(HistoryPaysCell), "");

        public static readonly BindableProperty SumPayProperty =
            BindableProperty.Create("SumPay", typeof(string), typeof(HistoryPaysCell), "");

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
                
                FormattedString formattedIdent = new FormattedString();
                double sum2;
                var parseSumpayOk = Double.TryParse(SumPay, NumberStyles.Float, new CultureInfo("ru-RU"), out sum2);
                if (parseSumpayOk)
                {
                    formattedIdent.Spans.Add(new Span
                    {
                        Text = $"{sum2:0.00}".Replace(',', '.'),
                        TextColor = (Color)Application.Current.Resources["MainColor"],
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
                        TextColor = (Color)Application.Current.Resources["MainColor"],
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