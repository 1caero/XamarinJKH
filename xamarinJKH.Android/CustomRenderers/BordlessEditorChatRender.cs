using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using xamarinJKH;
using xamarinJKH.Droid.CustomRenderers;

[assembly: ExportRenderer(typeof(BordlessEditorChat), typeof(BordlessEditorChatRender))]
namespace xamarinJKH.Droid.CustomRenderers
{
    public class BordlessEditorChatRender : EditorRenderer
    {
        public static void Init() { }
        
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Background = null;

                var layoutParams = new MarginLayoutParams(Control.LayoutParameters);
                layoutParams.SetMargins(0, 0, 0, 0);
                LayoutParameters = layoutParams;
                Control.LayoutParameters = layoutParams;
                Control.SetPadding(0, 0, 0, 0);
                SetPadding(0, 0, 0, 0);
            }
        }
    }
}