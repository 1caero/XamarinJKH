using System;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Main;
using xamarinJKH.MainConst;
using xamarinJKH.Utils;

using Xamarin.Essentials;



namespace xamarinJKH.ViewModels
{
    public class MainPageViewModel:BaseViewModel
    {
        public Command Login { get; set; }
        readonly INavigation Navigation;
        public MainPageViewModel(INavigation _navigation)
        {
            Navigation = _navigation;
            Login = new Command<Tuple<string, string, bool>>(async (data) =>
            {
                IsBusy = true;
                var isDispatcher = data.Item3;
                var replace = data.Item1.Replace("+", "")
                .Replace(" ", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "");
                var pass = data.Item2;
                if (!string.IsNullOrEmpty(replace) && !string.IsNullOrEmpty(pass))
                {
                    if (replace.Length < 11)
                    {
                        this.ShowError("Номер телефона необходимо ввести в формате: +7 (ХХХ) ХХХ-ХХХХ");
                        IsBusy = false;
                        return;
                    }

                    LoginResult login = isDispatcher ? await Server.LoginDispatcher(replace, pass) : await Server.Login(replace, pass);
                    if (login.Error == null)
                    {
                        Settings.Person = login;
                        ItemsList<RequestType> result = isDispatcher ? await Server.GetRequestsTypesConst() : await Server.GetRequestsTypes();
                        Settings.TypeApp = result.Data;
                        Preferences.Set(isDispatcher ? "loginConst" : "login", replace);
                        Preferences.Set(isDispatcher ? "passConst" : "pass", pass);
                        Preferences.Set("constAuth", isDispatcher);

                        if (isDispatcher)
                            await Navigation.PushModalAsync(new BottomNavigationConstPage());
                        else
                            await Navigation.PushModalAsync(new BottomNavigationPage());
                    }
                    else
                    {
                        if (login.Error.ToLower().Contains("unauthorized"))
                        {
                            this.ShowError("Пользователь не найден");
                        }
                        else
                        {
                            this.ShowError(login.Error);    
                        }
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Заполните пустые поля", "OK");
                }
                IsBusy = false;


            });
        }

        
    }
}
