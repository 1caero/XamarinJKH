using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace xamarinJKH
{
    public class CounterEntryNew: Entry
    {
        public CounterEntryNew()
        {
            FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Entry));
            FontAttributes = FontAttributes.Bold;
            MaxLength = 1;
            if (Device.RuntimePlatform == Device.iOS)
            {
                HorizontalTextAlignment = TextAlignment.Start;
                Margin = new Thickness(0, 0, -15, -10);
            }
            else
            {
                HorizontalTextAlignment = TextAlignment.Center;
                Margin = new Thickness(0, 0, 0, -10);
            }
            
            
            Keyboard = Keyboard.Numeric;
            //Margin = new Thickness(0, 0, 0, -10);
            Grid.SetRow(this, 0);
        }

        public delegate void BackspaceEventHandler(object sender, EventArgs e);

        public event BackspaceEventHandler OnBackspace;
        public void OnBackspacePressed()
        {
            OnBackspace?.Invoke(null, null);
        }
    }
}
