using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using xamarinJKH;
using xamarinJKH.iOS.CustomRenderers;

[assembly: ExportRenderer(typeof(BorderlessDatePickerMonitor), typeof(BorderlessDatePickerMonitorRenderer))]
namespace xamarinJKH.iOS.CustomRenderers
{
    public class BorderlessDatePickerMonitorRenderer : DatePickerRenderer
    {
        public static void Init() { }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (Control != null)
            {
                Control.Layer.BorderWidth = 0;
                Control.BorderStyle = UITextBorderStyle.None;
            }
        }
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && this.Control != null)
            {
                try
                {
                    //if (UIDevice.CurrentDevice.CheckSystemVersion(13, 2))
                    //{
                    //    UIDatePicker picker = (UIDatePicker)Control.InputView;
                    //    picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
                    //}
                    //else
                    //{
                        UIDatePicker picker = (UIDatePicker)Control.InputView;
                        picker.PreferredDatePickerStyle = UIDatePickerStyle.Compact;
                    //}
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }
    }
}