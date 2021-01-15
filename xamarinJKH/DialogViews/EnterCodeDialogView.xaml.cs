using System;
using AiForms.Dialogs.Abstractions;
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
    public partial class EnterCodeDialogView : DialogView
    {
        EnterCodeDialogViewModel viewModel { get; set; }
        public EnterCodeDialogView(string requestId)
        {
            InitializeComponent();
            IsCanceledOnTouchOutside = true;
            BindingContext = viewModel = new EnterCodeDialogViewModel(requestId, this);
        }

        public void CloseDialog()
        {
            DialogNotifier.Cancel();
        }
    }

    public class EnterCodeDialogViewModel : BaseViewModel
    {
        string requestID { get; set; }
        public Command SendCode { get; set; }
        public Color MainColor => Color.FromHex("#" + Settings.MobileSettings.color);

        RestClientMP Server { get; set; }
        EnterCodeDialogView dialog;
        public EnterCodeDialogViewModel(string id, EnterCodeDialogView view)
        {
            this.requestID = id;
            Server = new RestClientMP();
            dialog = view;
            SendCode = new Command<string>(async (code) =>
            {
                if (code != null)
                {
                    try
                    {
                        var success = await Server.SendCodeRequestForpaidService(new PaidRequestCodeModel { RequestId = this.requestID, Code = code });
                        if (success)
                        {
                            dialog.CloseDialog();
                            MessagingCenter.Send<Object>(this, "ChangeThemeConst");                                                       
                            DependencyService.Get<IMessage>().ShortAlert(AppResources.EnterCodeSuccess);
                        }
                        else
                        {
                            DependencyService.Get<IMessage>().ShortAlert(AppResources.EnterCodeWrongCode);
                        }
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                    
                }
                else
                {
                    DependencyService.Get<IMessage>().ShortAlert(AppResources.EnterCodeNoCode);
                }
            });
        }
    }
}