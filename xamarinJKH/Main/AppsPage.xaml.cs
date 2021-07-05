using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.Apps;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels.Main;
using AppPage = xamarinJKH.Tech.AppPage;

namespace xamarinJKH.Main
{
    /*!
\b Форма Работы с заявками по ПУ
*/
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppsPage : ContentPage
    {
        //public List<RequestInfo> RequestInfos { get; set; }
        public List<RequestInfo> RequestInfosAlive { get; set; }
        public List<RequestInfo> RequestInfosClose { get; set; }
        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        //private bool _isRefreshing = false;
        public Color hex { get; set; }
        public AppsPageViewModel viewModel { get; set; }

        
        //public bool IsRefreshing
        //{
        //    get { return _isRefreshing; }
        //    set
        //    {
        //        _isRefreshing = value;
        //        OnPropertyChanged(nameof(IsRefreshing));
        //    }
        //}

        //public ICommand RefreshCommand2
        //{
        //    get
        //    {
        //        return new Command(async () =>
        //        {
        //            IsRefreshing = true;

        //            await RefreshData();

        //            IsRefreshing = false;
        //        });
        //    }
            
        //}

        Task UpdateTask;
        /// <summary>
        /// Отображение выплывающего сообщения
        /// </summary>
        /// <param name="message">Текст</param>
        /// <param name="title">Заголовок</param>
        /// <param name="buttonText">Текст кнопки</param>
        /// <param name="afterHideCallback">Колбек функция</param>
        public async Task ShowMessage(string message,
            string title,
            string buttonText,
            Action afterHideCallback)
        {
            await DisplayAlert(
                title,
                message,
                buttonText);

            afterHideCallback?.Invoke();
        }

        bool showNoInetWindow = true;
        /// <summary>
        /// Инициализация автоматического обновления списка заявок
        /// </summary>
        void StartAutoUpdate()
        {
            CheckAkk();
            CancellationTokenSource = new CancellationTokenSource();
            this.CancellationToken = CancellationTokenSource.Token;
            UpdateTask = null;
            UpdateTask = new Task(async () =>
            {
                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (showNoInetWindow)
                            {
                                showNoInetWindow = false;

                                await ShowMessage(AppResources.ErrorNoInternet, AppResources.ErrorTitle, "OK", () =>
                                {
                                    showNoInetWindow = true;
                                });
                            }

                            await Task.Delay(TimeSpan.FromSeconds(5));
                        });
                    }
                    if(Settings.Person.Accounts.Count>0)
                    //Device.BeginInvokeOnMainThread(async () =>
                    //    {
                    //        await viewModel.UpdateTask().ContinueWith((obj )=> { Device.BeginInvokeOnMainThread(() => additionalList.ScrollTo(-1, null, ScrollToPosition.Start, false)); });                            
                    //    });
                    await viewModel.UpdateTask();
                return;
            });
            try
            {
                UpdateTask.Start();
            }
            catch(Exception ex)
            {
            }
        }
        /// <summary>
        /// Проверка подключенны ли ЛС
        /// </summary>
        private void CheckAkk()
        {
            if (Settings.Person.Accounts != null)
                if (Settings.Person.Accounts.Count > 0)
                {
                    StackLayoutNewApp.IsVisible = true;
                    StackLayoutIdent.IsVisible = false;
                }
                else
                {
                    StackLayoutNewApp.IsVisible = false;
                    StackLayoutIdent.IsVisible = true;
                }
        }

        static bool inUpdateNow = false;

        //public async Task RefreshData()
        //{
        //    try
        //    {
        //        if (inUpdateNow)
        //            return;
        //        inUpdateNow = true;

        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            aIndicator.IsVisible = true;
        //        });

        //        await getAppsAsync();
        //        inUpdateNow = false;
        //    }
        //    catch (Exception e)
        //    {
        //        inUpdateNow = false;
        //    }
        //    finally
        //    {
        //        inUpdateNow = false;
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            aIndicator.IsVisible = false;
        //        });
        //    }
        //}

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        public AppsPage()
        {
            InitializeComponent();
            FrameSwitch.BackgroundColor =
                System.Drawing.Color.FromArgb(200, System.Drawing.Color.White);
            Resources["hexColor"] = (Color)Application.Current.Resources["MainColor"];
            Analytics.TrackEvent("Заявки жителя");
            //Settings.MobileSettings.color = null;
            hex = Color.FromHex(!string.IsNullOrEmpty(Settings.MobileSettings.color)
                ? Settings.MobileSettings.color
                : "#FF0000");
            Analytics.TrackEvent("Заявки жителя - определили основной цвет");
            
            viewModel = new AppsPageViewModel();
            BindingContext = viewModel;
            

            messageRefreshView.Command = viewModel.LoadRequests;
            messageRefreshView.IsRefreshing = viewModel.IsRefreshing;


            Analytics.TrackEvent("Заявки жителя - создали/прибиндили модель");
            //aIndicator.Color = hex;

            NavigationPage.SetHasNavigationBar(this, false);
            MessagingCenter.Subscribe<Object>(this, "AutoUpdate", (sender) => { StartAutoUpdate(); });
            Analytics.TrackEvent("Заявки жителя - подписались на автообновление");

            //MessagingCenter.Subscribe<Object>(this, "StartRefresh", (sender) => 
            //{ Device.BeginInvokeOnMainThread(() => aIndicator.IsVisible = true); });
            Analytics.TrackEvent("Заявки жителя-подписались на обновление");

            MessagingCenter.Subscribe<Object>(this, "EndRefresh", (sender) => 
            { Device.BeginInvokeOnMainThread(() =>
            { 
                //aIndicator.IsVisible = false;
                messageRefreshView.IsRefreshing = false;
            }); 
            });
            Analytics.TrackEvent("Заявки жителя-подписались на окончание обновления");

            var goAddIdent = new TapGestureRecognizer();
            goAddIdent.Tapped += async (s, e) =>
            {
                /*await Dialog.Instance.ShowAsync<AddAccountDialogView>();*/
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddIdent) == null)
                    await Navigation.PushAsync(new AddIdent((PaysPage)Settings.paysPage));                
            };
            StackLayoutIdent.GestureRecognizers.Add(goAddIdent);
            Analytics.TrackEvent("Заявки жителя-добавили обработку тапа добавления счета");


            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    //хак чтобы список растягивался на все необходимое пространоство. а так
                    //есть баг в xamarin, потому что fillAndExpand не работает(https://github.com/xamarin/Xamarin.Forms/issues/6908)
                    additionalList.HeightRequest = 3000;

                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        LabelSwitch.FontSize = 12;
                        LabelSwitch2.FontSize = 12;
                    }

                    FrameBtnAdd.IsVisible = false;
                    //FrameBtnAddPass.IsVisible = false;
                    FrameBtnAddIos.IsVisible = true;
                    //FrameBtnAddPassIos.IsVisible = Settings.MobileSettings.enableCreationPassRequests;

                    break;
                case Device.Android:
                    FrameBtnAdd.IsVisible = true;
                    //FrameBtnAddPass.IsVisible = Settings.MobileSettings.enableCreationPassRequests;
                    FrameBtnAddIos.IsVisible = false;
                    //FrameBtnAddPassIos.IsVisible = false;

                    break;
                default:
                    break;
            }

            //FrameBtnAddPass.IsVisible = Settings.MobileSettings.enableCreationPassRequests;
            
