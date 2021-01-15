using xamarinJKH.ViewModels;

namespace xamarinJKH.Counters
{
    public class AddMetersPageViewModel:BaseViewModel
    {
        decimal _prevCount;
        public decimal PrevCounter
        {
            get => _prevCount;
            set
            {
                _prevCount = value;
                OnPropertyChanged("PrevCount");
            }
        }

        public AddMetersPageViewModel(decimal value)
        {
            PrevCounter = value;
        }
    }
}
