using System;
using System.Linq;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PayAppDialog : PopupPage
    {
        public Color hex { get; set; }
        public bool isBonusVisible { get; set; }
        public decimal bonusCount { get; set; }
        public string title { get; set; }
        private RequestContent request;
        private Page appPage;
        private RestClientMP server = new RestClientMP();
        private bool isCardPay = true;


        public PayAppDialog(Color hexColor, RequestContent request, Page appPage)
        {
            hex = hexColor;
            this.request = request;
            this.appPage = appPage;
            title = request.PaidServiceText;
            isBonusVisible = false;
            InitializeComponent();
            Analytics.TrackEvent("Диалог выбора оплаты по заявке");
            Frame.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"],
                Color.Transparent);

            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);

          
            FormattedString formatted = new FormattedString();
            formatted.Spans.Add(new Span
            {
                Text = $"{AppResources.Total}: ",
                FontSize = 17,
                TextColor = Color.Black
            });
            decimal bonusPaid = getBonusPaid();
            formatted.Spans.Add(new Span
            {
                Text = (GetSumPaid(request) - bonusPaid).ToString(),
                FontSize = 20,
                TextColor = hex,
                FontAttributes = FontAttributes.Bold
            });
            formatted.Spans.Add(new Span
            {
                Text = $" {AppResources.Currency}",
                FontSize = 15,
                TextColor = Color.FromHex("#777777")
            });
            LabelTotal.FormattedText = formatted;
            if (bonusPaid > 0)
            {
                LabelBonusCount.Text = AppResources.BonusTotal + bonusPaid;
            }
            else
            {
                LabelBonusCount.IsVisible = false;
            }

            BindingContext = this;
        }

        decimal getBonusPaid()
        {
            decimal count = 0;
            foreach (var each in request.ReceiptItems)
            {
                count += each.BonusAmount;
            }

            return count;
        }

        decimal GetSumPaid(RequestContent info)
        {
            var sum = info.ReceiptItems.Aggregate<RequestsReceiptItem, decimal>(0, (current, each) => (decimal) (current + (each.Quantity * each.Price)));
            return sum;
        }

        private async void payApp(object sender, EventArgs e)
        {
            if (!request.IsPaidByUser)
            {
                if (isCardPay)
                {
                    await appPage.Navigation.PushAsync(
                        new PayServicePage("", GetSumPaid(request) - getBonusPaid(), request.ID));
                    await PopupNavigation.Instance.PopAsync();
                }
                else
                {
                    CommonResult result = await server.SendPaidRequestCompleteCodeOnlineAndCah(request.ID, Settings.Person.Phone, false);
                    if (result.Error != null)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            await DisplayAlert("", AppResources.AlertCodeSent, "OK");
                        }
                        else
                        {
                            DependencyService.Get<IMessage>().ShortAlert(AppResources.AlertCodeSent);
                        }  
                        await PopupNavigation.Instance.PopAsync();
                    }
                }
                
              
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorPayApp, "OK");
            }
        }

        private void btnCardPay_Clicked(object sender, EventArgs e)
        {
            isCardPay = true;
            frameBtnCardPay.BorderColor = hex;
            btnCardPay.TextColor = hex;
            
            frameBtnCashPay.BorderColor = Color.Gray;
            btnCashPay.TextColor = Color.Gray;
            BtnAdd.Text = AppResources.Pay;
            LabelTitle.Text = AppResources.ValidateCodeCard;
        }

        private void btnCashPay_Clicked(object sender, EventArgs e)
        {
            isCardPay = false;
            frameBtnCashPay.BorderColor = hex;
            btnCashPay.TextColor = hex;
            
            frameBtnCardPay.BorderColor = Color.Gray;
            btnCardPay.TextColor = Color.Gray;

            BtnAdd.Text = AppResources.GetCode;
            LabelTitle.Text = AppResources.ValidateCodeCash;
        }
        
    }
}