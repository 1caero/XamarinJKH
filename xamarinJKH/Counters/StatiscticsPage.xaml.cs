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
                delta += (decimal)(end.ValueT2 - start.ValueT2);
            }
            if (start.ValueT3 != null)
            {
                delta += (decimal)(end.ValueT3 - start.ValueT3);
            }

            return delta;
        }

        decimal GetDeltaT(MeterValueInfo start, MeterValueInfo end, int t)
        {            
            switch (t)
            {
                case 1:
                    return end.Value - start.Value;
                case 2:
                    return (decimal)(end.ValueT2 - start.ValueT2);
                case 3:
                    return (decimal)(end.ValueT3 - start.ValueT3);
            }

            return 0;
        }



        decimal max;
        decimal min;
        decimal sum;
        string maxMName;
        string minMName;

        decimal GetMax(decimal delta, List<MeterValueInfo> selection, out int index)
        {
            index = 0;

            for (var i = 1; i < selection.Count() - 1; i++)
            {
                var deltaNew = GetDelta(selection[i - 1], selection[i]);
                if (delta < deltaNew)
                { 
                    delta = deltaNew;
                    index = i;
                }
            }

            return delta;
        }

        decimal GetMin(decimal delta, List<MeterValueInfo> selection, out int index)
        {
            index = 0;

            for (var i = 1; i < selection.Count() - 1; i++)
            {
                var deltaNew = GetDelta(selection[i - 1], selection[i]);
                if (delta > deltaNew)
                {
                    delta = deltaNew;
                    index = i;
                }
            }
            return delta;
        }

        decimal GetSum(decimal delta, List<MeterValueInfo> selection)
        {
            for (var i = 1; i < selection.Count() - 1; i++)
            {
                var deltaNew = GetDelta(selection[i - 1], selection[i]);
                delta += deltaNew;
            }
            return delta;
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

            var ruCultureInfo = new CultureInfo(Application.Current.Properties["Culture"].ToString());

            var selection = ValuesOrdered.Where(_ => _.Period.Split('.')[2] == year.ToString() && _.Kind == "Учтено").ToList();
            decimal delta;

            var start = selection.First();

            if (Years.Count > 1 && year > Years.Min())
                delta = GetDelta(start, ValuesOrdered.Where(_ => _.Period.Split('.')[2] == (year - 1).ToString()).Last());
            else
            {
                
                delta = start.Value;
                if (start.ValueT2 != null)
                {
                    delta += (decimal)(start.ValueT2);
                }
                if (start.ValueT3 != null)
                {
                    delta += (decimal)(start.ValueT3);
                }
            }

            max = GetMax(delta, selection, out int indexMax);

            maxMName = (new DateTime(1, Convert.ToInt32(selection[indexMax].Period.Split('.')[1]), 1)).ToString("MMM", ruCultureInfo);
            min = GetMin(delta, selection, out int indexMin);
            minMName = (new DateTime(1, Convert.ToInt32(selection[indexMin].Period.Split('.')[1]), 1)).ToString("MMM", ruCultureInfo);
            sum = GetSum(delta, selection);

            MaxValue = $"{maxMName} - {max} {meter.Units}";           
            
            MinValue = $"{minMName} - {min} {meter.Units}";

            TotalValue = $"{sum} {meter.Units}";

            //заполнение графиков 
            //for (var i = 0; i < ValuesOrdered.Count; i++)
            //{
            //    var periodArray = ValuesOrdered[i].Period.Split('.');
            //    if (periodArray[2] == year.ToString())
            //    {
            //        var mon = (new DateTime(1, Convert.ToInt32(periodArray[1]), 1)).ToString("MMM", ruCultureInfo);
            //        //Data.Add(new StatiscticsPageModel(mon, value.Value));
            //        //if (value.ValueT2 != null)
            //        //    Data2.Add(new StatiscticsPageModel(mon, (decimal)value.ValueT2));
            //        //if (value.ValueT3 != null)
            //        //    Data3.Add(new StatiscticsPageModel(mon, (decimal)value.ValueT3));
            //    }
            //}


            var mon = (new DateTime(1, Convert.ToInt32(start.Period.Split('.')[1]), 1)).ToString("MMM", ruCultureInfo);

            if (Years.Count > 1 && year > Years.Min())
            {
                var end = ValuesOrdered.Where(_ => _.Period.Split('.')[2] == (year - 1).ToString()).Last();
                delta = GetDeltaT(start, end , 1);
                Data.Add(new StatiscticsPageModel(mon, delta));
                if(meter.TariffNumberInt==2)
                {
                    delta = GetDeltaT(start, end, 2);
                    Data2.Add(new StatiscticsPageModel(mon, delta));
                }
                if (meter.TariffNumberInt == 3)
                {
                    delta = GetDeltaT(start, end, 3);
                    Data3.Add(new StatiscticsPageModel(mon, delta));
                }
            }                
            else
            {                                
                Data.Add(new StatiscticsPageModel(mon, start.Value));

                if (meter.TariffNumberInt == 2)
                {
                    Data2.Add(new StatiscticsPageModel(mon, (decimal)start.ValueT2));
                }
                if (meter.TariffNumberInt == 3)
                {                    
                    Data3.Add(new StatiscticsPageModel(mon, (decimal)start.ValueT3));
                }                
            }

            for (var i = 1; i < selection.Count() - 1; i++)
            {
                var deltaNew = GetDeltaT(selection[i - 1], selection[i],1);
                mon = (new DateTime(1, Convert.ToInt32(selection[i].Period.Split('.')[1]), 1)).ToString("MMM", ruCultureInfo);
                Data.Add(new StatiscticsPageModel(mon, deltaNew));

                if (meter.TariffNumberInt == 2)
                {
                    Data2.Add(new StatiscticsPageModel(mon, GetDeltaT(selection[i - 1], selection[i], 2)));
                }
                if (meter.TariffNumberInt == 3)
                {
                    Data3.Add(new StatiscticsPageModel(mon, GetDeltaT(selection[i - 1], selection[i], 3)));
                }

            }


            DataName2Visible = false;
            DataName3Visible = false;

            if (Data2 != null && Data2.Any())
            {
                DataName2Visible = true;
            }
            if (Data3 != null && Data3.Any())
            {
                DataName3Visible = true;
            }
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
            var ruCulture = System.Globalization.CultureInfo.GetCultureInfo("RU");
            if (meterValues.Error == null)
            {
                foreach (var val in meterValues.Data)
                {
                    if (DateTime.TryParseExact(val.Period, "dd.mm.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime pdate))
                        ValuesOrdered.Add(val);                    
                }
                ValuesOrdered = ValuesOrdered.OrderBy(_ => Convert.ToDateTime(_.Period, ruCulture)).ToList();
                Years = ValuesOrdered.Select(_ => Convert.ToDateTime(_.Period, ruCulture).Year).Distinct().ToList();
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
