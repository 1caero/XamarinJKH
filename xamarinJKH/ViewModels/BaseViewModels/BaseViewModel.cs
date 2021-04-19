using System.ComponentModel;
using System.Runtime.CompilerServices;
using AiForms.Dialogs;
using Xamarin.Forms;
using xamarinJKH.Server;
using xamarinJKH.Utils;

namespace xamarinJKH.ViewModels
{
    public class BaseViewModel:INotifyPropertyChanged
    {
        bool _isLoading;
        public bool IsLoading
        {
            set
            {
                _isLoading = value;
                if (_isLoading)
                {
                    Loading.Instance.Show(LoadingMessage);
                }
                else
                {
                    Loading.Instance.Hide();
                }
            }
        }
        public string LoadingMessage { get; set; }
        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }
        private bool _isChangeTheme;

        public bool IsChangeTheme
        {
            get => _isChangeTheme;
            set
            {
                _isChangeTheme = value;
                OnPropertyChanged(nameof(IsChangeTheme));
            }
        }
        string title;
        public string Title
        {
            get => Settings.MobileSettings.main_name;// title;
        }
        string phone;
        public string Phone => Settings.Person.companyPhone;
        public event PropertyChangedEventHandler PropertyChanged;
        public RestClientMP Server => DependencyService.Get<RestClientMP>(DependencyFetchTarget.NewInstance);
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void ShowError(string error, string title = "Ошибка")
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(title, error, "OK");
            });
        }
    }
}
