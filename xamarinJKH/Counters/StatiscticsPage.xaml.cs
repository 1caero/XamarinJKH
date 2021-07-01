using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;

namespace xamarinJKH.Counters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatiscticsPage : ContentPage
    {
        public StatiscticsPage(MeterInfo meterInfo)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);

            UkName.Text = Settings.MobileSettings.main_name;

            viewModel = new StatiscticsPageViewModel(meterInfo);

            this.BindingContext = viewModel;


            MessagingCenter.Subscribe<object, int>(this, "setStartYear", (sender, arg) =>
            {
                Device.BeginInvokeOnMainThread(() => yearPicker.SelectedItem = arg);
            });

        }

        StatiscticsPageViewModel viewModel;

        private void yearPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var year = (int)((Picker)sender).SelectedItem;
            viewModel.SetChart(year);
        }
    }
    public class StatiscticsPageModel
    {
        public string Month { get; set; }

        public decimal Target { get; set; }

        public StatiscticsPageModel(string xValue, decimal yValue)
        {
            Month = xValue;
            Target = yValue;
        }
    }

    public class StatiscticsPageViewModel : BaseViewModel
    {
        public bool IsVisible { get; set; }
        public Color hex { get { return (Color)Application.Current.Resources["MainColor"]; } }

        public ObservableCollection<StatiscticsPageModel> Data { get => data; set { data = value; OnPropertyChanged("Data"); } }
        public ObservableCollection<StatiscticsPageModel> Data2 { get => data2; set { data2 = value; OnPropertyChanged("Data2"); } }
        public ObservableCollection<StatiscticsPageModel> Data3 { get => data3; set { data3 = value; OnPropertyChanged("Data3"); } }

        public List<int> Years { get => years; set { years = value; OnPropertyChanged("Years"); } }


        public string MaxValue { get => maxvalue; set { maxvalue = value; OnPropertyChanged("MaxValue"); } }
        public string MinValue { get => minvalue; set { minvalue = value; OnPropertyChanged("MinValue"); } }
        public string TotalValue { get => totalvalue; set { totalvalue = value; OnPropertyChanged("TotalValue"); } }

        public string DataName { get => dataName; set { dataName = value; OnPropertyChanged("DataName"); } }
        public string DataName2 { get => dataName2; set { dataName2 = value; OnPropertyChanged("DataName2"); } }
        public string DataName3 { get => dataName3; set { dataName3 = value; OnPropertyChanged("DataName3"); } }

        public bool DataName2Visible { get => dataName2Visible; set { dataName2Visible = value; OnPropertyChanged("DataName2Visible"); } }
        public bool DataName3Visible { get => dataName3Visible; set { dataName3Visible = value; OnPropertyChanged("DataName3Visible"); } }


        decimal GetDelta(MeterValueInfo start, MeterValueInfo end)
        {
            decimal delta = 0;
            delta = end.Value - start.Value;
            if (start.ValueT2 != null)
            {
                delta = (decimal)(end.ValueT2 - start.ValueT2);
            }
            if (start.ValueT3 != null)
            {
                delta = (decimal)(end.ValueT3 - start.ValueT3);
            }

            return delta;
        }
        void getmax(int year)
        {
            var selection = ValuesOrdered.Where(_ => _.Period.Split('.')[2] == year.ToString()).ToList();
           
            decimal delta;
            int index = 0;
            if(year==Years.Max())
            {
                if (Years.Count > 1)
                   delta=selection.First().Value- ValuesOrdered.Where(_ => _.Period.Split('.')[2] == (year - 1).ToString()).Last().Value;
            }
        }
        public void SetChart(int? year)
        {
            if (year == null)
            {
                year = Years.Max();
                MessagingCenter.Send<object, int>(this, "setStartYear", (int)year);
            }

            Data = new ObservableCollection<StatiscticsPageModel>();
            Data2 = new ObservableCollection<StatiscticsPageModel>();
            Data3 = new ObservableCollection<StatiscticsPageModel>();

            var cltr = Application.Current.Properties["Culture"].ToString();

            foreach (var value in ValuesOrdered)
            {
                var periodArray = value.Period.Split('.');
                if (periodArray[2] == year.ToString())
                {
                    var mon = (new DateTime(1, Convert.ToInt32(periodArray[1]), 1)).ToString("MMM", new CultureInfo(cltr));
                    Data.Add(new StatiscticsPageModel(mon, value.Value));
                    if (value.ValueT2 != null)
                        Data2.Add(new StatiscticsPageModel(mon, (decimal)value.ValueT2));
                    if (value.ValueT3 != null)
                        Data3.Add(new StatiscticsPageModel(mon, (decimal)value.ValueT3));
                }
            }

            DataName2Visible = false;
            DataName3Visible = false;

            var maxV = Data.Max(_ => _.Target);
            var minV = Data.Min(_ => _.Target);
            var tot = Data.Sum(_ => _.Target);
            if (Data2 != null && Data2.Any())
            {
                DataName2Visible = true;
   maxV += Data2.Max(_ => _.Target);
                minV += Data2.Min(_ => _.Target);
                tot += Data2.Sum(_ => _.Target);
            }
            if (Data3 != null && Data3.Any())
            {
                DataName3Visible = true;

                maxV += Data3.Max(_ => _.Target);
                minV += Data3.Min(_ => _.Target);
                tot += Data3.Sum(_ => _.Target);
            }

            var max = Data.Max(_ => _.Target);
            var maxMName = Data.First(_ => _.Target == max).Month;

            MaxValue = $"{maxMName} - {maxV} {meter.Units}";

            var min = Data.Min(_ => _.Target);
            var minMName = Data.First(_ => _.Target == min).Month;
            MinValue = $"{minMName} - {minV} {meter.Units}";

            TotalValue = $"{tot} {meter.Units}";
        }

        List<MeterValueInfo> ValuesOrdered = new List<MeterValueInfo>();
        private ObservableCollection<StatiscticsPageModel> data;
        private List<int> years;
        private ObservableCollection<StatiscticsPageModel> data2;
        private ObservableCollection<StatiscticsPageModel> data3;
        private string maxvalue;
        private string minvalue;
        private string totalvalue;

        async void Init(string UniqueNum)
        {
            IsBusy = true;
            ItemsList<MeterValueInfo> meterValues = await Server.MeterValues(UniqueNum);
            if (meterValues.Error == null)
            {
                foreach (var val in meterValues.Data)
                {


                    if (DateTime.TryParseExact(val.Period, "dd.mm.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime pdate))
                        ValuesOrdered.Add(val);
                    else
                    {

                    }

                }
                ValuesOrdered = ValuesOrdered.OrderBy(_ => Convert.ToDateTime(_.Period, System.Globalization.CultureInfo.GetCultureInfo("RU"))).ToList();
                Years = ValuesOrdered.Select(_ => Convert.ToDateTime(_.Period, System.Globalization.CultureInfo.GetCultureInfo("RU")).Year).Distinct().ToList();
                SetChart(null);
            }
            else
            {
                // await DisplayAlert(AppResources.ErrorTitle, meterValues.Error, "OK");
            }
            IsBusy = false;
        }

        MeterInfo meter;
        private string dataName;
        private string dataName2;
        private string dataName3;
        private bool dataName3Visible;
        private bool dataName2Visible;

        public StatiscticsPageViewModel(MeterInfo meterInfo)
        {
            meter = meterInfo;
            
            DataName = string.IsNullOrWhiteSpace(meter.Tariff1Name) ? AppResources.tarif1 : meterInfo.Tariff1Name;
            DataName2 = string.IsNullOrWhiteSpace(meter.Tariff2Name) ? AppResources.tarif2 : meterInfo.Tariff2Name;
            DataName3 = string.IsNullOrWhiteSpace(meter.Tariff3Name) ? AppResources.tarif3 : meterInfo.Tariff3Name;

            Init(meterInfo.UniqueNum);
        }
    }
}
