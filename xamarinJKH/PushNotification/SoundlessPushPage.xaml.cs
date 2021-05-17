using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;

namespace xamarinJKH.PushNotification
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundlessPushPage : ContentPage
    {
        #region SoundlessViewModel

        private SoundlessViewModel _soundlessPushViewModel;

        public SoundlessViewModel SoundlessPushViewModel
        {
            get { return _soundlessPushViewModel; }
            set
            {
                _soundlessPushViewModel = value;
                OnPropertyChanged();
            }
        }

        #endregion
        
        public SoundlessPushPage()
        {
            SoundlessPushViewModel = new SoundlessViewModel();
            InitializeComponent();
            BindingContext = SoundlessPushViewModel;
            NavigationPage.SetHasNavigationBar(this, false);
            UkName.Text = Settings.MobileSettings.main_name;
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                string phone = Preferences.Get("techPhone", Settings.Person.Phone);
                if (Settings.Person != null && !string.IsNullOrWhiteSpace(phone))
                {
                    Settings.SetPhoneTech(phone);
                    await Navigation.PushModalAsync(new AppPage());
                }
                else
                {
                    await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
                }
            };
            LabelTech.GestureRecognizers.Add(techSend);
        }

        private async void ButtonAddRule_OnClicked(object sender, EventArgs e)
        {
            Button button = (Button) sender;
            await button.ScaleTo(0.6, 250, Easing.Linear);
            await button.ScaleTo(1, 250, Easing.Linear);
            await Dialog.Instance.ShowAsync<AddRuleDialog>(SoundlessPushViewModel.AddRuleViewModel);
        }
        
        public class SoundlessViewModel : BaseViewModel
        {
            private RestClientMP server = new RestClientMP();
            #region AddRuleViewModel

            private AddRuleViewModel _addRuleViewModel;

            public AddRuleViewModel AddRuleViewModel
            {
                get { return new AddRuleViewModel(); }
                set
                {
                    _addRuleViewModel = value;
                    OnPropertyChanged("AddRuleViewModel");
                }
            }

            #endregion

            public ObservableCollection<SilenceOption> Rules { get; set; }
            public SoundlessViewModel()
            {
                GetRules();
            }

            async void GetRules()
            {
                List<SilenceOption> silenceSettings = await server.GetSilenceSettings();
                Device.BeginInvokeOnMainThread( () => Rules = new ObservableCollection<SilenceOption>(silenceSettings));
            }

            
        }

        public class AddRuleViewModel : BaseViewModel
        {
            public ObservableCollection<OptionModel> DaysOfWeek { get; set; }

            private RestClientMP server = new RestClientMP();

            #region Dialog

            private IDialogNotifier  _dialog;

            public IDialogNotifier  Dialog
            {
                get { return _dialog; }
                set
                {
                    _dialog = value;
                    OnPropertyChanged("Dialog");
                }
            }

            #endregion

            
            
            #region CommandSave

            private ICommand _commandSave;

            public ICommand CommandSave
            {
                get { return _commandSave; }
                set
                {
                    _commandSave = value;
                    OnPropertyChanged();
                }
            }

            #endregion

            #region IsEnabledSave

            private bool _isEnabledSave;

            public bool IsEnabledSave
            {
                get { return _isEnabledSave; }
                set
                {
                    _isEnabledSave = value;
                    OnPropertyChanged("IsEnabledSave");
                }
            }

            #endregion

            #region StartTime

            private TimeSpan? _startTime;

            public TimeSpan? StartTime
            {
                get { return _startTime; }
                set
                {
                    _startTime = value;
                    OnPropertyChanged("StartTime");
                }
            }

            #endregion

            #region EndTime

            private TimeSpan? _endTime;

            public TimeSpan? EndTime
            {
                get { return _endTime; }
                set
                {
                    _endTime = value;
                    OnPropertyChanged("EndTime");
                }
            }

            #endregion

            #region MinimumDate

            private DateTime _minimumDate;

            public DateTime MinimumDate
            {
                get { return DateTime.Now; }
                set
                {
                    _minimumDate = value;
                    OnPropertyChanged("MinimumDate");
                }
            }

            #endregion

            #region StartDate

            private DateTime? _startDate = DateTime.Now;

            public DateTime? StartDate
            {
                get { return _startDate; }
                set
                {
                    _startDate = value;
                    OnPropertyChanged("StartDate");
                }
            }

            #endregion

            #region EndDate

            private DateTime? _endDate;

            public DateTime? EndDate
            {
                get { return _endDate; }
                set
                {
                    _endDate = value;
                    OnPropertyChanged("EndDate");
                }
            }

            #endregion

            public AddRuleViewModel()
            {
                InitDays();
                CommandSave = new Command(async () =>
                {
                    string fromTime = null;
                    if (StartTime != null)
                    {
                        DateTime dop = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,
                               StartTime.Value.Hours,
                               StartTime.Value.Minutes,
                               StartTime.Value.Seconds,
                               StartTime.Value.Milliseconds,
                               DateTime.Now.Kind
                               
                        );
                        fromTime = $"{dop:HH:mm}";
                    }

                    string toTime = null;
                    if (EndTime != null)
                    {
                        DateTime dop = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,
                            EndTime.Value.Hours,
                            EndTime.Value.Minutes,
                            EndTime.Value.Seconds,
                            EndTime.Value.Milliseconds,
                            DateTime.Now.Kind
                               
                        );
                        toTime = $"{dop:HH:mm}";
                    }
                    CommonResult newSilenceOption = await server.NewSilenceOption($"{StartDate:dd.MM.yyy}", $"{EndDate:dd.MM.yyy}",
                        fromTime, toTime,
                        DaysOfWeek[0].IsSelected, DaysOfWeek[1].IsSelected, DaysOfWeek[2].IsSelected,
                        DaysOfWeek[3].IsSelected, DaysOfWeek[4].IsSelected, DaysOfWeek[5].IsSelected,
                        DaysOfWeek[6].IsSelected
                    );
                    if (newSilenceOption.Error == null)
                    {
                        Toast.Instance.Show<ToastDialog>(new
                        {
                            Title = "Настройка добавлена", Duration = 500, ColorB = Color.Gray,
                            ColorT = Color.White
                        });
                        Dialog.Cancel();
                    }
                    else
                    {
                        Toast.Instance.Show<ToastDialog>(new
                        {
                            Title = newSilenceOption.Error, Duration = 500, ColorB = Color.Gray,
                            ColorT = Color.White
                        });
                    }
                });
            }

            private void InitDays()
            {
                DaysOfWeek = new ObservableCollection<OptionModel>();
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ПН",
                    Command = new Command(Execute)
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ВТ",
                    Command = new Command(Execute)
                    
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "СР",
                    Command = new Command(Execute)
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ЧТ",
                    Command = new Command(Execute)
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ПТ",
                    Command = new Command(Execute)
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "СБ",
                    Command = new Command(Execute)
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ВС",
                    Command = new Command(Execute)
                });
            }

            private void Execute(object obj)
            {
                IsEnabledSave = DaysOfWeek.Any(x => x.IsSelected);
            }
        }
        
    }
}