using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetLsConstDialog : PopupPage
    {
        private RestClientMP _server = new RestClientMP();
        private ItemsList<HouseProfile> groups = new ItemsList<HouseProfile>();
        private Dictionary<string,List<Account>> Accountses = new Dictionary<string, List<Account>>();
        public SetLsConstDialog()
        {
            InitializeComponent();
            var setLss = new TapGestureRecognizer();
            setLss.Tapped += async (s, e) => { getHouse(); };
            StackLayoutHouse.GestureRecognizers.Add(setLss);
            var setRoom = new TapGestureRecognizer();
            setRoom.Tapped += async (s, e) => { getRooms(); };
            StackLayoutHouseRoom.GestureRecognizers.Add(setRoom);
            var setLs = new TapGestureRecognizer();
            setLs.Tapped += async (s, e) => { getLs(); };
            StackLayoutLs.GestureRecognizers.Add(setLs);
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
        }

        void setListHouse(ItemsList<HouseProfile> groups, ref string[] param)
        {
            Houses = new Dictionary<string, string>();
            param = new string [groups.Data.Count];
            int i = 0;
            foreach (var each in groups.Data)
            {
                try
                {
                    if (each.Address != null)
                    {
                        Houses.Add(each.Address, each.ID.ToString());
                        param[i] = each.Address;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                i++;
            }
        }

        async Task getHouse()
        {
            groups = await _server.GetHouse();
            if (groups.Error == null)
            {
                FrameBtnAdd.IsVisible = false;
                LabelHouseRoom.Text = AppResources.FlatChoose;
                StackLayoutHouseRoom.IsVisible = false;
                StackLayoutLs.IsVisible = false;
                string[] param = null;
                setListHouse(groups, ref param);
                var action = await DisplayActionSheet(AppResources.HomeChoose, AppResources.Cancel, null, param);
                if (action != null && !action.Equals(AppResources.Cancel))
                {
                    LabelHouse.Text = action;
                    StackLayoutHouseRoom.IsVisible = true;
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, groups.Error, "OK");
            }
        }

        int getHouseId(string name)
        {
            foreach (var each in groups.Data)
            {
                try
                {
                    if (each.Address.Equals(name))
                    {
                        return each.ID;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return -1;
        }


        async void getRooms()
        {
            int id = getHouseId(LabelHouse.Text);
            if (id != -1)
            {
                ItemsList<R731PremiseWithAccounts> itemsList = await _server.GetHouseData(id.ToString());
                if (itemsList.Error == null)
                {
                    LabelHouseLs.Text = AppResources.LsChoose;
                    FrameBtnAdd.IsVisible = false;
                    StackLayoutLs.IsVisible = false;
                    string[] paramsi = getLsRoom(itemsList.Data);
                    var action = await DisplayActionSheet("Выбор помещения", AppResources.Cancel, null, paramsi);
                    if (action != null && !action.Equals(AppResources.Cancel))
                    {
                        LabelHouseRoom.Text = action;
                        StackLayoutLs.IsVisible = true;
                    }
                    
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, groups.Error, "OK");
                }
            }
        }


        string[] getLsRoom(List<R731PremiseWithAccounts> premiseWithAccountses)
        {
            string[] paramsi = new string[premiseWithAccountses.Count];
            int i = 0;
            foreach (var each in premiseWithAccountses)
            {
                try
                {
                    Accountses.Add(each.Number, each.Accounts);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                paramsi[i] = each.Number;
                i++;
            }
            return paramsi;
        }

        string[] getAcc(List<Account> accounts)
        {
            string[] param = new string[accounts.Count];
            int i = 0;
            foreach (var each in accounts)
            {
                param[i] = each.Ident;
            }

            return param;
        }

        async void getLs()
        {
            string[] paramsi = getAcc(Accountses[LabelHouseRoom.Text]);
            var action = await DisplayActionSheet("Выбор ЛС", AppResources.Cancel, null, paramsi);
            if (action != null && !action.Equals(AppResources.Cancel))
            {
                LabelHouseLs.Text = action;
                FrameBtnAdd.IsVisible = true;
            }
        }

        public Dictionary<string, string> Houses { get; set; }

        private async void BtnAdd_OnClicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<Object, string>(this, "SetLs", LabelHouseLs.Text);
            await PopupNavigation.Instance.PopAsync();
        }
    }
}