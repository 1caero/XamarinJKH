using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OSSMain : ContentPage
    {
        public bool forsvg { get; set; }
        public OSSMain()
        {
            InitializeComponent();
            Analytics.TrackEvent("Главная ОСС");
            forsvg = false;
            this.BindingContext = this;

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {    await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone)) 
                        phoneDialer.MakePhoneCall(Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    //BackgroundColor = Color.White;
                    OSSList.Padding = new Thickness(10,0);
                    var dw = DeviceDisplay.MainDisplayInfo.Width;
                    if(dw>800)
                        OSSList.Margin = new Thickness(-10, -80, -10, 0);
                    else
                        OSSList.Margin = new Thickness(-10, -60, -10, 0);

                    break;
                default:
                    break;
            }
            var dH = DeviceDisplay.MainDisplayInfo.Height;
            if (dH<1400)
            {
                titleLabel.FontSize = 18;
            }

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    _ = await Navigation.PopModalAsync();
                }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);

            UkName.Text = Settings.MobileSettings.main_name;
           

            ButtonActive.TextColor = colorFromMobileSettings;
            GetOssData(1);

            NavigationPage.SetHasNavigationBar(this, false);
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            OssTypeFrame.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#8d8d8d"));
            
        }

        readonly RestClientMP rc = new RestClientMP();

        IDictionary<Guid, Guid> frames = new Dictionary<Guid, Guid>();
        IDictionary<Guid, Guid> arrows = new Dictionary<Guid, Guid>();

        Color colorFromMobileSettings = (Color)Application.Current.Resources["MainColor"];

        Frame prevAddtionlExpanded=null;

        async void action(Frame frme, bool hide=false)
        {
            try
            {
                var idAdditional = frames[frme.Id];
                var additionalStack = ((StackLayout)frme.Content).Children.FirstOrDefault(_ => _.Id == idAdditional);
                if (hide)
                {
                    additionalStack.IsVisible = false;
                }
                else
                {
                    var additionalStackVisibility = additionalStack.IsVisible;
                    additionalStack.IsVisible = !additionalStackVisibility;
                }

                var idArrow = arrows[frme.Id];
                var arrow = ((StackLayout)((StackLayout)frme.Content).Children.First()).Children.FirstOrDefault(_ => _.Id == idArrow);
                if (additionalStack.IsVisible)
                    await arrow.RotateTo(270);
                else
                {
                    await arrow.RotateTo(90);
                }
            }
            catch { }
            
        }

        
        private async Task<bool> GetOssData(int type=0)
        {
            ButtonActive.IsEnabled = false;
            ButtonArchive.IsEnabled = false;

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return false;
            }
            //получаем данные от сервера по ОСС
            var result = await rc.GetOss();

            if(result.Error==null)
            {
                var dateNow = DateTime.Now;
                //Завершенные
                if (type == 0)
                {
                    var rr = result.Data.Where(_ => Convert.ToDateTime(_.DateEnd, new CultureInfo("ru-RU")) < dateNow ).OrderByDescending(_ => _.DateEnd).ToList();
                    result.Data = rr;
                }
                //Активные
                if(type==1)
                {
                    var rr = result.Data.Where(_ => Convert.ToDateTime(_.DateEnd, new CultureInfo("ru-RU")) > dateNow ).OrderByDescending(_=>_.DateEnd).ToList();
                    result.Data = rr;
                }

                frames.Clear();
                arrows.Clear();

                OSSListContent.Children.Clear();
                    
                    bool isFirst = true;
                    
                    foreach (var oss in result.Data)
                    {

                        Frame f = new Frame();
                        f.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"], Color.White);
                        f.MinimumHeightRequest = 50;
                        f.SetOnAppTheme(Frame.HasShadowProperty, false, true);
                        f.BackgroundColor = Color.White;

                        f.CornerRadius = Device.RuntimePlatform==Device.iOS? 20: 40;
                        f.HorizontalOptions = LayoutOptions.FillAndExpand;
                        f.Margin = new Thickness(0, 10);                        

                        //по нажатию обработка в раскрытие                        
                        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += async (s, e) =>
                        {
                            //делаем видимыми/невидимыми все доп.элементы
                            
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                var frme = (Frame)s;
                                if (prevAddtionlExpanded!=null)
                                {
                                    if (prevAddtionlExpanded.Id!=frme.Id)                                        
                                        action(prevAddtionlExpanded, true);
                                }
                                
                                action(frme);
                                prevAddtionlExpanded = frme;
                            });
                            };
                        f.GestureRecognizers.Add(tapGesture);

                        StackLayout rootStack = new StackLayout() { Orientation = StackOrientation.Vertical, VerticalOptions = LayoutOptions.FillAndExpand };

                        //добавление краткой инфо о собраниях
                        StackLayout stack = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand };

                        IconView iconViewStatus = new IconView();
                        iconViewStatus.Source =  "ic_OssCircleStatus";


                        int statusInt = 0;//зеленый
                        if(Convert.ToDateTime(oss.DateEnd, new CultureInfo("ru-RU")) < DateTime.Now)
                        {
                            iconViewStatus.Foreground= Color.FromHex("#ed2e37");
                            statusInt = 3; // красный, голосование окончено , статус "итоги голосования" и переход на эту страницу 
                        }
                        if (Convert.ToDateTime(oss.DateStart, new CultureInfo("ru-RU")) < DateTime.Now && Convert.ToDateTime(oss.DateEnd, new CultureInfo("ru-RU")) > DateTime.Now)
                        {
                            //статус - идет голосование
                            if (oss.IsComplete)
                            {
                                // Ваш голос учтен - страница личных результатов голосования
                                statusInt = 2;//желтый(стоит сейчас) или какой еще цвет?
                                iconViewStatus.Foreground = Color.FromHex("#50ac2f");
                            }
                            else
                            {
                                //если не все проголосовано, переходим на вопрос начиная с которго нет ответов, и продолжаем голосование
                                iconViewStatus.Foreground = Color.FromHex("#ff971c");
                                statusInt = 1;//желтый
                            }
                            
                        }
                        if (Convert.ToDateTime(oss.DateStart, new CultureInfo("ru-RU")) > DateTime.Now)
                        {
                            //зеленый, "Уведомление о проведении ОСС", statusInt = 0
                            iconViewStatus.Foreground = Color.FromHex("#50ac2f");
                        }
                       

                        iconViewStatus.HeightRequest = 15;
                        iconViewStatus.WidthRequest = 15;
                        iconViewStatus.VerticalOptions = LayoutOptions.Center;
                        stack.Children.Add(iconViewStatus);
                        
                        Label MeetingTitle = new Label() { Text = oss.MeetingTitle, FontSize = 18 ,TextColor=Color.Black, FontAttributes=FontAttributes.Bold, HorizontalOptions=LayoutOptions.FillAndExpand};
                        stack.Children.Add(MeetingTitle);

                        IconView iconViewArrow = new IconView();
                        iconViewArrow.Source = "ic_arrow_forward";
                        iconViewArrow.Foreground = colorFromMobileSettings;
                        await iconViewArrow.RotateTo(90);
                        iconViewArrow.HeightRequest = 15;
                        iconViewArrow.WidthRequest = 15;
                        iconViewArrow.VerticalOptions = LayoutOptions.Center;
                        iconViewArrow.HorizontalOptions = LayoutOptions.End;

                        stack.Children.Add(iconViewArrow);
                        
                        arrows.Add(f.Id, iconViewArrow.Id);

                        //добавляем видимю часть на старте
                        rootStack.Children.Add(stack);

                        //невидимые на старте элементы фрейма, кроме 1го фрейма                                              
                        StackLayout additionslData = new StackLayout() { IsVisible = false , Orientation= StackOrientation.Vertical};
                        if (isFirst)
                        {
                            prevAddtionlExpanded = f;
                            isFirst = false;
                        }

                        //добавляем в список связку стека с невидимыми плями и родительского фрейма
                        frames.Add(f.Id, additionslData.Id);

                        //инициатор
                        Label initiator = new Label() {};
                        FormattedString text = new FormattedString();
                        Span t1 = new Span() { TextColor =Color.FromHex("#545454"), FontSize=14, Text=$"{AppResources.OSSInfoInitiator}: "};
                        text.Spans.Add(t1);
                        Span t2 = new Span() { TextColor = Color.Black, FontSize = 14, Text = oss.InitiatorNames };
                        text.Spans.Add(t2);
                        initiator.FormattedText = text;

                        additionslData.Children.Add(initiator);
                        //дата собрания
                        Label date = new Label() {  };
                        FormattedString datetext = new FormattedString();
                        Span datet1 = new Span() { TextColor = Color.FromHex("#545454"), FontSize = 14, Text = $"{AppResources.OSSInfoDate}: " };
                        datetext.Spans.Add(datet1);
                        Span datet2 = new Span() { TextColor = Color.Black, FontSize = 14, Text = $"{oss.DateStart} {AppResources.To} {oss.DateEnd}" };
                        datetext.Spans.Add(datet2);
                        date.FormattedText = datetext;

                        additionslData.Children.Add(date);

                        //Адрес дома
                        Label adress = new Label() {  };
                        FormattedString adresstext = new FormattedString();
                        Span adresst1 = new Span() { TextColor = Color.FromHex("#545454"), FontSize = 14, Text = $"{AppResources.OSSInfoAdress}: " };
                        adresstext.Spans.Add(adresst1);
                        Span adresst2 = new Span() { TextColor = Color.Black, FontSize = 14, Text = $"{oss.HouseAddress}" };
                        adresstext.Spans.Add(adresst2);
                        adress.FormattedText = adresstext;

                        additionslData.Children.Add(adress);

                        //форма проведения
                        Label formAction = new Label() {  };
                        FormattedString formActiontext = new FormattedString();
                        Span formActiont1 = new Span() { TextColor = Color.FromHex("#545454"), FontSize = 14, Text = $"{AppResources.OSSInfoForm}: " };
                        formActiontext.Spans.Add(formActiont1);
                        Span formActiont2 = new Span() { TextColor = Color.Black, FontSize = 14, Text = $"{oss.Form}" };
                        formActiontext.Spans.Add(formActiont2);
                        formAction.FormattedText = formActiontext;

                        additionslData.Children.Add(formAction);

                        //разделитель
                        BoxView delim = new BoxView() { BackgroundColor = Color.FromHex("#545454"), 
                            HorizontalOptions = LayoutOptions.FillAndExpand, HeightRequest = 1, Margin=new Thickness(0,10) };
                        additionslData.Children.Add(delim);

                        //статус собрания:
                        StackLayout status = new StackLayout() { Orientation = StackOrientation.Horizontal , HorizontalOptions=LayoutOptions.FillAndExpand};
                        
                        StackLayout statusNameIcon = new StackLayout() { Orientation = StackOrientation.Vertical, HorizontalOptions = LayoutOptions.FillAndExpand };
                        
                        Label statusName = new Label() { Text = $"{AppResources.OSSInfoStatus} ", FontAttributes = FontAttributes.Bold, FontSize = 14 , TextColor=Color.Black,                             
                            HorizontalOptions = LayoutOptions.Start};

                        statusNameIcon.Children.Add(statusName);

                        StackLayout coloredStatus = new StackLayout() { HorizontalOptions=LayoutOptions.Start, Orientation=StackOrientation.Horizontal};
                        IconView iconViewStatusNameIcon = new IconView();

                        string textStatius = "";
                        Color ColorStatusTextString;
                        if (statusInt == 0)
                        {
                            iconViewStatusNameIcon.Source = "ic_status_green";
                            iconViewStatusNameIcon.Foreground = Color.FromHex("#50ac2f");
                            ColorStatusTextString = Color.FromHex("#50ac2f");
                            textStatius = AppResources.OSSInfoNotif;
                        }
                        else if (statusInt == 1) 
                        {
                            iconViewStatusNameIcon.Source = "ic_status_yellow";
                            iconViewStatusNameIcon.Foreground = Color.FromHex("#ff971c");
                            ColorStatusTextString = Color.FromHex("#ff971c");
                            textStatius = AppResources.OSSInfoVoting;
                        }
                        else if(statusInt == 2)
                        {
                            iconViewStatusNameIcon.Source = "ic_status_done";
                            iconViewStatusNameIcon.Foreground = Color.FromHex("#50ac2f");
                            ColorStatusTextString = Color.FromHex("#50ac2f");
                            textStatius = AppResources.OSSVoteChecked;
                        }
                        else //3
                        {
                            iconViewStatusNameIcon.Source = "ic_status_red";
                            iconViewStatusNameIcon.Foreground = Color.FromHex("#ed2e37");
                            ColorStatusTextString = Color.FromHex("#ed2e37");
                            textStatius = AppResources.OSSInfoPassed;
                        }                                            
                        iconViewStatusNameIcon.HeightRequest = 15;
                        iconViewStatusNameIcon.WidthRequest = 15;
                        iconViewStatusNameIcon.VerticalOptions = LayoutOptions.Center;
                        iconViewStatusNameIcon.HorizontalOptions = LayoutOptions.Start;

                        coloredStatus.Children.Add(iconViewStatusNameIcon);

                        Label statSting = new Label() { Text = textStatius, FontSize=12, TextColor = ColorStatusTextString, HorizontalOptions=LayoutOptions.Start };
                        coloredStatus.Children.Add(statSting);
                        
                        statusNameIcon.Children.Add(coloredStatus);

                        status.Children.Add(statusNameIcon);

                        //кнопка раскрытия осс
                        Frame buttonFrame = new Frame() { HeightRequest = 40, WidthRequest = 40, BackgroundColor = colorFromMobileSettings, CornerRadius=10 , Padding=0};
                        IconView iconViewArrowButton = new IconView();
                        iconViewArrowButton.Source = "ic_arrow_forward";
                        iconViewArrowButton.Foreground = Color.White;
                        iconViewArrowButton.HeightRequest = 15;
                        iconViewArrowButton.WidthRequest = 15;
                        iconViewArrowButton.VerticalOptions = LayoutOptions.Center;
                        iconViewArrowButton.HorizontalOptions = LayoutOptions.Center;
                        buttonFrame.SetOnAppTheme(Frame.HasShadowProperty, false, true);
                        buttonFrame.Content = iconViewArrowButton;

                        TapGestureRecognizer buttonFrametapGesture = new TapGestureRecognizer();
                        buttonFrametapGesture.Tapped += async (s, e) =>
                        {
                            OSS result = await rc.GetOssById(oss.ID.ToString());
                            var stack = this.Navigation.NavigationStack;
                            var modal = this.Navigation.ModalStack;
                            var pages = new List<Page>();
                            pages.AddRange(modal);
                            pages.AddRange(stack);
                            switch (statusInt){
                                case 0: 
                                case 1:
                                   var setAcquintedResult = await rc.SetAcquainted(oss.ID);
                                    if(string.IsNullOrWhiteSpace( setAcquintedResult.Error) )
                                    {
                                        if (pages.FirstOrDefault(x => x is OSSInfo) == null)
                                            OpenPage(new OSSInfo(result));
                                    }
                                    else
                                    {
                                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorOSSMain, "OK");

                                    }

                                    break;
                                case 3: //"Итоги голосования"/"завершено" - открываем форму общих результатов голосования
                                    if (pages.FirstOrDefault(x => x is OSSTotalVotingResult) == null)
                                        OpenPage(new OSSTotalVotingResult(result));

                                    break;
                                case 2: //"Ваш голос учтен"  - открываем форму личных результатов голосования
                                    if (pages.FirstOrDefault(x => x is OSSPersonalVotingResult) == null)
                                        OpenPage(new OSSPersonalVotingResult(result));

                                    break;
                                default:
                                    if (pages.FirstOrDefault(x => x is OSSInfo) == null)
                                        OpenPage(new OSSInfo(result));
                                    return;
                            } 
                        };
                        buttonFrame.GestureRecognizers.Add(buttonFrametapGesture);

                        status.Children.Add(buttonFrame);
                        
                        
                        additionslData.Children.Add(status);

                        //добавляем "невидимую" часть
                        rootStack.Children.Add(additionslData);

                        f.Content = rootStack;


                        OSSListContent.Children.Add(f);
                    }
                action(prevAddtionlExpanded);

                ButtonActive.IsEnabled = true;
                ButtonArchive.IsEnabled = true;
                //});
                return true;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorOSSMainInfo, "OK");
                return false;
            }

            
        }

        private async void Button_Clicked(object sender, EventArgs e)
        { 
            //активные
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {

                    frames.Clear();
                    arrows.Clear();

                    var r = await GetOssData(1);


                    ((Button)sender).TextColor = colorFromMobileSettings;
                    ButtonArchive.SetAppThemeColor(Button.TextColorProperty, Color.Black, Color.White);

                   
                });
            }
            catch(Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorOSSMainInfo, "OK");
            }
            
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {

                    frames.Clear();
                    arrows.Clear();

                    var r =  await GetOssData(0);


                    ((Button)sender).TextColor = colorFromMobileSettings;
                    ButtonActive.SetAppThemeColor(Button.TextColorProperty, Color.Black, Color.White);
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorOSSMainInfo, "OK");
            }
        }

        async void OpenPage(Page page)
        {
            try
            {
                await Navigation.PushAsync(page);
            }
            catch
            {
                await Navigation.PushModalAsync(page);
            }
        }
        
        
    }
}