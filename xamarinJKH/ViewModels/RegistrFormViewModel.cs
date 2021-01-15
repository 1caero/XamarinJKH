using System;
using Xamarin.Forms;

namespace xamarinJKH.ViewModels
{
    public class RegistrFormViewModel:BaseViewModel
    {
        readonly INavigation Navigation;
        public Command Register { get; set; }
        public RegistrFormViewModel(INavigation navigation)
        {
            Navigation = navigation;
            Register = new Command<Tuple<string, string>>(async (data) =>
            {

            });
        }
    }
}
