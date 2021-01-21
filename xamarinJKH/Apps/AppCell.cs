using System;
using System.Collections.Generic;
using FFImageLoading.Svg.Forms;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using xamarinJKH.CustomRenderers;
using xamarinJKH.Utils;

namespace xamarinJKH.Apps
{
    public class AppCell : ViewCell
    {
        private Label numberAndDate = new Label();
        private Label LabelDate = new Label();
        SvgCachedImage ImageStatus = new SvgCachedImage();
        private Label LabelStatus = new Label();
        private Label LabelText = new Label();
        Frame ReadIndicator;

        public AppCell()
        {
            MaterialFrame frame = new MaterialFrame();
            frame.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"],
                Color.White);
            frame.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frame.SetOnAppTheme(MaterialFrame.ElevationProperty, 0, 20);
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(10, 0, 10, 14);
            frame.Padding = new Thickness(25, 15, 25, 20);
            frame.CornerRadius = 40;

            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Horizontal;

            StackLayout containerData = new StackLayout();
            containerData.HorizontalOptions = LayoutOptions.FillAndExpand;


            var arrow = new SvgCachedImage();
            arrow.Source = "resource://xamarinJKH.Resources.ic_arrow_forward.svg";
            Color hex = (Color)Application.Current.Resources["MainColor"];
            arrow.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}" } };
            arrow.HeightRequest = 25;
            arrow.WidthRequest = 25;
            arrow.VerticalOptions = LayoutOptions.CenterAndExpand;
            arrow.HorizontalOptions = LayoutOptions.End;

            numberAndDate.TextColor = Color.Black;
            numberAndDate.FontSize = 12;
            numberAndDate.VerticalOptions = LayoutOptions.StartAndExpand;
            numberAndDate.HorizontalOptions = LayoutOptions.StartAndExpand;

            LabelDate.TextColor = Color.Black;
            LabelDate.FontSize = 15;
            LabelDate.Margin = new Thickness(0, -5, 0, 0);

            StackLayout status = new StackLayout();
            status.Orientation = StackOrientation.Horizontal;
            status.HorizontalOptions = LayoutOptions.FillAndExpand;
            arrow.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}" } };
            ImageStatus.Source = "resource://xamarinJKH.Resources.ic_status_new.svg";
            ImageStatus.HeightRequest = 15;
            ImageStatus.VerticalOptions = LayoutOptions.End;
            ImageStatus.WidthRequest = 15;
            ImageStatus.Margin = new Thickness(0, 0, 0, 0);

            LabelStatus.TextColor = Color.Black;
            LabelStatus.FontSize = 15;
            LabelStatus.VerticalTextAlignment = TextAlignment.Center;
            LabelStatus.VerticalOptions = LayoutOptions.Center;



            LabelText.TextColor = Color.Black;
            LabelText.HorizontalOptions = LayoutOptions.Start;
            LabelText.FontSize = 15;
            LabelText.HorizontalTextAlignment = TextAlignment.Start;
            LabelText.VerticalOptions = LayoutOptions.Start;
            LabelText.FontAttributes = FontAttributes.Bold;

            Grid grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                }
            };


            grid.Children.Add(numberAndDate, 0, 0);

            StackLayout stackLayoutStatus = new StackLayout();
            stackLayoutStatus.Orientation = StackOrientation.Horizontal;

            stackLayoutStatus.Children.Add(ImageStatus);
            stackLayoutStatus.Children.Add(LabelStatus);

            containerData.Children.Add(grid);
            containerData.Children.Add(LabelDate);
            containerData.Children.Add(stackLayoutStatus);

            Frame readindicator = new Frame
            {
                BackgroundColor = Color.Red,
                CornerRadius = 5
            };

            Grid containerMain = new Grid();
            containerMain.Padding = 0;
            containerMain.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = new GridLength(5) }
            };

            containerMain.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(5)},
                new RowDefinition { Height = GridLength.Star }
            };

            container.Children.Add(containerData);
            container.Children.Add(arrow);

            containerMain.Children.Add(container);
            Grid.SetRowSpan(container, 2);
            Grid.SetColumnSpan(container, 2);


            ReadIndicator = new Frame
            {
                CornerRadius = 5,
                BackgroundColor = Color.Red,
                IsVisible = false
            };
            ReadIndicator.SetBinding(View.IsVisibleProperty, "Read", BindingMode.TwoWay);
                containerMain.Children.Add(ReadIndicator, 1, 0);
            frame.Content = containerMain;

            var stackLMain = new StackLayout() { Margin=new Thickness(0,2)};
            stackLMain.Children.Add(frame);
            View = stackLMain;

          
        }


        public static readonly BindableProperty NumberProperty =
            BindableProperty.Create("Number", typeof(string), typeof(AppCell), "");

        public static readonly BindableProperty StatusProperty =
            BindableProperty.Create("Status", typeof(string), typeof(AppCell), "");

        public static readonly BindableProperty DateAppProperty =
            BindableProperty.Create("DateApp", typeof(string), typeof(AppCell), "");

        public static readonly BindableProperty TextAppProperty =
            BindableProperty.Create("TextApp", typeof(string), typeof(AppCell), "");

        public static readonly BindableProperty StatusIDProperty =
            BindableProperty.Create("StatusID", typeof(int), typeof(AppCell), 0);

        public static readonly BindableProperty ReadProperty =
            BindableProperty.Create("Read", typeof(bool), typeof(AppCell));

        public static readonly BindableProperty IDProperty =
            BindableProperty.Create("ID", typeof(int), typeof(AppCell));

        public string Number
        {
            get { return (string) GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        public string Status
        {
            get { return (string) GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        } 
        public int StatusID
        {
            get { return (int) GetValue(StatusIDProperty); }
            set { SetValue(StatusIDProperty, value); }
        }

        public string DateApp
        {
            get { return (string) GetValue(DateAppProperty); }
            set { SetValue(DateAppProperty, value); }
        }

        public string TextApp
        {
            get { return (string) GetValue(TextAppProperty); }
            set { SetValue(TextAppProperty, value); }
        }

        public bool Read
        {
            get { return (bool)GetValue(ReadProperty); }
            set { SetValue(ReadProperty, value); }
        }

        public int ID
        {
            get { return (int)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                Analytics.TrackEvent("Формирование списка заявок у жителя");

                FormattedString formatted = new FormattedString();
                formatted.Spans.Add(new Span
                {
                    Text = "№" + Number + " ",
                    TextColor = Color.Black,
                    FontSize = 13
                });
                formatted.Spans.Add(new Span
                {
                    Text = "• " + TextApp.Trim(),
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 17
                });
                numberAndDate.FormattedText = formatted;
                LabelStatus.Text = Status;

                LabelDate.Text = DateApp;
                ImageStatus.ReplaceStringMap = new Dictionary<string, string>
                {
                    {"#000000",  $"#{Settings.MobileSettings.color}"}
                };
                ImageStatus.Source = "resource://xamarinJKH.Resources."+Settings.GetStatusIcon(StatusID)+".svg";
                ReadIndicator.IsVisible = !Read && StatusID != 6;

            }
        }
    }
}