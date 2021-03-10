using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using xamarinJKH.Droid.CustomReader;

[assembly: ExportRenderer(typeof(Button), typeof(CustomButtonRender))]
namespace xamarinJKH.Droid.CustomReader
{
   
    public class CustomButtonRender : ButtonRenderer
    {
        public CustomButtonRender(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.SetAllCaps(false);
            }
        }
    }
}