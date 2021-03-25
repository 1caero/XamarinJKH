using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AiForms.Dialogs;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeterValuesDialog : PopupPage
    {
        private MeterInfo mInfo;
        private RestClientMP server = new RestClientMP();

        public ObservableCollection<MeterValueInfo> MeterValueInfos { get; set; } =
            new ObservableCollection<MeterValueInfo>();

        public bool isRefresh { get; set; }
        private CountersPage countersPage;

        public MeterValuesDialog(MeterInfo mInfo, CountersPage countersPage)
        {
            this.mInfo = mInfo;
            this.countersPage = countersPage;
            InitializeComponent();
            PopupMeterValue.HeightRequest = App.ScreenWidth;
            CollectionViewFiles.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
            GetMeterValues();
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);

            SetName(mInfo);
            SetImage();
            SetLastPenanse();
            SetEnabled();
        }

        void SetEnabled()
        {
            if (mInfo.Values != null && mInfo.Values.Count > 0 && mInfo.Values[0].IsCurrentPeriod)
            {
                LayoutEdDell.IsVisible = true;
                var dellClick = new TapGestureRecognizer();
                dellClick.Tapped += async (s, e) =>
                {
                    bool displayAlert = await Settings.mainPage.DisplayAlert("", AppResources.DellCouneter,
                        AppResources.Yes, AppResources.Cancel);
                    if (displayAlert)
                    {
                        await Loading.Instance.StartAsync(async progress =>
                        {
                            CommonResult result = await server.DeleteMeterValue(mInfo.ID);
                            if (result.Error == null)
                            {
                                MessagingCenter.Send<Object>(this, "UpdateCounters");
                                await PopupNavigation.Instance.PopAsync();
                            }
                            else
                            {
                                await Settings.mainPage.DisplayAlert(AppResources.ErrorTitle, result.Error,
                                    "OK");
                            }

                        });
                    }
                };
                LayoutDell.GestureRecognizers.Add(dellClick);

                var editClick = new TapGestureRecognizer();
                editClick.Tapped += async (s, e) =>
                {
                    await countersPage.OpenMeter(mInfo);
                    await PopupNavigation.Instance.PopAsync();
                 
                };
                LayoutEdit.GestureRecognizers.Add(editClick);
            }
        }

        private void SetName(MeterInfo mInfo)
        {
            string name = (!string.IsNullOrWhiteSpace(mInfo.CustomName)) ? mInfo.CustomName : mInfo.Resource;
            FormattedString formattedResource = new FormattedString();
            formattedResource.Spans.Add(new Span
            {
                Text = name + ", " + mInfo.Units,
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18
            });
            LabelResourse.FormattedText = formattedResource;
        }

        async void GetMeterValues()
        {
//#if DEBUG
//            ItemsList<MeterValueInfo> meterValues = new ItemsList<MeterValueInfo>();
//            meterValues.Data = new List<MeterValueInfo>();
//            Random r = new Random();
//            for(int i=0;i<30; i++)
//            {
//                MeterValueInfo m = new MeterValueInfo();
//                m.IsCurrentPeriod = false;
//                int addDays = r.Next(-365, 0);
//                m.Period = DateTime.Now.AddDays(addDays).ToString("dd.mm.yyyy");
//                var p = r.Next(0, 99999);
//                m.Value = p;
//                m.ValueT2= r.Next(0, 99999);
//                m.ValueT3 = r.Next(0, 99999);
                
//                meterValues.Data.Add(m);
//            }
//            //r.Next(99999);

//#else
//       ItemsList<MeterValueInfo> meterValues = await server.MeterValues(mInfo.UniqueNum);
//#endif
            ItemsList<MeterValueInfo> meterValues = await server.MeterValues(mInfo.UniqueNum);
            if (meterValues.Error == null)
            {
                MeterValueInfos = new ObservableCollection<MeterValueInfo>(SetDateMeter(meterValues.Data));
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, meterValues.Error, "OK");
            }
            BindingContext = this;
        }

        List<MeterValueInfo> SetDateMeter(List<MeterValueInfo> meterValueInfo)
        {
            foreach (var each in meterValueInfo)
            {
                each.TariffNumberInt = mInfo.TariffNumberInt;
                each.ValueT2 ??= 0;
                each.ValueT3 ??= 0;
            }
            meterValueInfo.Add(new MeterValueInfo
            {
                Period = AppResources.Initial,
                Value = string.IsNullOrWhiteSpace(mInfo.StartValue.ToString()) ? 0 : mInfo.StartValue.Value,
                ValueT2 = mInfo.StartValueT2 ?? 0,
                ValueT3 = mInfo.StartValueT3 ?? 0,
                TariffNumberInt = mInfo.TariffNumberInt
            });
          
            return SetSortValues(meterValueInfo);
        }

        List<MeterValueInfo> SetSortValues(List<MeterValueInfo> meterValueInfo)
        {
            List<MeterValueInfo> result = new List<MeterValueInfo>();

            int count = meterValueInfo.Count;
            if (count > 1)
            {
                int center = (count / 2);
                for (int i = 0; i < center; i++)
                {
                    result.Add(meterValueInfo[i]);
                    result.Add(meterValueInfo[center + i]);
                }

                if (count % 2 != 0)
                {
                    result.Add(meterValueInfo[count-1]);
                }
            }
            else
            {
                result = meterValueInfo;
            }

            return result;
        }
        
        void SetLastPenanse()
        {
            if (mInfo.Values != null && mInfo.Values.Count > 0)
            {
                LabelLastPenance.Text = $"{AppResources.LastPenanse} {mInfo.Values[0].Period}: ";
                string values = mInfo.Values[0].Value.ToString();
                if (mInfo.TariffNumberInt > 1)
                {
                    List<string> tarifs = new List<string>
                    {
                        string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name,
                        string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name,
                        string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name
                    };
                    List<string> valuesT = new List<string>
                    {
                        mInfo.Values[0].Value.ToString(),
                        mInfo.Values[0].ValueT2.ToString() ?? "0",
                        mInfo.Values[0].ValueT3.ToString() ?? "0"
                    };
                    values = "";
                    for (int i = 0; i < mInfo.TariffNumberInt; i++)
                    {
                        values += $"{tarifs[i]} - {valuesT[i]}";
                        if (i != mInfo.TariffNumberInt - 1)
                        {
                            values += "\n";
                        }
                    }
                }

                LabelValueLastPenance.Text = values;
            }
            else
            {
                StackLayoutLastPenance.IsVisible = false;
            }
        }

        void SetImage()
        {
            if ((mInfo.Resource.ToLower().Contains("холод")  && !mInfo.Resource.ToLower().Contains("гвс"))|| (mInfo.Resource.ToLower().Contains("хвс") && !mInfo.Resource.ToLower().Contains("гвс")))
            {
                IconViewImage.Source = ImageSource.FromFile("ic_cold_water");
            }
            else if (mInfo.Resource.ToLower().Contains("горяч") || mInfo.Resource.ToLower().Contains("гвс"))
            {
                IconViewImage.Source = ImageSource.FromFile("ic_heat_water");
            }
            else if (mInfo.Resource.ToLower().Contains("подог") || mInfo.Resource.ToLower().Contains("отопл")|| mInfo.Resource.ToLower().Contains("тепл"))
            {
                IconViewImage.Source = ImageSource.FromFile("ic_heat_energ");
            }
            else if (mInfo.Resource.ToLower().Contains("эле"))
            {
                IconViewImage.Source = ImageSource.FromFile("ic_electr");
            }
            else if (mInfo.Resource.ToLower().Contains("газ"))
            {
                IconViewImage.Source = ImageSource.FromFile("ic_gas");
            }
            else
            {
                IconViewImage.Source = ImageSource.FromFile("ic_cold_water");
            }
        }
    }
}