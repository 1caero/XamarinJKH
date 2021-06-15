
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using xamarinJKH;
using xamarinJKH.iOS.CustomRenderers;
using xamarinJKH.Tech;

[assembly: ExportRenderer(typeof(BordlessEditorChat), typeof(BordlessEditorChatRender))]
namespace xamarinJKH.iOS.CustomRenderers
{
    public class BordlessEditorChatRender : EditorRenderer
    {
        public static void Init() { }
       

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            var element = this.Element as BordlessEditor;
            if (Control != null)
            {
                Control.InputAccessoryView = null;
                Control.ShouldEndEditing += DisableHidingKeyboard;

                MessagingCenter.Unsubscribe<object>(this, "SetKeyboardFocusStatic");
                MessagingCenter.Subscribe<object>(this, "SetKeyboardFocusStatic", (sender) =>
                {

                    if (Control != null)
                    {
                        Control.ShouldEndEditing += DisableHidingKeyboard;
                    }

                    //MessagingCenter.Unsubscribe<object>(this, "SetKeyboardFocusStatic");
                });


                MessagingCenter.Unsubscribe<object>(this, "FocusKeyboardStatus");
                MessagingCenter.Subscribe<object>(this, "FocusKeyboardStatus", (sender) =>
                {

                    if (Control != null)
                    {
                        Control.ShouldEndEditing += EnableHidingKeyboard;
                    }

                    //MessagingCenter.Unsubscribe<object>(this, "FocusKeyboardStatus");
                });
            }                
        }

        private bool DisableHidingKeyboard(UITextView textView)
        {
            return false;
        }

        private bool EnableHidingKeyboard(UITextView textView)
        {
            return true;
        }
    }
}