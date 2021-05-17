using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.PushNotification
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddRuleDialog : DialogView
    {
        public AddRuleDialog()
        {
            InitializeComponent();
            View.WidthRequest = App.ScreenWidth;
        }
    }
}