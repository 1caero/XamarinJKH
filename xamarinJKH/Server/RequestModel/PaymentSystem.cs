using System;
using Xamarin.Forms;

namespace xamarinJKH.Server.RequestModel
{
    public class PaymentSystem
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Check { get; set; }
        public string ImageLink => RestClientMP.SERVER_ADDR + "/" + RestClientMP.GET_PAYMENT_IMAGE + "/" + Name;

        public UriImageSource UriImageSource
        {
            get => new UriImageSource
            {
                Uri = new Uri(ImageLink),
                CachingEnabled = true,
                CacheValidity = new TimeSpan(5,0,0,0)
            };
        }
    }
}