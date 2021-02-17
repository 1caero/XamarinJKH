using System;
using System.Collections.Generic;
using System.IO;
using FFImageLoading.Forms;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using xamarinJKH.Server;

namespace xamarinJKH.Additional
{
    public class AdditionalCell : ViewCell
    {
        Image image;
        CachedImage CachedImage;

        PancakeView frame;
        RestClientMP _server = new RestClientMP();

        public AdditionalCell()
        {
            image = new Image();
            CachedImage = new CachedImage();
            CachedImage.VerticalOptions = LayoutOptions.FillAndExpand;
            CachedImage.HorizontalOptions = LayoutOptions.FillAndExpand;
            CachedImage.Aspect = Aspect.Fill;
            CachedImage.HeightRequest = ImageHeight;


            frame = new PancakeView(); 
            
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.IsClippedToBounds = true;
            frame.Margin = new Thickness(10, 0, 10, 10);
            frame.Padding = new Thickness(0);
            frame.CornerRadius = 40;
            frame.Content = CachedImage;// image;

            View = frame;
        }

        public static readonly BindableProperty ImagePathProperty =
            BindableProperty.Create("ImagePath", typeof(ImageSource), typeof(AdditionalCell), null);

        public static readonly BindableProperty ImageWidthProperty =
            BindableProperty.Create("ImageWidth", typeof(int), typeof(AdditionalCell), 100);

        public static readonly BindableProperty ImageHeightProperty =
            BindableProperty.Create("ImageHeight", typeof(int), typeof(AdditionalCell), 100);

        public static readonly BindableProperty DetailProperty =
            BindableProperty.Create("Detail", typeof(string), typeof(AdditionalCell), "");

        public static readonly BindableProperty LogoFileIdProperty =
            BindableProperty.Create("LogoFileId", typeof(string), typeof(AdditionalCell), "");

        public int ImageWidth
        {
            get { return (int) GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public int ImageHeight
        {
            get { return (int) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public String ImagePath
        {
            get { return (String) GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        public string Detail
        {
            get { return (string) GetValue(DetailProperty); }
            set { SetValue(DetailProperty, value); }
        }

        public string LogoFileId
        {
            get { return (string) GetValue(LogoFileIdProperty); }
            set { SetValue(LogoFileIdProperty, value); }
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            CachedImage.Source = RestClientMP.SERVER_ADDR + $"/AdditionalServices/logo/{Detail}";
            CachedImage.HeightRequest = ImageHeight;
           
            return;
        }
    }
}