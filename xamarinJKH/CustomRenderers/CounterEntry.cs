using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace xamarinJKH.CustomRenderers
{
    public class CounterEntry:Entry
    {

        bool DecimalPart;
        int Changed;

        public static BindableProperty IsEditing = BindableProperty.Create("IsEditing", typeof(bool), typeof(bool), false);
        public bool Editing
        {
            get => (bool)GetValue(IsEditing);
            set => SetValue(IsEditing, value);
        }
        public CounterEntry() : base()
        {
            this.CursorPosition = 5;
            this.Focus();
            this.TextChanged += (sender, e) =>
            {
                try
                {
                    
                    var val = e.NewTextValue.Replace('.', ',');
                    
                    if (e.NewTextValue != e.OldTextValue)
                    {
                        if (val[e.NewTextValue.Length - 1] == ',')
                        {
                            this.Text = String.Format("{0:00000.###}", double.Parse(val)).Replace('.', ',');

                        }
                        var debug = val.Split(',');
                        if ((string.IsNullOrEmpty(e.OldTextValue) && double.Parse(val) > 0 && Changed < 2 && Editing) || debug.Length > 2)
                        {
                            Changed++;
                            try
                            {
                                decimal value = decimal.Parse(val.Replace(',', '.'));
                                value += 0.0001M;
                                this.Text = String.Format("{0:00000.000}", value).Replace('.', ',');
                                this.CursorPosition = 9;
                            }
                            catch
                            {
                                decimal value = decimal.Parse($"{debug[0]}.{debug[1]}{debug[2]}");
                                value += 0.0001M;
                                this.Text = String.Format("{0:00000.000}", value).Replace('.', ',');
                                this.CursorPosition = 9;
                            }
                        }
                    }
                    
                }
                catch
                {

                }
            };
        }

        public static int GetDecimalPlaces(decimal n)
        {
            n = Math.Abs(n); //make sure it is positive.
            n -= (int)n;     //remove the integer part of the number.
            var decimalPlaces = 0;
            while (n > 0)
            {
                decimalPlaces++;
                n *= 10;
                n -= (int)n;
            }
            return decimalPlaces;
        }
    }
}
