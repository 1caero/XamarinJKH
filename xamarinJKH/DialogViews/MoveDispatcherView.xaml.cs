using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using xamarinJKH.Utils.ReqiestUtils;
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
        private readonly bool _addApp;
        private readonly ICommand _setMoveCommand;
        public int PikerDispItem = 0;
        List<ConsultantInfo> dispList { get; set; }

        public MoveDispatcherView(Color hexColor, RequestInfo request, bool isConst, bool addApp = false,
            ICommand setMoveCommand = null)
        {
            HexColor = hexColor;
            _Request = request;
            InitializeComponent();
            Analytics.TrackEvent("Диалог смены сотрудника");

            IsConst = isConst;
            _addApp = addApp;
            _setMoveCommand = setMoveCommand;
            getDispatcherList();
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
            var pickerOpen = new TapGestureRecognizer();
            pickerOpen.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => { PickerDisp.Focus(); }); };
            Layout.GestureRecognizers.Add(pickerOpen);

            var PickerDispDepartOpen = new TapGestureRecognizer();
            PickerDispDepartOpen.Tapped += (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() => { PickerDispDepart.Focus(); });
            };
            PickerDispDepartStack.GestureRecognizers.Add(PickerDispDepartOpen);

            var PickerDispKindOpen = new TapGestureRecognizer();
            PickerDispKindOpen.Tapped += (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() => { PickerDispKind.Focus(); });
            };
            PickerDispKindStack.GestureRecognizers.Add(PickerDispKindOpen);

            var CloseAppButtonTgr = new TapGestureRecognizer();
            CloseAppButtonTgr.Tapped += (s, e) => { MoveApp(); };
            CloseAppButton.GestureRecognizers.Add(CloseAppButtonTgr);
        }

        private void MoveApp()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
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
            });
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
            Toast.Instance.Show<ToastDialog>(new
                {Title = title, Duration = 1500, ColorB = Color.Gray, ColorT = Color.White});
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
                            SelectedPool = AllPool[0];
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

                if (!_addApp)
                {
                    CommonResult result = await server.ChangeConsultant(_Request.ID, dispId, poolId,
                        departId == -999 ? null : departId);
                    if (result != null)
                    {
                        if (result.Error == null)
                        {
                            if (!string.IsNullOrWhiteSpace(BordlessEditor.Text))
                            {
                                result = await server.AddMessageConst(BordlessEditor.Text, _Request.ID.ToString(),
                                    true);
                            }

                            await ShowToast(AppResources.MoveDispatcherSuccess);
                            // MessagingCenter.Send<Object>(this, "UpdateAppCons");
                            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
                            {
                                RequestUtils.UpdateRequestCons();
                                MessagingCenter.Send<Object>(this,  "RefreshApp");
                                return false; // runs again, or false to stop
                            });
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
                }
                else
                {
                    StringBuilder text = new StringBuilder();
                    text.Append($"{_dispListModel.SelectedKind} - ");
                    if (dispId != null)
                    {
                        text.Append($"{_dispListModel.SelectedDisp.Name}\n");
                    }

                    if (departId != null)
                    {
                        text.Append($"{_dispListModel.SelectedDepartments._Name} ");
                    }

                    if (poolId != null)
                    {
                        text.Append($"{_dispListModel.SelectedPool._Name} ");
                    }

                    if (!string.IsNullOrWhiteSpace(BordlessEditor.Text))
                    {
                        text.Append($"\nС сообщением: \"{BordlessEditor.Text}\"");
                    }

                    _setMoveCommand.Execute(new Tuple<long?, int?, int?, string, string?>(dispId, departId, poolId,
                        text.ToString(), BordlessEditor.Text));
                    await PopupNavigation.Instance.PopAsync();
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

        protected override bool OnBackButtonPressed()
        {
            PopupNavigation.Instance.PopAsync();
            return true;
        }

        private void PickerDispPool_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void BtnMove_OnClicked(object sender, EventArgs e)
        {
            MoveApp();
        }
    }
}