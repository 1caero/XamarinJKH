using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using RestSharp;
using Syncfusion.SfAutoComplete.XForms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;
using SelectionChangedEventArgs = Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs;


namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppFilterDialog : DialogView
    {
        private FilterModel _filterModel = null;
        private RestClientMP server = new RestClientMP();
        public const string REQUEST_NUMBER = "RequestNumber"; //номер заявки (значение передается в поле Text)

        public const string
            REQUEST_STATUS_ID = "RequestStatusId"; // ID статуса заявки (значение передается в поле ItemID)

        public const string
            REQUEST_SUB_TYPE_ID = "RequestSubTypeId"; // ID типа неисправности (значение передается в поле ItemID)

        public const string REQUEST_TYPE_ID = "RequestTypeId"; // ID типа заявки (значение передается в поле ItemID)

        public const string
            REQUEST_PRIORITY_ID = "RequestPriorityId"; //  ID приоритета заявки (значение передается в поле ItemID)

        private Dictionary<string, int> Statuses = new Dictionary<string, int>();
        private Page _page;

        public AppFilterDialog(Page page)
        {
            _page = page;
            InitializeComponent();
            LayoutLoading.BackgroundColor = System.Drawing.Color.FromArgb(150, System.Drawing.Color.White);
            View.WidthRequest = App.ScreenWidth;
            var close = new TapGestureRecognizer();
            close.Tapped += (s, e) => { DialogNotifier.Cancel(); };
            IconViewClose.GestureRecognizers.Add(close);
            var apply = new TapGestureRecognizer();
            apply.Tapped += (s, e) => { Apply(); };
            FrameBtnApply.GestureRecognizers.Add(apply);
            var reset = new TapGestureRecognizer();
            reset.Tapped += (s, e) => { Reset(); };
            FrameBtnReset.GestureRecognizers.Add(reset);
            BindingContext = _filterModel =
                new FilterModel(AutoCompleteType, AutoCompletePodType, AutoCompletePrioritets);
            var status = new TapGestureRecognizer();
            status.Tapped += async (s, e) =>
            {
                string[] param = null;
                SetListStatuses(Settings.StatusApp, ref param);
                var action =
                    await page.DisplayActionSheet(AppResources.StatusFilterApp, AppResources.Cancel, null, param);
                if (action != null && !action.Equals(AppResources.Cancel))
                {
                    _filterModel.StatusId = Statuses[action];
                    LabelStatus.Text = action;
                    ImageStatus.IsVisible = true;
                    ImageStatus.Source = "resource://xamarinJKH.Resources." + Settings.GetStatusIcon(Statuses[action]) +
                                         ".svg";
                }
            };
            LayoutStatus.GestureRecognizers.Add(status);

            // if (Settings.StatusApp != null)
            // {
            //     LabelStatus.Text = Settings.StatusApp?[0].Name;
            //     ImageStatus.Source = "resource://xamarinJKH.Resources." +
            //                          Settings.GetStatusIcon(Settings.StatusApp[0].ID) +
            //                          ".svg";
            // }

            SetFilters();
        }

        void Reset()
        {
            MessagingCenter.Send<object>(this, "RemooveFilter");
            Preferences.Remove(REQUEST_NUMBER);
            Preferences.Remove(REQUEST_STATUS_ID);
            Preferences.Remove(REQUEST_TYPE_ID);
            Preferences.Remove(REQUEST_SUB_TYPE_ID);
            Preferences.Remove(REQUEST_PRIORITY_ID);
            DialogNotifier.Cancel();
        }
        
        async void Apply()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MainContainer.IsEnabled = false;
                LayoutLoading.IsVisible = true;
            });

            Preferences.Remove(REQUEST_NUMBER);
            Preferences.Remove(REQUEST_STATUS_ID);
            Preferences.Remove(REQUEST_TYPE_ID);
            Preferences.Remove(REQUEST_SUB_TYPE_ID);
            Preferences.Remove(REQUEST_PRIORITY_ID);

            List<CustomSearchCriteria> Criterias = new List<CustomSearchCriteria>();
            string numberApp = EntryNumberApp.Text;
            if (!string.IsNullOrWhiteSpace(numberApp))
            {
                Criterias.Add(new CustomSearchCriteria
                {
                    SearchCriteriaType = REQUEST_NUMBER,
                    Text = numberApp
                });
                Preferences.Set(REQUEST_NUMBER, numberApp);
            }

            if (!string.IsNullOrWhiteSpace(LabelStatus.Text))
            {
                Criterias.Add(new CustomSearchCriteria
                {
                    SearchCriteriaType = REQUEST_STATUS_ID,
                    ItemID = _filterModel.StatusId
                });
                if (_filterModel.StatusId != null) Preferences.Set(REQUEST_STATUS_ID, (int) _filterModel.StatusId);
            }

            try
            {
                ObservableCollection<object> observableCollectionType =
                    (ObservableCollection<object>) AutoCompleteType?.SelectedItem;
                if (observableCollectionType != null && observableCollectionType.Any())
                {
                    string ids = "";
                    foreach (RequestType each in observableCollectionType)
                    {
                        Criterias.Add(new CustomSearchCriteria
                        {
                            SearchCriteriaType = REQUEST_TYPE_ID,
                            ItemID = each.ID
                        });
                        ids += each.ID + ";";
                    }

                    Preferences.Set(REQUEST_TYPE_ID, ids);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                ObservableCollection<object> observableCollection =
                    (ObservableCollection<object>) AutoCompletePodType?.SelectedItem;
                if (observableCollection != null && observableCollection.Any())
                {
                    string ids = "";
                    foreach (NamedValue each in observableCollection)
                    {
                        Criterias.Add(new CustomSearchCriteria
                        {
                            SearchCriteriaType = REQUEST_SUB_TYPE_ID,
                            ItemID = each.ID
                        });
                        ids += each.ID + ";";
                    }

                    Preferences.Set(REQUEST_SUB_TYPE_ID, ids);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                ObservableCollection<object> observableCollectionPriority =
                    (ObservableCollection<object>) AutoCompletePrioritets?.SelectedItem;
                if (observableCollectionPriority != null && observableCollectionPriority.Any())
                {
                    string ids = "";
                    foreach (NamedValue each in observableCollectionPriority)
                    {
                        Criterias.Add(new CustomSearchCriteria
                        {
                            SearchCriteriaType = REQUEST_PRIORITY_ID,
                            ItemID = each.ID
                        });
                        ids += each.ID + ";";
                    }

                    Preferences.Set(REQUEST_PRIORITY_ID, ids);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            List<RequestInfo> requestInfos = await server.Search(Criterias);
            if (requestInfos != null && requestInfos.Count > 0)
            {
                MessagingCenter.Send<object, List<RequestInfo>>(this, "SetFilter", requestInfos);
                DialogNotifier.Cancel();
            }
            else
            {
                await _page.DisplayAlert(AppResources.ErrorTitle, AppResources.FilterError, "OK");
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                MainContainer.IsEnabled = true;
                LayoutLoading.IsVisible = false;
            });
        }

        void SetFilters()
        {
            string numberApp = Preferences.Get(REQUEST_NUMBER, "");
            if (!string.IsNullOrWhiteSpace(numberApp))
            {
                EntryNumberApp.Text = numberApp;
            }

            int statusId = Preferences.Get(REQUEST_STATUS_ID, -1);
            if (statusId != -1)
            {
                if (Settings.StatusApp != null)
                {
                    LabelStatus.Text = Settings.StatusApp?.FirstOrDefault(x => x.ID == statusId)?.Name;
                    ImageStatus.IsVisible = true;
                    ImageStatus.Source = "resource://xamarinJKH.Resources." +
                                         Settings.GetStatusIcon(statusId) +
                                         ".svg";
                    _filterModel.StatusId = statusId;
                }
            }
        }

        void SetListStatuses(List<NamedValue> groups, ref string[] param)
        {
            Statuses = new Dictionary<string, int>();
            param = new string [groups.Count];
            int i = 0;
            foreach (var each in groups)
            {
                try
                {
                    if (each.Name != null)
                    {
                        Statuses.Add(each.Name, each.ID);
                        param[i] = each.Name;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                i++;
            }
        }

        public class FilterModel : BaseViewModel
        {
            public ObservableCollection<RequestType> Types { get; set; } = new ObservableCollection<RequestType>();
            public ObservableCollection<NamedValue> PodTypes { get; set; } = new ObservableCollection<NamedValue>();
            public ObservableCollection<NamedValue> Prioritets { get; set; } = new ObservableCollection<NamedValue>();
            private List<int> _selectedIndexType;

            public List<int> SelectedIndexType
            {
                get => _selectedIndexType;
                set
                {
                    _selectedIndexType = value;
                    OnPropertyChanged("SelectedIndexType");
                }
            }

            OptionModel selectedTyp;

            public OptionModel SelectedTyp
            {
                get => selectedTyp;
                set
                {
                    selectedTyp = value;
                    OnPropertyChanged("SelectedTyp");
                }
            }

            public int? StatusId { get; set; } = null;
            public Command SelectTyp { get; set; }
            private SfAutoComplete _sfAutoCompleteType;

            public FilterModel(SfAutoComplete sfAutoCompleteType, SfAutoComplete sfAutoCompletePodType,
                SfAutoComplete sfAutoCompletePriority)
            {
                string TypesIds = Preferences.Get(REQUEST_TYPE_ID, "");
                string PodTypesIds = Preferences.Get(REQUEST_SUB_TYPE_ID, "");
                string PriorityIds = Preferences.Get(REQUEST_PRIORITY_ID, "");

                ObservableCollection<RequestType> requestTypes = new ObservableCollection<RequestType>();
                ObservableCollection<NamedValue> requestPodTypes = new ObservableCollection<NamedValue>();
                ObservableCollection<NamedValue> requestPriority = new ObservableCollection<NamedValue>();

                List<string> ids = new List<string>();
                List<string> idsPod = new List<string>();
                List<string> idsPriority = new List<string>();

                if (!string.IsNullOrWhiteSpace(TypesIds))
                {
                    ids = new List<string>(TypesIds.Split(";"));
                }

                if (!string.IsNullOrWhiteSpace(PodTypesIds))
                {
                    idsPod = new List<string>(PodTypesIds.Split(";"));
                }

                if (!string.IsNullOrWhiteSpace(PriorityIds))
                {
                    idsPriority = new List<string>(PriorityIds.Split(";"));
                }


                foreach (var type in Settings.TypeApp)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (type.HasSubTypes)
                        {
                            foreach (var each in type.SubTypes)
                            {
                                if (idsPod.Contains(each.ID.ToString()))
                                    requestPodTypes.Add(each);
                                PodTypes.Add(each);
                            }
                        }

                        if (ids.Contains(type.ID.ToString()))
                        {
                            requestTypes.Add(type);
                        }

                        Types.Add(type);
                    });
                }


                foreach (var each in Settings.PrioritetsApp)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (idsPriority.Contains(each.ID.ToString()))
                            requestPriority.Add(each);
                        Prioritets.Add(each);
                    });
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    sfAutoCompleteType.ShowSuggestionsOnFocus = false;
                    sfAutoCompletePodType.ShowSuggestionsOnFocus = false;
                    sfAutoCompletePriority.ShowSuggestionsOnFocus = false;
                    sfAutoCompleteType.SelectedItem = requestTypes;
                    sfAutoCompletePodType.SelectedItem = requestPodTypes;
                    sfAutoCompletePriority.SelectedItem = requestPriority;
                    
                });

                Device.StartTimer (new TimeSpan (0, 0, 1), () =>
                {
                    
                    Device.BeginInvokeOnMainThread (() => 
                    {
                        sfAutoCompleteType.ShowSuggestionsOnFocus = true;
                        sfAutoCompletePodType.ShowSuggestionsOnFocus = true;
                        sfAutoCompletePriority.ShowSuggestionsOnFocus = true;
                    });
                    return false; // runs again, or false to stop
                });

                SelectTyp = new Command<object>(name =>
                {
                    var selected = SelectedTyp;

                    if (selected != null)
                    {
                        if (selected.IsSelected)
                        {
                            selected.Selected = false;
                            selected.IsSelected = false;
                            string replaceColor = Application.Current.RequestedTheme == OSAppTheme.Dark
                                ? "#FFFFFF"
                                : "#8D8D8D";
                            selected.SelectedColor = Color.FromHex(replaceColor);
                            selected.ReplaceMap = new Dictionary<string, string> {{"#000000", replaceColor}};
                        }
                        else
                        {
                            selected.IsSelected = true;
                            selected.Selected = true;
                            selected.SelectedColor = Color.FromHex(Settings.MobileSettings.color);
                            selected.ReplaceMap = new Dictionary<string, string>
                                {{"#000000", "#" + Settings.MobileSettings.color}};
                        }
                    }
                });
            }

            public void SetTypes(ObservableCollection<RequestType> requestTypes)
            {
                Device.BeginInvokeOnMainThread(() => { _sfAutoCompleteType.SelectedItem = requestTypes; });
            }

            private static Dictionary<string, string> SetIconType(String Name, ref string Image)
            {
                switch (Name.ToLower())
                {
                    case "бухгалтерия":
                        Image = "resource://xamarinJKH.Resources.app_accountaint.svg";
                        break;
                    case "паспортный стол":
                        Image = "resource://xamarinJKH.Resources.app_passport.svg"; //vector 3
                        break;
                    case "сантехник":
                        Image = "resource://xamarinJKH.Resources.app_plumber.svg"; //vector 1
                        break;
                    case "электрик":
                        Image = "resource://xamarinJKH.Resources.app_electritian.svg"; //vector 2
                        break;
                    case "другие вопросы":
                        Image = "resource://xamarinJKH.Resources.app_other.svg"; //vector 5
                        break;
                    case "домофон":
                        Image = "resource://xamarinJKH.Resources.app_domophone.svg";
                        break;
                    case "заявка на пропуск":
                        Image = "resource://xamarinJKH.Resources.app_pass.svg"; //vector 4
                        break;
                    default:
                        Image = "resource://xamarinJKH.Resources.ic_gears.svg"; //vector 4
                        break;
                }

                string replaceColor = Application.Current.RequestedTheme == OSAppTheme.Dark ? "#FFFFFF" : "#8D8D8D";
                return new Dictionary<string, string> {{"#000000", replaceColor}};
            }
        }

        StackLayout lastElementSelected2;

        private void FrameIdentGR_Tapped(object sender, EventArgs e)
        {
            var el = sender as StackLayout;

            var om = el.BindingContext as OptionModel;
            var vm = (BindingContext as FilterModel);
            vm.SelectedTyp = null;
            vm.SelectedTyp = om;
            lastElementSelected2 = (StackLayout) sender;
        }

        private void AutoCompletePodType_OnFocused(object sender, FocusEventArgs e)
        {
        }

        private void AutoCompletePodType_OnFocusChanged(object sender, FocusChangedEventArgs e)
        {
        }

        private void AutoCompletePodType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            Apply();
        }

        private void ButtonReset_OnClicked(object sender, EventArgs e)
        {
            Reset();
        }

        private void AutoCompletePrioritets_OnFocusChanged(object sender, FocusChangedEventArgs e)
        {
          
            
        }
    }
}