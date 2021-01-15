using Xamarin.Forms;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    public class BoxForCounter:BoxView
    {
        public Color CellColor => Color.FromHex(Settings.MobileSettings.color);

        public BoxForCounter()
        {
            Color = CellColor;  
            Margin = new Thickness(0);  
            Grid.SetRow(this,1);
        }
    }
}
