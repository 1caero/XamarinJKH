using System;
using System.IO;
using Xamarin.Forms;

namespace xamarinJKH.AppsConst
{
    public class FileConstCell : ViewCell
    {
        private Label LabelName = new Label();
        private Label LabelSize = new Label();
        IconView IconViewDell = new IconView();

        public FileConstCell()
        {
            var hex = (Color)Application.Current.Resources["MainColor"];

            Frame frame = new Frame();
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.Transparent;
            frame.BorderColor = hex;
            frame.Margin = new Thickness(0, 0, 0, 5);
            frame.Padding = new Thickness(10, 10, 10, 10);
            frame.CornerRadius = 10;
            frame.HasShadow = false;


            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Horizontal;

            LabelName.FontSize = 13;
            LabelName.TextColor = Color.Black;
            LabelName.HorizontalOptions = LayoutOptions.FillAndExpand;
            LabelName.VerticalOptions = LayoutOptions.Center;

            LabelSize.FontSize = 13;
            LabelSize.TextColor = Color.Gray;
            LabelSize.HorizontalOptions = LayoutOptions.FillAndExpand;
            LabelSize.HorizontalTextAlignment = TextAlignment.End;
            LabelSize.VerticalOptions = LayoutOptions.Center;

            IconViewDell.Source = "ic_close";
            IconViewDell.Foreground = hex;
            IconViewDell.HorizontalOptions = LayoutOptions.End;
            IconViewDell.VerticalOptions = LayoutOptions.Center;
            IconViewDell.HeightRequest = 12;
            IconViewDell.WidthRequest = 12;

            container.Children.Add(LabelName);
            container.Children.Add(LabelSize);
            container.Children.Add(IconViewDell);

            frame.Content = container;

            View = frame;
        }

        public static readonly BindableProperty FileNameProperty =
            BindableProperty.Create("FileName", typeof(string), typeof(FileConstCell), "");

        public static readonly BindableProperty FileSizeProperty =
            BindableProperty.Create("FileSize", typeof(Stream), typeof(FileConstCell));

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public Stream FileSize
        {
            get { return (Stream)GetValue(FileSizeProperty); }
            set { SetValue(FileSizeProperty, value); }
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                //byte[] bytes = new byte[] { 1 };
                LabelName.Text = FileName;

                double size = FileSize.Length;
                string sizeType = AppResources.b;
                if (size >= 1024)
                {
                    size /= 1024;
                    sizeType = AppResources.kb;
                }
                if (size >= 1024)
                {
                    size /= 1024;
                    sizeType = AppResources.mb;
                }

                LabelSize.Text = Math.Round(size, 2).ToString() + " " + sizeType;

            }
        }
    }
}