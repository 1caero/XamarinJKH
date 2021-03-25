using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using xamarinJKH;
using xamarinJKH.iOS.CustomRenderers;

[assembly: ExportRenderer(typeof(JustifiedLabel), typeof(JustifiedLabelRenderer))]
namespace xamarinJKH.iOS.CustomRenderers
{
    public class JustifiedLabelRenderer : LabelRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            //if we have a new forms element, we want to update text with font style (as specified in forms-pcl) on native control
            if (e.NewElement != null)
                UpdateTextOnControl();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //if there is change in text or font-style, trigger update to redraw control
            if (e.PropertyName == nameof(Label.Text)
               || e.PropertyName == nameof(Label.FontFamily)
               || e.PropertyName == nameof(Label.FontSize)
               || e.PropertyName == nameof(Label.TextColor)
               || e.PropertyName == nameof(Label.FontAttributes))
            {
                UpdateTextOnControl();
            }
        }

        void UpdateTextOnControl()
        {
            if (Control == null)
                return;

            //define paragraph-style
            var style = new NSMutableParagraphStyle()
            {
                Alignment = UITextAlignment.Justified,
                FirstLineHeadIndent = 0.001f,
            };

            //define attributes that use both paragraph-style, and font-style 
            var uiAttr = new UIStringAttributes()
            {
                ParagraphStyle = style,
                BaselineOffset = 0,

                Font = Control.Font
            };

            //define frame to ensure justify alignment is applied
            Control.Frame = new RectangleF(0, 0, (float)Element.Width, (float)Element.Height);

            //set new text with ui-style-attributes to native control (UILabel)
            var stringToJustify = Control.Text ?? string.Empty;
            var attributedString = new Foundation.NSAttributedString(stringToJustify, uiAttr.Dictionary);
            Control.AttributedText = attributedString;
            Control.Lines = 0;
        }
    }
}