using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.ViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchDialogView : PopupPage
    {
        int Type;
        SearchDialogViewModel viewModel { get; set; }
        public SearchDialogView(int type, int? id = null)
        {
            InitializeComponent();
            Type = type;
            BindingContext = viewModel = new SearchDialogViewModel(type, id);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.LoadResults.Execute(null);
        }

        private void BordlessEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as BordlessEditor).Text;
            viewModel.Filter.Execute(text);
        }

        private async void ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection[0] as NamedValue;
            if (item != null)
            {
                if (Type == (int)SearchType.DISTRICT)
                {
                    MessagingCenter.Send<Object, NamedValue>(this, "SetDistrict", item);
                }

                if (Type == (int)SearchType.STREET)
                {
                    MessagingCenter.Send<Object, NamedValue>(this, "SetHouse", item);
                }

                if (Type == (int)SearchType.FLAT)
                {
                    MessagingCenter.Send<Object, NamedValue>(this, "SetPremise", item);
                }
            }

            await PopupNavigation.PopAllAsync();
        }
    }


    public class SearchDialogViewModel : BaseViewModel
    {
        public ObservableCollection<NamedValue> Items { get; set; }
        public Command LoadResults { get; set; }
        public Command Filter { get; set; }
         
        public int Type { get; set; }
        public SearchDialogViewModel(int type, int? id = null)
        {
            Type = type;
            Items = new ObservableCollection<NamedValue>();
            LoadResults = new Command<string>((text) =>
            {
                string searchTerm = Convert.ToString(text);
                IsBusy = true;
                Task.Run(async () =>
                {
                    var result = new ItemsList<NamedValue>();
                    if (Type == (int)SearchType.DISTRICT)
                    {
                        result = await Server.GetHouseGroups();
                    }

                    if (Type == (int)SearchType.STREET || Type == (int)SearchType.HOUSE)
                    {
                        var houses = await Server.GetHouse();
                        result.Data = new List<NamedValue>();
                        result.Error = houses.Error;
                        foreach (var house in houses.Data)
                        {
                            var val = new NamedValue();
                            val.ID = house.ID;
                            val.Name = house.Address;
                            result.Data.Add(val);
                        }
                    }

                    if (Type == (int)SearchType.FLAT)
                    {
                        var premises = await Server.GetHouseData(id.ToString());
                        result.Error = premises.Error;
                        result.Data = new List<NamedValue>();
                        foreach (var premise in premises.Data)
                        {
                            var flat = new NamedValue();
                            flat.ID = premise.ID;
                            flat.Name = premise.Number;
                            result.Data.Add(flat);
                        }
                    }

                    IsBusy = false;
                    if (result.Error == null)
                    {
                        Items.Clear();
                        foreach (var item in result.Data)
                            Device.BeginInvokeOnMainThread(() => Items.Add(item));
                    }
                });
                
            });
            Filter = new Command<string>((text) =>
            {
                if (string.IsNullOrEmpty(text.ToString()))
                {
                    LoadResults.Execute(null);
                }
                else
                {
                    var filtered = Items.Where(x => x.Name.ToLower().Contains(text.ToString().ToLower())).ToList();
                    Items.Clear();
                    foreach (var item in filtered)
                    {
                        Items.Add(item);
                    }
                }
            });
        }
    }
}