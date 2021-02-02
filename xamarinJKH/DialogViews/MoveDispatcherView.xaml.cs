using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.ViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MoveDispatcherView : PopupPage
    {
        private RestClientMP server = new RestClientMP();
        public Color HexColor { get; set; }
        public RequestInfo _Request { get; set; }
        bool IsConst = false;
        public int PikerDispItem = 0;
        List<ConsultantInfo> dispList { get; set; }

        public MoveDispatcherView(Color hexColor, RequestInfo request, bool isConst)
        {
            HexColor = hexColor;
            _Request = request;
            InitializeComponent();
            Analytics.TrackEvent("Диалог смены сотрудника");

            IsConst = isConst;
            getDispatcherList();
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
            var pickerOpen = new TapGestureRecognizer();
            pickerOpen.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => { PickerDisp.Focus(); }); };
            Layout.GestureRecognizers.Add(pickerOpen);

            var PickerDispDepartOpen = new TapGestureRecognizer();
            PickerDispDepartOpen.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => { PickerDispDepart.Focus(); }); };
            PickerDispDepartStack.GestureRecognizers.Add(PickerDispDepartOpen);

            var PickerDispKindOpen = new TapGestureRecognizer();
            PickerDispKindOpen.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => { PickerDispKind.Focus(); }); };
            PickerDispKindStack.GestureRecognizers.Add(PickerDispKindOpen);

            var CloseAppButtonTgr = new TapGestureRecognizer();
            CloseAppButtonTgr.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    CloseAppButton.IsEnabled = false;
                    if (!ClosingApp)
                    {
                        ClosingApp = true;
                        await StartProgressBar();
                        await Navigation.PopAsync();
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        ClosingApp = false;
                    }
                }
                catch
                {
                }
                finally
                {
                    ClosingApp = false;
                    CloseAppButton.IsEnabled = true;
                }
            }); };
            CloseAppButton.GestureRecognizers.Add(CloseAppButtonTgr);


            
        }

        public DispListModel _dispListModel = null;

        async void getDispatcherList()
        {
            try
            {
                List<ConsultantInfo> result = await server.GetConsultants();
                List<NamedValue> resultDepartsment = await server.GetDepartments();
                List<NamedValue> resultPools = await server.GetRequestPools();
                dispList = new List<ConsultantInfo>(result.Where(x => x.Name != null));
                BindingContext = null;
                if (resultDepartsment != null)
                {
                    resultDepartsment.Insert(0, new NamedValue
                    {
                        ID = -999,
                        Name = AppResources.NotDepartsment,
                        Value = -999
                    });
                }

                BindingContext = _dispListModel = new DispListModel()
                {
                    AllDisp = new ObservableCollection<ConsultantInfo>(dispList),
                    AllDispMain = new List<ConsultantInfo>(dispList),
                    AllPool = new ObservableCollection<NamedValue>(resultPools),
                    Kind = new ObservableCollection<string>
                    {
                        AppResources.MoveConsultant, AppResources.MovePool, AppResources.MoveDepartsment
                    },
                    AllDepartments = new ObservableCollection<NamedValue>(resultDepartsment),
                    hex = HexColor,
                    SelectedDisp = dispList[0],
                    SelectedKind = AppResources.MoveConsultant,
                    SelectedDepartments = resultDepartsment[0],
                    SelectedPool = resultPools[0]
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        bool ClosingApp;

        //private async void CloseApp(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!ClosingApp)
        //        {
        //            ClosingApp = true;
        //            await StartProgressBar();
        //            await Navigation.PopAsync();
        //            await Task.Delay(TimeSpan.FromSeconds(2));
        //            ClosingApp = false;
        //        }
        //    }
        //    catch
        //    {
        //    }
        //    finally
        //    {
        //        ClosingApp = false;
        //    }
        //}

        public async Task ShowToast(string title)
        {
            Toast.Instance.Show<ToastDialog>(new {Title = title, Duration = 1500});
            // Optionally, view model can be passed to the toast view instance.
        }

        public class DispListModel : BaseViewModel
        {
            public ObservableCollection<ConsultantInfo> AllDisp { get; set; }
            public List<ConsultantInfo> AllDispMain { get; set; }
            public ObservableCollection<NamedValue> AllPool { get; set; }
            public ObservableCollection<NamedValue> AllDepartments { get; set; }
            public ObservableCollection<string> Kind { get; set; }
            ConsultantInfo _selectedDisp;

            public ConsultantInfo SelectedDisp
            {
                get => _selectedDisp;
                set
                {
                    _selectedDisp = value;
                    OnPropertyChanged("SelectedDisp");
                }
            }

            bool isVisiblePool;

            public bool IsVisiblePool
            {
                get => isVisiblePool;
                set
                {
                    isVisiblePool = value;
                    OnPropertyChanged("IsVisiblePool");
                }
            }

            bool isVisibleDisp;

            public bool IsVisibleDisp
            {
                get => isVisibleDisp;
                set
                {
                    isVisibleDisp = value;
                    OnPropertyChanged("IsVisibleDisp");
                }
            }

            bool isVisibleDepart;

            public bool IsVisibleDepart
            {
                get => isVisibleDepart;
                set
                {
                    isVisibleDepart = value;
                    OnPropertyChanged("IsVisibleDepart");
                }
            }

            public NamedValue SelectedPool { get; set; }
            public NamedValue SelectedDepartments { get; set; }
            public Color hex { get; set; }

            public Command SelectKind { get; set; }

            public string _SelectedKind { get; set; }

            public string SelectedKind
            {
                get => _SelectedKind;
                set
                {
                    _SelectedKind = value;
                    OnPropertyChanged("SelectedKind");
                }
            }

            public Command SelectDepartsment { get; set; }

            public DispListModel()
            {
                SelectKind = new Command<object>(name =>
                {
                    if (SelectedKind.Equals(AppResources.MoveConsultant))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsVisiblePool = false;
                            IsVisibleDisp = true;
                            IsVisibleDepart = true;

                            SelectedPool = null;
                        });
                    }
                    else if (SelectedKind.Equals(AppResources.MovePool))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsVisiblePool = true;
                            IsVisibleDisp = false;
                            IsVisibleDepart = false;

                            SelectedDisp = null;
                            SelectedDepartments = null;
                        });
                    }
                    else if (SelectedKind.Equals(AppResources.MoveDepartsment))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsVisiblePool = false;
                            IsVisibleDisp = false;
                            IsVisibleDepart = true;

                            SelectedDisp = null;
                            SelectedPool = null;
                        });
                    }
                });
                SelectDepartsment = new Command<object>(name =>
                {
                    if (SelectedDepartments != null && !SelectedKind.Equals(AppResources.MoveDepartsment))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            AllDisp.Clear();
                            SelectedDisp = null;
                            foreach (var each in AllDispMain.Where(x =>
                                SelectedDepartments.ID == -999 || x.DivisionID == SelectedDepartments.ID))
                            {
                                AllDisp.Add(each);
                                SelectedDisp = AllDisp[0];
                            }
                        });
                    }
                });
            }
        }

        public async Task StartProgressBar(string title = "", double opacity = 0.6)
        {
            // Loading settings
            if (string.IsNullOrEmpty(title))
                title = AppResources.MoveDispatcherStatus;
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = HexColor == null ? (Color) App.Current.Resources["MainColor"] : HexColor,
                OverlayColor = Color.Black,
                Opacity = opacity,
                DefaultMessage = title,
            };


            await Loading.Instance.StartAsync(async progress =>
            {
                long? dispId = _dispListModel.SelectedDisp?.ID;
                int? departId = _dispListModel.SelectedDepartments?.ID;
                int? poolId = _dispListModel.SelectedPool?.ID;

                if (departId == -999 && dispId == null && poolId == null)
                {
                    await ShowToast(AppResources.SetMoveDepartsment);
                    return;
                }

                CommonResult result = await server.ChangeConsultant(_Request.ID, dispId, poolId, departId==-999 ? null : departId);
                if (result != null)
                {
                    if (result.Error == null)
                    {
                        if (!string.IsNullOrWhiteSpace(BordlessEditor.Text))
                        {
                            result = await server.AddMessageConst(BordlessEditor.Text, _Request.ID.ToString(), true);
                        }

                        await ShowToast(AppResources.MoveDispatcherSuccess);
                        MessagingCenter.Send<Object>(this, "UpdateAppCons");
                        await PopupNavigation.Instance.PopAsync();
                    }
                    else
                    {
                        await ShowToast(result.Error);
                    }
                }
                else
                {
                    await ShowToast(AppResources.ErrorUnknown);
                }
            });
        }

        private void pickerDisp_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        Thickness frameMargin = new Thickness();

        private void BordlessEditor_Focused(object sender, FocusEventArgs e)
        {
            if (DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                frameMargin = Frame.Margin;
                Device.BeginInvokeOnMainThread(() => { Frame.Margin = new Thickness(15, 0, 15, 15); });
            }
        }

        private void BordlessEditor_Unfocused(object sender, FocusEventArgs e)
        {
            if (DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                Device.BeginInvokeOnMainThread(() => { Frame.Margin = frameMargin; });
            }
        }

        private void PickerDispKind_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            _dispListModel.SelectKind.Execute(null);
        }

        private void PickerDispDepart_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            _dispListModel.SelectDepartsment.Execute(null);
        }

        private void PickerDispPool_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}