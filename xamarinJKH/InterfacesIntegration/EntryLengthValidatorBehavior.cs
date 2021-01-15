using Xamarin.Forms;

namespace xamarinJKH.InterfacesIntegration
{
    public class EntryLengthValidatorBehavior : Behavior<CounterEntryNew>
    {
        public int MaxLength { get; set; }

        protected override void OnAttachedTo(CounterEntryNew bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnEntryTextChanged;
        }

        protected override void OnDetachingFrom(CounterEntryNew bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnEntryTextChanged;
        }

        void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (CounterEntryNew)sender;

            // if Entry text is longer then valid length
            if (entry.Text?.Length > this.MaxLength)
            {
                string entryText = entry.Text;

                entryText = entryText.Remove(entryText.Length - 1); // remove last char

                entry.Text = entryText;
            }
        }
    }
}