// #if DEBUG
//             FrameBtnAddPass.IsVisible = true; 
// #endif

            //FrameSwitch.IsVisible = Settings.MobileSettings.enableCreationPassRequests;
            //LayoutSwitch.IsVisible = !Settings.MobileSettings.enableCreationPassRequests;
            

            Analytics.TrackEvent("Заявки жителя-платформозависимый код выполнен");

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            Analytics.TrackEvent("Заявки жителя-добавили обработку тапа Профиля");

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => 
            { 
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage()); 
            };
            LabelTech.GestureRecognizers.Add(techSend);
            Analytics.TrackEvent("Заявки жителя-добавили обработку тапа Техподдержки");


            var addClick = new TapGestureRecognizer();
            addClick.Tapped += async (s, e) => { startNewApp(FrameBtnAdd, null); };
            FrameBtnAdd.GestureRecognizers.Add(addClick);
            Analytics.TrackEvent("Заявки жителя-добавление обработки тапа кнопки Новая заявка андроид");
            var addClickIOS = new TapGestureRecognizer();
            addClickIOS.Tapped += async (s, e) => { startNewApp(FrameBtnAddIos, null); };
            FrameBtnAddIos.GestureRecognizers.Add(addClickIOS);
            var addIosPassClick = new TapGestureRecognizer();
            addIosPassClick.Tapped += async (s, e) => { StartNewPass(FrameBtnAddPass, null); };
            FrameBtnAddPass.GestureRecognizers.Add(addIosPassClick);
            // FrameBtnAddPassIos.GestureRecognizers.Add(addIosPassClick);
            Analytics.TrackEvent("Заявки жителя-добавление обработки тапа кнопки Новая заявка иос");

            SetText();
            Analytics.TrackEvent("Заявки жителя-SetText выполнено");
            //getApps();
            // additionalList.BackgroundColor = Color.Transparent;
            // additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            this.CancellationTokenSource = new CancellationTokenSource();
            //MessagingCenter.Subscribe<Object>(this, "UpdateAppCons", (sender) => RefreshData()); зачем тут обновлять заявки, при изменении заявок у "сотрудника"?
            //Analytics.TrackEvent("Заявки жителя-UpdateAppCons подписались");
            //Task.Run(async () => await RefreshData());

            MessagingCenter.Subscribe<Object, int>(this, "CloseAPP", async (sender, args) =>
            {
                //await RefreshData();
                viewModel.LoadRequests.Execute(null);
                //var request = RequestInfos?.Find(x => x.ID == args);
                var request = viewModel.Requests?.FirstOrDefault(x => x.ID == args);
                
                if (request != null)
                {
                    try
                    {
                        CancellationTokenSource.Cancel();
                        CancellationTokenSource.Dispose();
                    }
                    catch
                    {
                    }

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
                            await Navigation.PushAsync(new Apps.AppPage(request));
                    });
                }
            });
            Analytics.TrackEvent("Заявки жителя-CloseAPP подписались");
            try
            {
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    //await RefreshData();
                //    viewModel.LoadRequests.Execute(null);
                //});

                //viewModel.LoadRequests.Execute(null);
                viewModel.LoadRequests.Execute(null);
                Analytics.TrackEvent("Заявки жителя-LoadRequests");
            }
            catch (Exception exc)
            {
                Analytics.TrackEvent("Заявки жителя-LoadRequests. error:"+exc.ToString());
                throw;
            }

            SwitchApp.Toggled += SwitchApp_Toggled;
            SwitchApp2.Toggled += SwitchApp_Toggled;
            Analytics.TrackEvent("Заявки жителя-SwitchApp.Toggled подписались");

            MessagingCenter.Subscribe<Object>(this, "ChangeThemeCounter", (sender) =>
            {
                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                var colors = new Dictionary<string, string>();
                var arrowcolor = new Dictionary<string, string>();
                if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
                {
                    colors.Add("#000000", ((Color) Application.Current.Resources["MainColor"]).ToHex());
                    arrowcolor.Add("#000000", "#494949");
                }
                else
                {
                    colors.Add("#000000", "#FFFFFF");
                    arrowcolor.Add("#000000", "#FFFFFF");
                }

            });
            Analytics.TrackEvent("Заявки жителя-ChangeThemeCounter подписались");

            MessagingCenter.Subscribe<Object, int>(this, "OpenApp", async (sender, index) =>
            {
                await viewModel.UpdateTask();
                while (viewModel.AllRequests == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
                var request = viewModel.AllRequests.Where(x => x.ID == index).ToList();
                if (request != null && request.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
                            await Navigation.PushAsync(new Apps.AppPage(request[0],false, request[0].IsPaid));
                    });
                }
            });

            Analytics.TrackEvent("Заявки жителя-OpenApp подписались");

            MessagingCenter.Subscribe<Object, string>(this, "RemoveIdent", (sender, args) =>
           {
               if (args == null)
               {
                   try
                   {
                       if (viewModel.Requests != null && viewModel.Requests.Count > 0)
                           //viewModel.Requests.Clear(); 
                           viewModel.Requests = new System.Collections.ObjectModel.ObservableCollection<RequestInfo>();
                       if (viewModel.AllRequests != null && viewModel.AllRequests.Count > 0)
                           viewModel.AllRequests.Clear();
                   }
                   catch(Exception ex)
                   {
                       Console.WriteLine("не удалсоь очистить список заявок");
                   }
                   
               }
           });
            Analytics.TrackEvent("Заявки жителя-RemoveIdent подписались");
        }
        /// <summary>
        /// Показать выполненные заявки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchApp_Toggled(object sender, ToggledEventArgs e)
        {
            viewModel.ShowClosed = SwitchApp.IsToggled;
            //if (SwitchApp.IsToggled)
            //{
            //    RequestInfos = RequestInfosClose;
            //}
            //else
            //{
            //    RequestInfos = RequestInfosAlive;
            //}

            //additionalList.ItemsSource = null;
            //additionalList.ItemsSource = RequestInfos;
        }

        public AppsPage(string app_id) : base()
        {
            //var request = RequestInfos.Find(x => x.ID == int.Parse(app_id));
            var request = viewModel.Requests.FirstOrDefault(x => x.ID == int.Parse(app_id));
            if (request != null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
                        await Navigation.PushAsync(new Apps.AppPage(request));
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
            }
            catch
            {
            }
        }
        /// <summary>
        /// Обработка переоткрытия формы
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Settings.Person != null && Settings.Person.Accounts != null)
                FrameBtnAddPass.IsVisible = Settings.Person.Accounts.Any(_ => _.AllowPassRequestCreation);
            else
                FrameBtnAddPass.IsVisible = false;
            FrameSwitch.IsVisible = FrameBtnAddPass.IsVisible;
            LayoutSwitch.IsVisible = !FrameBtnAddPass.IsVisible;
            
            
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            var colors = new Dictionary<string, string>();
            var buttonColor = new Dictionary<string, string>();
            if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
            {
                colors.Add("#000000", ((Color)Application.Current.Resources["MainColor"]).ToHex());
                buttonColor.Add("#000000", "#FFFFFF");
            }
            else
            {
                colors.Add("#000000", "#FFFFFF");
                buttonColor.Add("#000000", "#FFFFFF");
            }

            IconViewSaldos.ReplaceStringMap = buttonColor;
            //if(!viewModel.IsRefreshing)
            //{
            //    Device.BeginInvokeOnMainThread(()=>
            //    {
            //        viewModel.IsRefreshing = false;
            //        messageRefreshView.IsRefreshing=false;
            //    }
            //    );
            //}
            Device.BeginInvokeOnMainThread(() => viewModel.LoadRequests.Execute(null));
             //viewModel.LoadRequests.Execute(null);
            CheckAkk();
            viewModel.IsChangeTheme = !viewModel.IsChangeTheme;
            viewModel.SetAppTheme();
        }
        /// <summary>
        /// Установка названия УК
        /// </summary>
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            

            SwitchApp.OnColor = hex;
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            // GoodsLayot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
        }

        //async Task getAppsAsync()
        //{
        //    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        //    {
        //        Device.BeginInvokeOnMainThread(async () =>
        //            await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
        //        return;
        //    }

        //    _requestList = await _server.GetRequestsList();
        //    if (_requestList.Error == null)
        //    {
        //        if (Settings.UpdateKey != _requestList.UpdateKey)
        //        {
        //            RequestInfos = null;
        //            setCloses(_requestList.Requests);
        //            Settings.UpdateKey = _requestList.UpdateKey;
        //            this.BindingContext = this;
        //        }
        //    }
        //    else
        //    {
        //        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsInfo, "OK");
        //    }
        //}

        //void setCloses(List<RequestInfo> infos)
        //{
        //    RequestInfosAlive = new List<RequestInfo>();
        //    RequestInfosClose = new List<RequestInfo>();
        //    foreach (var each in infos)
        //    {
        //        if (each.IsClosed)
        //        {
        //            RequestInfosClose.Add(each);
        //        }
        //        else
        //        {
        //            RequestInfosAlive.Add(each);
        //        }
        //    }

        //    if (SwitchApp.IsToggled)
        //    {
        //        RequestInfos = RequestInfosClose;
        //    }
        //    else
        //    {
        //        RequestInfos = RequestInfosAlive;
        //    }

        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        additionalList.ItemsSource = null;
        //        additionalList.ItemsSource = RequestInfos;
        //    });
        //}

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            RequestInfo select = e.Item as RequestInfo;
            if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
                await Navigation.PushAsync(new Apps.AppPage(select));
        }
        /// <summary>
        /// Открытие формы создания заявки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void startNewApp(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Person.Accounts.Count > 0)
                {
                    if (Settings.TypeApp.Count > 0)
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppPage) == null)
                            await Navigation.PushAsync(new NewAppPage());
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoTypes, "OK");
                    }
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoIdent, "OK");
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent(ex.Message);
            }
        }

        private async void AdditionalList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //RequestInfo select = viewModel.SelectedRequest;
            //if (select != null)
            //{
            //    if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
            //    {
            //        await Navigation.PushAsync(new Apps.AppPage(select, false, select.IsPaid));
            //        try
            //        {
            //            CollectionView container = (CollectionView) sender;
            //            IEnumerable<Element> enumerable = container.LogicalChildren.Where(x =>
            //                ((Label) (((StackLayout) x).Children[0])).Text == @select.ID.ToString());

            //            foreach (var element in enumerable)
            //            {
            //                StackLayout stackLayout = (StackLayout) element;
            //                PancakeView pancakeView = (PancakeView) stackLayout.Children[1];
            //                Grid grid = (Grid) pancakeView.Content;
            //                grid.Children[1].IsVisible = false;
            //            }
            //        }
            //        catch (Exception exception)
            //        {
            //            Console.WriteLine(exception);
            //        }
                   
            //    }
            //}
        }

        private async void FrameIdentGR_Tapped_1(object sender, EventArgs e)
        {
            Task.Delay(500);
            Grid grid = (Grid) sender; 
            grid.Children[1].IsVisible = false;
        }
        /// <summary>
        /// Обработка нажатия на элемент списка заявок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var s = (StackLayout)sender;
            var id = Convert.ToInt32(((Label)s.Children[0]).Text);

            //RequestInfo select = RequestInfos.FirstOrDefault(_ => _.ID == id); // viewModel.Requests.First(_=>_.ID==id);
            RequestInfo select = viewModel.Requests.FirstOrDefault(_=>_.ID==id);

            if (select != null)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
                {
                    await Navigation.PushAsync(new Apps.AppPage(select, false, select.IsPaid));
                    try
                    {
                        CollectionView container = (CollectionView)sender;
                        IEnumerable<Element> enumerable = container.LogicalChildren.Where(x =>
                            ((Label)(((StackLayout)x).Children[0])).Text == @select.ID.ToString());

                        foreach (var element in enumerable)
                        {
                            StackLayout stackLayout = (StackLayout)element;
                            PancakeView pancakeView = (PancakeView)stackLayout.Children[1];
                            Grid grid = (Grid)pancakeView.Content;
                            grid.Children[1].IsVisible = false;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                }
            }
        }
        /// <summary>
        /// Открытие формы создания заявки на пропуск
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartNewPass(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Person.Accounts.Count > 0)
                {
                    if (Settings.TypeApp.Count > 0)
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppPage) == null)
                            await Navigation.PushAsync(new NewAppPage(true));
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoTypes, "OK");
                    }
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoIdent, "OK");
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent(ex.Message);
            }
        }
    }
}