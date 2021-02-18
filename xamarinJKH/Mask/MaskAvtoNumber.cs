using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace xamarinJKH.Mask
{
    public class MaskAvtoNumber: Behavior<Entry>
    {
        private Color colorError = Color.Black;
        
        
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            nameof(ColorError),
            typeof(Color),
            typeof(MaskAvtoNumber),
            Color.Black,
            BindingMode.TwoWay
        ); 
        public Color ColorError
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        } 
        
        private bool _isChecked = false;
        
        public static readonly BindableProperty IsCheckedProperty =
            BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBox), false,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                }, defaultBindingMode: BindingMode.TwoWay);
        
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value); 
            
        }

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }
        private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            var entry = sender as Entry;

            var entryNumberText = entry.Text.Replace(" ","");

            Regex regexNumberAvto = new Regex(@"^[А-Я]{1}[0-9]{3}[А-Я]{2}[0-9]{2,3}$");
            Regex regexNumberAvto2 = new Regex(@"[А-Я]{2}[0-9]{3}[0-9]{2,3}$");
            string result = entryNumberText;
            if (!IsChecked)
            {
                if (regexNumberAvto.IsMatch(entryNumberText))
                {
                    result = entryNumberText.Insert(1, " ").Insert(5, " ").Insert(8, " ");
                    entry.Text = result.Trim();
                    entry.MaxLength = 12;
                    ColorError = Color.Black;

                }
                else if (regexNumberAvto2.IsMatch(entryNumberText))
                {
                    result = entryNumberText.Insert(2, " ").Insert(6, " ");
                    entry.Text = result.Trim();
                    entry.MaxLength = 10;
                    ColorError = Color.Black;
                }
                else
                {
                    ColorError = Color.Red;
                }
            }
            else
            {
                ColorError = Color.Black;
                entry.Text = entry.Text.Trim();
            }
            
          
        }
    }
}