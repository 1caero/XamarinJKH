using System;
using AiForms.Dialogs.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddAccountDialogView : DialogView
    {
        public AddAccountDialogView()
        {
            InitializeComponent();
            BindingContext = new AddAccountDialogViewModel(this);
        }

        public void CloseDialog()
        {
            DialogNotifier.Cancel();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            CloseDialog();
        }
    }

    public class AddAccountDialogViewModel : BaseViewModel
    {
        private AddAccountDialogView accountDialogView;
        public AddAccountDialogViewModel(AddAccountDialogView accountDialogView)
        {
            this.accountDialogView = accountDialogView;
        }

        public Color ButtonColor
        {
            get => Color.FromHex(Settings.MobileSettings.color);
        }
        bool _progress;
        public bool Progress 
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }
        bool _btn;
        public bool Button
        {
            get => _btn;
            set
            {
                _btn = value;
                OnPropertyChanged("Button");
            }
        }

        public RestClientMP _server => new RestClientMP();
        readonly INavigation Navigation;

        public Command AddIdent
        {
            get => new Command<string>(async (ident) =>
            {
                if (ident != "")
                {
                    Progress = true;
                    Button = false;
                    Progress = true;
                    AddAccountResult result = await _server.AddIdent(ident, true);
                    if (result.Error == null)
                    {
                        Console.WriteLine(result.Address);
                        Console.WriteLine("Отправлено");
                        bool answer = await Application.Current.MainPage.DisplayAlert(AppResources.ErrorCheckAdress, result.Address, AppResources.AddIdent, AppResources.Cancel);
                        if (answer)
                        {
                           
                            AcceptAddIdentAccount(ident, Settings.Person.Email);
                        }
                        else
                        {
                            Button = true;
                            Progress = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("---ОШИБКА---");
                        Console.WriteLine(result.ToString());
                        Button = true;
                        Progress = false;
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            await Application.Current.MainPage.DisplayAlert("ОШИБКА", result.Error, "OK");
                        }
                        else
                        {
                            DependencyService.Get<IMessage>().ShortAlert(result.Error);
                        }
                    }

                    Progress = false;
                    Button = true;
                }
                else
                {
                    if (ident == "")
                    {
                        await Application.Current.MainPage.DisplayAlert("", AppResources.ErrorFillIdent, "OK");
                    }
                }
            });
        }
        public async void AcceptAddIdentAccount(string ident, string email)
        {
            if (ident != "")
            {
                Progress = true;
                Button = false;
                Progress = true;
                AddAccountResult result = await _server.AddIdent(ident);
                if (result.Error == null)
                {
                    Console.WriteLine(result.Address);
                    Console.WriteLine("Отправлено");
                    await Application.Current.MainPage.DisplayAlert("", AppResources.IdentSuccess.Replace("ident",ident.ToString()), "ОК");
                    Button = true;
                    Progress = false;

                    accountDialogView.CloseDialog();
                    string login = Preferences.Get("login", "");
                    string pass = Preferences.Get("pass", "");
                    if (!pass.Equals("") && !login.Equals(""))
                    {
                        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                        {
                            Device.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                            return;
                        }
                        LoginResult loginResult = await _server.Login(login, pass);
                        if (loginResult.Error == null)
                        {
                            Settings.Person = loginResult;
                        }
                    }
                    MessagingCenter.Send<Object>(this, "UpdateIdent");
                    MessagingCenter.Send<Object>(this, "ChangeThemeCounter");
                    MessagingCenter.Send<Object>(this, "UpdateEvents");
                    //_paysPage.RefreshPaysData();
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    Button = true;
                    Progress = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await Application.Current.MainPage.DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                Progress = false;
                Button = true;
            }
            else
            {
                if (ident == "")
                {
                    await Application.Current.MainPage.DisplayAlert("", AppResources.ErrorFillIdent, "OK");
                }
            }
        }
    }

}