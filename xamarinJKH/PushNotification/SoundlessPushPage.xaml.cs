using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
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
            // await Dialog.Instance.ShowAsync<AddRuleDialog>(SoundlessPushViewModel.AddRuleViewModel);
        }

        public class SoundlessViewModel : BaseViewModel
        {
            #region AddRuleViewModel

            public AddRuleViewModel AddRuleViewModel
            {
                get => new AddRuleViewModel(RefreshCommand);
            }

            #endregion

            #region RefreshCommand

            private ICommand _refreshCommand;

            public ICommand RefreshCommand
            {
                get { return _refreshCommand; }
                set
                {
                    _refreshCommand = value;
                    OnPropertyChanged("RefreshCommand");
                }
            }

            #endregion

            #region OpenAddRule

            private ICommand _openAddRele;

            public ICommand OpenAddRule
            {
                get { return _openAddRele; }
                set
                {
                    _openAddRele = value;
                    OnPropertyChanged("OpenAddRule");
                }
            }

            #endregion

            #region Rules

            private ObservableCollection<SilenceOption> _rules;

            public ObservableCollection<SilenceOption> Rules
            {
                get { return _rules; }
                set
                {
                    _rules = value;
                    OnPropertyChanged("Rules");
                }
            }

            #endregion

            #region RemoveRuleCommand

            private ICommand _removeRuleComman;

            public ICommand RemoveRuleCommand
            {
                get { return _removeRuleComman; }
                set
                {
                    _removeRuleComman = value;
                    OnPropertyChanged("RemoveRuleCommand");
                }
            }

            #endregion


            public SoundlessViewModel()
            {
                RefreshCommand = new Command(RefreshAction);
                OpenAddRule = new Command(AddRuleAction);
                RemoveRuleCommand = new Command(RemoveRuleAction);
                GetRules();
            }

            private async void RemoveRuleAction(object obj)
            {
                SilenceOption silenceOption = (SilenceOption) obj;
                bool showChoose = await ShowChoose("Удалить настройку?");
                if (showChoose)
                {
                    var removeSilenceOption = await Server.RemoveSilenceOption(silenceOption.ID);
                    if (removeSilenceOption.Error == null)
                    {
                        Device.BeginInvokeOnMainThread(() => {
                        {
                            Rules.Remove(silenceOption);
                        } });
                        Toast.Instance.Show<ToastDialog>(new
                            {Title = "Настройка удалена", Duration = 500, ColorB = Color.Gray, ColorT = Color.White});
                    }
                    else
                    {
                        ShowError(removeSilenceOption.Error);
                    }
                }

                
            }

            private async void AddRuleAction()
            {
                await ShowDialog<AddRuleDialog>(AddRuleViewModel);
            }

            async void GetRules()
            {
                RefreshCommand.Execute(null);
            }

            private async void RefreshAction()
            {
                IsRefreshing = true;

                List<SilenceOption> silenceSettings = await Server.GetSilenceSettings();

                Device.BeginInvokeOnMainThread(() =>
                    {
                        Rules = new ObservableCollection<SilenceOption>(silenceSettings);
                    }
                );
                IsRefreshing = false;
            }
        }

        public class AddRuleViewModel : BaseViewModel
        {
            public ObservableCollection<OptionModel> DaysOfWeek { get; set; }


            #region Dialog

            private IDialogNotifier _dialog;

            public IDialogNotifier Dialog
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

            #region RefreshRulesCommand

            private ICommand _refreshRulesCommand;

            public ICommand RefreshRulesCommand
            {
                get { return _refreshRulesCommand; }
                set
                {
                    _refreshRulesCommand = value;
                    OnPropertyChanged("RefreshRulesCommand");
                }
            }

            #endregion

            #region SelectRuleCommand

            private ICommand _selectRuleCommand;

            public ICommand SelectRuleCommand
            {
                get { return _selectRuleCommand; }
                set
                {
                    _selectRuleCommand = value;
                    OnPropertyChanged("SelectRuleCommand");
                }
            }

            #endregion

            #region OpenHelpCommand

            private ICommand _OpenHelpCommand;

            public ICommand OpenHelpCommand
            {
                get { return _OpenHelpCommand; }
                set
                {
                    _OpenHelpCommand = value;
                    OnPropertyChanged("OpenHelpCommand");
                }
            }

            #endregion

            

            public AddRuleViewModel(ICommand refresRules)
            {
                InitDays();
                RefreshRulesCommand = refresRules;
                SelectRuleCommand = new Command(SelectDayAction);
                CommandSave = new Command(SaveRuleAction);
                OpenHelpCommand = new Command(OpenHelpAction);
            }

            private void OpenHelpAction()
            {
                ShowError("Если время окончания меньше времени начала, то считается что окончание будет на следующие сутки после времени начала, таким образом можно включить беззвучный режим на ночь.","Примечание");
            }

            private async void SaveRuleAction()
            {
                string fromTime = null;
                if (StartTime != null)
                {
                    DateTime dop = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        StartTime.Value.Hours, StartTime.Value.Minutes, StartTime.Value.Seconds,
                        StartTime.Value.Milliseconds, DateTime.Now.Kind);
                    fromTime = $"{dop:HH:mm}";
                }

                string toTime = null;
                if (EndTime != null)
                {
                    DateTime dop = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        EndTime.Value.Hours, EndTime.Value.Minutes, EndTime.Value.Seconds, EndTime.Value.Milliseconds,
                        DateTime.Now.Kind);
                    toTime = $"{dop:HH:mm}";
                }

                CommonResult newSilenceOption = await Server.NewSilenceOption($"{StartDate:dd.MM.yyy}",
                    $"{EndDate:dd.MM.yyy}", fromTime, toTime, DaysOfWeek[0].IsSelected, DaysOfWeek[1].IsSelected,
                    DaysOfWeek[2].IsSelected, DaysOfWeek[3].IsSelected, DaysOfWeek[4].IsSelected,
                    DaysOfWeek[5].IsSelected, DaysOfWeek[6].IsSelected);
                if (newSilenceOption.Error == null)
                {
                    Toast.Instance.Show<ToastDialog>(new
                        {Title = "Настройка добавлена", Duration = 500, ColorB = Color.Gray, ColorT = Color.White});
                    RefreshRulesCommand.Execute(null);
                    Dialog.Cancel();
                }
                else
                {
                    ShowError(newSilenceOption.Error);
                }
            }

            private void InitDays()
            {
                DaysOfWeek = new ObservableCollection<OptionModel>();
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ПН",
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ВТ",
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "СР",
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ЧТ",
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ПТ",
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "СБ",
                });
                DaysOfWeek.Add(new OptionModel
                {
                    Name = "ВС",
                });
            }

            private void SelectDayAction(object obj)
            {
                OptionModel optionModel = (OptionModel) obj;
                optionModel.IsSelected = !optionModel.IsSelected;
                IsEnabledSave = DaysOfWeek.Any(x => x.IsSelected);
            }
        }
    }
}