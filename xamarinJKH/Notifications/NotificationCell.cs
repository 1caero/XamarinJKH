using System;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    public class NotificationCell : ViewCell
    {
        private Label title = new Label();
        private Label more = new Label();
        private Label date = new Label();
        private Label text = new Label();
        Frame ReadIndicator;

        public NotificationCell()
        {
            PancakeView frame = new PancakeView();
            frame.HasShadow = false;
            frame.BorderThickness = 1;
            frame.SetAppThemeColor(PancakeView.BorderColorProperty, (Color)Application.Current.Resources["MainColor"], Color.White);
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(10, 0, 10, 10);
            frame.Padding = new Thickness(20, 23, 20, 10);
            frame.CornerRadius = 30;
            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Vertical;
            StackLayout titleContainer = new StackLayout();
            titleContainer.Orientation = StackOrientation.Horizontal;
            titleContainer.HorizontalOptions = LayoutOptions.FillAndExpand;
            title.Text = "Заголовок";
            title.HorizontalTextAlignment = TextAlignment.Start;
            title.HorizontalOptions = LayoutOptions.StartAndExpand;
            title.FontAttributes = FontAttributes.Bold;
            title.FontSize = 15;
            title.TextColor = Color.Black;
            more.Text =  $"{AppResources.Details} >";
            more.FontSize = 13;
            more.TextDecorations = TextDecorations.Underline;
            more.TextColor = (Color)Application.Current.Resources["MainColor"];
            more.HorizontalTextAlignment = TextAlignment.End;
            more.VerticalOptions = LayoutOptions.Center;
            more.HorizontalOptions = LayoutOptions.EndAndExpand;
            more.MaxLines = 1;
            more.MinimumWidthRequest = 80;
            titleContainer.Children.Add(title);
            titleContainer.Children.Add(more);
            container.Children.Add(titleContainer);
            date.HorizontalOptions = LayoutOptions.Start;
            date.TextColor = Color.Black;
            date.FontSize = 12;
            date.VerticalOptions = LayoutOptions.Center;
            date.Margin = new Thickness(0,-5,0,0);
            container.Children.Add(date);
            text.HorizontalOptions = LayoutOptions.FillAndExpand;
            text.VerticalOptions = LayoutOptions.FillAndExpand;
            text.TextColor = Color.Black;
            text.FontSize = 15;
            text.TextType = TextType.Html;
            container.Children.Add(text);
            Grid containerMain = new Grid();
            containerMain.Padding = 0;
            containerMain.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = new GridLength(5) }
            };

            containerMain.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(5)},
                new RowDefinition { Height = GridLength.Star }
            };

            containerMain.Children.Add(container);
            Grid.SetRowSpan(container, 2);
            Grid.SetColumnSpan(container, 2);


            ReadIndicator = new Frame
            {
                CornerRadius = 5,
                BackgroundColor = Color.Red,
                IsVisible = false
            };
           
            ReadIndicator.SetBinding(View.IsVisibleProperty, "Read", BindingMode.TwoWay);
         
            containerMain.Children.Add(ReadIndicator, 1, 0);
            frame.Content = containerMain;
            View = frame;
            View.BackgroundColor = Color.White;

            MessagingCenter.Subscribe<Object, int>(this, "SetNotificationRead", (sender, args) =>
            {
                if (this.ID == args)
                {
                    ReadIndicator.IsVisible = false;
                }
            });
        }

        public static readonly BindableProperty TextNotifProperty =
            BindableProperty.Create("TextNotif", typeof(string), typeof(NotificationCell), "");

        public static readonly BindableProperty DateNotifProperty =
            BindableProperty.Create("DateNotif", typeof(string), typeof(NotificationCell), "");

        public static readonly BindableProperty TitleNotifProperty =
            BindableProperty.Create("TitleNotif", typeof(string), typeof(NotificationCell), "");

        public static readonly BindableProperty IDProperty =
            BindableProperty.Create("ID", typeof(int), typeof(NotificationCell), 0);


        public static readonly BindableProperty ReadProperty =
            BindableProperty.Create("Read", typeof(bool), typeof(NotificationCell));

        public string TextNotif
        {
            get { return (string) GetValue(TextNotifProperty); }
            set { SetValue(TextNotifProperty, value); }
        }

        public string DateNotif
        {
            get { return (string) GetValue(DateNotifProperty); }
            set { SetValue(DateNotifProperty, value); }
        }

        public string TitleNotif
        {
            get { return (string) GetValue(TitleNotifProperty); }
            set { SetValue(TitleNotifProperty, value); }
        }

        public int ID
        {
            get { return (int) GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }


        public bool Read
        {
            get { return (bool)GetValue(ReadProperty); }
            set { SetValue(ReadProperty, value); }
        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                text.Text = TextNotif;
                date.Text = DateNotif;
                title.Text = TitleNotif;
                if(Settings.Person.Accounts.Count > 0)
                    ReadIndicator.IsVisible = !Read;
                else
                {
                    ReadIndicator.IsVisible = false;
                }
            }
        }
    }
}