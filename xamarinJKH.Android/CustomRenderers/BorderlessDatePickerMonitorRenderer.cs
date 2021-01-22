//using Xamarin.Forms.Platform.Android;
//using Xamarin.Forms;
//using DatePicker = Xamarin.Forms.DatePicker;
//using xamarinJKH.Droid.CustomRenderers;
//using xamarinJKH;

//[assembly: ExportRenderer(typeof(BorderlessDatePickerMonitor), typeof(BorderlessDatePickerMonitorRenderer))]
//namespace xamarinJKH.Droid.CustomRenderers
//{
//    public class BorderlessDatePickerMonitorRenderer : DatePickerRenderer
//    {
//        public static void Init() { }
//        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
//        {
//            base.OnElementChanged(e);
//            if (e.OldElement == null)
//            {
//                Control.Background = null;

//                var layoutParams = new MarginLayoutParams(Control.LayoutParameters);
//                layoutParams.SetMargins(0, 0, 0, 0);
//                LayoutParameters = layoutParams;
//                Control.LayoutParameters = layoutParams;
//                Control.SetPadding(0, 0, 0, 0);
//                SetPadding(0, 0, 0, 0);
//            }
//        }
//    }
//}