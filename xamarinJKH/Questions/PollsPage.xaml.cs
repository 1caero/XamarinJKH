using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Questions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PollsPage : ContentPage
    {
        private PollInfo _pollInfo;
        private readonly bool _isComplite;
        private List<StackLayout> _contentQuest = new List<StackLayout>();
        private RestClientMP server = new RestClientMP();
        private int quest = 0;
        private bool isCheched = false;
        private List<Label> indicators = new List<Label>();

        private PollingResult _pollingResult = new PollingResult();
        
        public string IsRefreshing { get; set; }
        public PollsPage(PollInfo pollInfo, bool isComplite)
        {
            _pollInfo = pollInfo;
            _isComplite = isComplite;
            InitializeComponent();
            Analytics.TrackEvent("Вопросы по опросу " + pollInfo.ID);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake2.HeightRequest = statusBarHeight;

                    if (DeviceDisplay.MainDisplayInfo.Width<700)
                    {
                        FrameBack.Padding = new Thickness(15, 12);
                        FrameBtnFinish.Padding = new Thickness(10, 12);
                    }

                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var nextClick = new TapGestureRecognizer();
            nextClick.Tapped += async (s, e) => { NextQuest(); };
            FrameBtnNext.GestureRecognizers.Add(nextClick);
            var backClickQuest = new TapGestureRecognizer();
            backClickQuest.Tapped += async (s, e) => { BackQuest(); };
            FrameBack.GestureRecognizers.Add(backClickQuest);
            var finishClick = new TapGestureRecognizer();
            finishClick.Tapped += async (s, e) => { FinishClick(); };
            FrameBtnFinish.GestureRecognizers.Add(finishClick);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {    await Navigation.PushAsync(new AppPage());};
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
            LabelPhone.GestureRecognizers.Add(call);
            SetText();
            setQuest();
            if (_contentQuest.Count > 0)
            {
                setQuestVisible();
                ChechQuestions();
                setVisibleButton();
                setIndicator();
            }
            else
            {
                FrameBack.IsVisible = false;
            }

            if (_isComplite)
            {
                Container.IsEnabled = false;
            }

            BindingContext = this;
            if (!pollInfo.IsReaded)
                Task.Run(async () =>
                {
                    var result = await server.SetPollReadFlag(pollInfo.ID);
                    MessagingCenter.Send<Object, int>(this, "SetEventsAmount", -1);
                    MessagingCenter.Send<Object>(this, "ReducePolls");
                });
        }

        private async void FinishClick()
        {
            CommonResult result = await server.SaveResultPolls(_pollingResult);
            if (result.Error == null)
            {
                await DisplayAlert(AppResources.AlertSuccess, AppResources.SuccessOSSPollPass, "OK");
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, $"{AppResources.ErrorOSSPollPass}\n" + result.Error, "OK");
            }
        }

        private void ChechQuestions()
        {
            if (quest + 1 == _pollInfo.Questions.Count)
            {
                FrameBtnNext.IsVisible = false;
                FrameBtnFinish.IsVisible = true;
                if (quest > 0)
                {
                    FrameBack.IsVisible = true;
                }
                else
                {
                    FrameBack.IsVisible = false;
                }
            }
            else
            {
                FrameBtnNext.IsVisible = true;
                FrameBtnFinish.IsVisible = false;

                if (quest > 0)
                {
                    FrameBack.IsVisible = true;
                }
                else
                {
                    FrameBack.IsVisible = false;
                }
            }
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            if (!string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
            {
                LabelPhone.Text = "+" + Settings.Person.companyPhone.Replace("+", "");
            }
            else
            {
                IconViewLogin.IsVisible = false;
                LabelPhone.IsVisible = false;
            }
            LabelTitle.Text = _pollInfo.Name;

            FrameBtnNext.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            FrameBtnFinish.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.Black);
            Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);if (Device.RuntimePlatform == Device.iOS){ if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo"){PancakeViewIcon.Padding = new Thickness(0);}}
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.Black);
            Color unselect = hexColor.AddLuminosity(0.3);
            FrameBack.SetAppThemeColor(BackgroundColorProperty, unselect, Color.FromHex("#4A4A4A"));
            StackLayoutIndicator.SetAppThemeColor(BackgroundColorProperty, unselect, Color.White);
        }

        void setQuest()
        {
            int i = 1;
            _pollingResult.Answers = new List<PollAnswer>();
            _pollingResult.PollId = _pollInfo.ID;
            _pollingResult.ExtraInfo = "";
            foreach (var each in _pollInfo.Questions)
            {
                PollAnswer pollAnswer = new PollAnswer()
                {
                    QuestionId = each.ID,
                    AnswerId = -1
                };
                _pollingResult.Answers.Add(pollAnswer);
                StackLayout containerPolss = new StackLayout();
                Label questions = new Label();
                FormattedString formattedString = new FormattedString();
                formattedString.Spans.Add(new Span
                {
                    Text = "Вопрос " + i + "/" + _pollInfo.Questions.Count + ".",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Black,
                    FontSize = 17
                });
                formattedString.Spans.Add(new Span
                {
                    Text = " " + each.Text,
                    TextColor = Color.Black,
                    FontSize = 17
                });

                questions.FormattedText = formattedString;
                containerPolss.Children.Add(questions);
                StackLayout radio = new StackLayout();
                foreach (var jAnswer in each.Answers)
                {
                    StackLayout horizont = new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal
                    };
                    RadioButton radioButton = new RadioButton
                    {
                        GroupName = "poolsGroup",
                        IsChecked = jAnswer.IsUserAnswer,
                        BackgroundColor = Color.Transparent
                    };
                    Label textRadio = new Label()
                    {
                        Text = jAnswer.Text,
                        TextColor = Color.Black,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var tClick = new TapGestureRecognizer();
                    tClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => radioButton.IsChecked = true); };
                    textRadio.GestureRecognizers.Add(tClick);

                    isCheched = jAnswer.IsUserAnswer;
                    if (jAnswer.IsUserAnswer)
                    {
                        textRadio.TextColor = (Color)Application.Current.Resources["MainColor"];
                    }

                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            radioButton.BorderColor = (Color)Application.Current.Resources["MainColor"];
                            break;
                        case Device.Android:
                            radioButton.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
                            break;
                    }

                    radioButton.BorderColor = Color.Red;
                    radioButton.Margin = new Thickness(-5, 0, 0, 0);
                    radioButton.CheckedChanged += (sender, e) =>
                    {
                        isCheched = true;
                        if (radioButton.IsChecked)
                        {
                            pollAnswer.AnswerId = jAnswer.ID;
                            textRadio.TextColor = (Color)Application.Current.Resources["MainColor"];
                        }
                        else
                        {
                            textRadio.TextColor = Color.Black;
                        }
                       
                        setVisibleButton();
                    };
                    horizont.Children.Add(radioButton);
                    horizont.Children.Add(textRadio);
                    radio.Children.Add(horizont);
                }

                containerPolss.Children.Add(radio);

                _contentQuest.Add(containerPolss);
                i++;
            }
        }

        void setIndicator()
        {
            Grid grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                }
            };
            int i = 0;
            double width = StackLayoutIndicator.WidthRequest / _pollInfo.Questions.Count;
            foreach (var each in _pollInfo.Questions)
            {
                Color backgroundColor = (Color)Application.Current.Resources["MainColor"];
                if (i > 0)
                {
                    backgroundColor = Color.Transparent;
                }

                Label indicator = new Label
                {
                    HeightRequest = 2,
                    WidthRequest = width,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = backgroundColor
                };
                indicators.Add(indicator);
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(columnDefinition);
                grid.Children.Add(indicator, i, 0);
                grid.HorizontalOptions = LayoutOptions.FillAndExpand;
                i++;
            }

            StackLayoutIndicator.Children.Add(grid);
        }

        void setVisibleButton()
        {
            if (_isComplite)
            {
                FrameBtnNext.IsVisible = true;
            }
            else
            {
                FrameBtnNext.IsVisible = isCheched;
            }

            if (_pollingResult.Answers[quest].AnswerId != -1)
            {
                FrameBtnNext.IsVisible = true;
            }

            if (quest + 1 == _pollInfo.Questions.Count)
            {
                FrameBtnNext.IsVisible = false;
                if (_isComplite)
                {
                    FrameBtnFinish.IsVisible = false;
                }
                else
                {
                    FrameBtnFinish.IsVisible = isCheched;
                }

                if (_pollingResult.Answers[quest].AnswerId != -1)
                {
                    FrameBtnFinish.IsVisible = !_isComplite;
                }

                if (quest > 0)
                {
                    FrameBack.IsVisible = true;
                }
                else
                {
                    FrameBack.IsVisible = false;
                }
            }
        }

        void setQuestVisible()
        {
            if (_contentQuest.Count != 0 && quest > -1)
                Container.Children.Add(_contentQuest[quest]);
        }

        private void ButtonClickBack(object sender, EventArgs e)
        {
            BackQuest();
        }

        private void BackQuest()
        {
            quest--;
            visibleIndicator(quest, false);
            Container.Children.Clear();
            Container.Children.Add(_contentQuest[quest]);
            ChechQuestions();
            FrameBtnNext.IsVisible = true;
        }

        private void ButtonClickNext(object sender, EventArgs e)
        {
            NextQuest();
        }

        void visibleIndicator(int i, bool isNext)
        {
            if (isNext)
            {
                indicators[i].BackgroundColor = (Color)Application.Current.Resources["MainColor"];
                indicators[i - 1].BackgroundColor = Color.Transparent;
            }
            else
            {
                indicators[i + 1].BackgroundColor = Color.Transparent;
                indicators[i].BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            }
        }

        private void NextQuest()
        {
            quest++;
            visibleIndicator(quest, true);
            Container.Children.Clear();
            Container.Children.Add(_contentQuest[quest]);
            ChechQuestions();
            isCheched = false;
            setVisibleButton();
        }

        private async void ButtonClickFinish(object sender, EventArgs e)
        {
        }
    }
}