﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Questions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionsPage : ContentPage
    {
        public List<PollInfo> Quest { get; set; }
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;

                    await RefreshData();

                    IsRefreshing = false;
                });
            }
        }

        private async Task RefreshData()
        {
            if (Settings.EventBlockData.Error == null)
            {
                Settings.EventBlockData = await server.GetEventBlockData();
                Quest = Settings.EventBlockData.Polls;
                additionalList.ItemsSource = null;
                additionalList.ItemsSource = Quest;
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось получить информацию об опросах", "OK");
            }
        }

        public QuestionsPage()
        {
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    BackgroundColor = Color.White;
                    ImageTop.Margin = new Thickness(0, 0, 0, 0);
                    StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    RelativeLayoutTop.Margin = new Thickness(0,0,0,0);
                    if (App.ScreenHeight <= 667)//iPhone6
                    {
                        additionalList.Margin = new Thickness(0,-110,0,0);
                    }else if (App.ScreenHeight <= 736)//iPhone8Plus Height=736
                    {
                        additionalList.Margin = new Thickness(0,-145,0,0);
                    }
                    else
                    {
                        additionalList.Margin = new Thickness(0,-145,0,0);
                    }
                    break;
                case Device.Android:
                    double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        RelativeLayoutTop.Margin = new Thickness(0,0,0,-80);
                    }
                    break;
                default:
                    break;
            }
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            Quest = Settings.EventBlockData.Polls;
            this.BindingContext = this;
            additionalList.BackgroundColor = Color.Transparent;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
        }

        void isComplite()
        {
            foreach (var each in Settings.EventBlockData.Polls)
            {
                bool flag = false;
                foreach (var quest in each.Questions)
                {
                    foreach (var ans in quest.Answers)
                    {
                        if (ans.IsUserAnswer)
                        {
                            
                        }
                    }
                }
            }
        }
        
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text = "+" + Settings.Person.Phone;
            SwitchQuest.ThumbColor = Color.Black;
            SwitchQuest.OnColor = Color.FromHex(Settings.MobileSettings.color);
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            PollInfo select = e.Item as PollInfo;
            await Navigation.PushAsync(new PollsPage(select));
        }
    }
}