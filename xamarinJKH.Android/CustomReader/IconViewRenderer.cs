using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using xamarinJKH;
using xamarinJKH.Droid.CustomReader;
using Exception = Java.Lang.Exception;

[assembly: ExportRenderer(typeof(IconView), typeof(IconViewRenderer))]

namespace xamarinJKH.Droid.CustomReader
{
    public class IconViewRenderer : ViewRenderer<IconView, ImageView>
    {
        private bool _isDisposed;

        public IconViewRenderer(Context context):base(context)
        {
            base.AutoPackage = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<IconView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                SetNativeControl(new ImageView(Context));
            }
            UpdateBitmap(e.OldElement);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == IconView.SourceProperty.PropertyName)
            {
                UpdateBitmap(null);
            }
            else if (e.PropertyName == IconView.ForegroundProperty.PropertyName)
            {
                UpdateBitmap(null);
            }
        }

        private void UpdateBitmap(IconView previous = null)
        {
          
            if (!_isDisposed && !string.IsNullOrWhiteSpace(Element.Source))
            {
                try
                {
                    Analytics.TrackEvent($"Установка цвета {Element.Foreground.ToString()} картинки {Element.Source}");
                    var d = Context.GetDrawable(Element.Source).Mutate();
                    d.SetTint(Element.Foreground.ToAndroid());
                    d.Alpha = Element.Foreground.ToAndroid().A;
                    Control.SetImageDrawable(d);
                    ((IVisualElementController)Element).NativeSizeChanged();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}