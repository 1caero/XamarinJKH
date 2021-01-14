using AiForms.Dialogs;
using System;
using System.IO;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.AppsConst
{
    public class MessageCellAuthor : StackLayout
    {
        private StackLayout ConteinerA = new StackLayout();

        private Image ImagePersonA = new Image();

        //private Label LabelNameA = new Label();
        private Label LabeltimeA = new Label();
        private Label LabelTextA = new Label();
        private Label LabelDateA = new Label();
        Frame frameDateA = new Frame();
        IconView imageA = new IconView();
        IconView imageHiden = new IconView();
        IconView imageHiden2 = new IconView();
        Frame frameA = new Frame();

        public MessageCellAuthor(RequestMessage message, Page p, string DateUniq, out string newDate)
        {
            frameA.HorizontalOptions = LayoutOptions.Start;
            frameA.VerticalOptions = LayoutOptions.Start;
            frameA.BackgroundColor = Color.White;
            frameA.Margin = new Thickness(-15, -30, 5, 0);
            frameA.Padding = 10;
            frameA.CornerRadius = 23;
            frameA.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            ImagePersonA.Source = ImageSource.FromFile("ic_not_author");
            ImagePersonA.HeightRequest = 25;
            ImagePersonA.WidthRequest = 25;
            ImagePersonA.VerticalOptions = LayoutOptions.Start;
            frameA.Content = ImagePersonA;

            LabeltimeA.VerticalOptions = LayoutOptions.End;
            LabeltimeA.HorizontalTextAlignment = TextAlignment.End;
            int margin = message.IsHidden ? -5 : -10;
            LabeltimeA.Margin = new Thickness(0, margin, 5, 0);

            LabelTextA.HorizontalTextAlignment = TextAlignment.Start;


            StackLayout containerDateA = new StackLayout();
            //StackLayout containerFioTimeA = new StackLayout();

            //LabelNameA.TextColor = Color.Transparent;
            //LabelNameA.FontSize = 15;
            //LabelNameA.IsVisible = false;
            //LabelNameA.HorizontalTextAlignment = TextAlignment.End;
            //LabelNameA.HorizontalOptions = LayoutOptions.Start;

            LabeltimeA.TextColor = Color.Gray;
            LabeltimeA.FontSize = 15;
            LabeltimeA.HorizontalOptions = LayoutOptions.End;

            //containerFioTimeA.Orientation = StackOrientation.Horizontal;

            Frame frameTextA = new Frame();
            frameTextA.HorizontalOptions = LayoutOptions.End;
            frameTextA.VerticalOptions = LayoutOptions.StartAndExpand;
            if (message.IsHidden)
            {
                frameTextA.SetAppThemeColor(Frame.BorderColorProperty,(Color) Application.Current.Resources["MainColor"], Color.Transparent );
                frameTextA.BackgroundColor = Color.FromHex("#EBEBEB");
                LabelTextA.TextColor = Color.Black;
            }
            else
            {
                frameTextA.BackgroundColor = (Color) Application.Current.Resources["MainColor"];
                LabelTextA.TextColor = Color.White;

            }

            frameTextA.Margin = new Thickness(0, 0, 0, 10);
            frameTextA.Padding = new Thickness(15, 15, 15, 15);
            frameTextA.CornerRadius = 20;
            frameTextA.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            StackLayout stackLayoutContentA = new StackLayout();
            stackLayoutContentA.HorizontalOptions = LayoutOptions.End;

            LabelTextA.FontSize = 15;

            LabelTextA.HorizontalOptions = LayoutOptions.Center;
            stackLayoutContentA.Children.Add(LabelTextA);

            imageA.IsVisible = message.FileID != -1;
            imageA.HorizontalOptions = LayoutOptions.CenterAndExpand;
            imageA.HeightRequest = 40;
            imageA.WidthRequest = 40;
            imageA.Foreground = Color.White;
            imageA.Source = "ic_file_download";

            imageHiden.IsVisible = message.IsHidden;
            imageHiden.HorizontalOptions = LayoutOptions.End;
            imageHiden.VerticalOptions = LayoutOptions.End;
            imageHiden.HeightRequest = 20;
            imageHiden.WidthRequest = 20;
            imageHiden.Margin = new Thickness(0, -10, -10, 0);
            imageHiden.Foreground = (Color) Application.Current.Resources["MainColor"];
            imageHiden.Source = "ic_close_password";

            ActivityIndicator indicator = new ActivityIndicator();
            indicator.WidthRequest = 40;
            indicator.HeightRequest = 40;
            indicator.Color = Color.White;
            indicator.IsVisible = false;


            if (message.FileID != -1)
            {
                var tgr = new TapGestureRecognizer();

                tgr.Tapped += async (s, e) =>
                {
                    string fileName = message.Text.Replace("Отправлен новый файл: ", "")
                        .Replace("\"", "")
                        .Replace("\"", "");
                    if (await DependencyService.Get<IFileWorker>().ExistsAsync(fileName))
                    {
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(fileName))
                        });
                    }
                    else
                    {
                        await Settings.StartProgressBar("Загрузка", 0.8);

                        byte[] memoryStream = await _server.GetFileAPPConst(message.FileID.ToString());
                        if (memoryStream != null)
                        {
                            await DependencyService.Get<IFileWorker>().SaveTextAsync(fileName, memoryStream);
                            Loading.Instance.Hide();
                            await Launcher.OpenAsync(new OpenFileRequest
                            {
                                File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(fileName))
                            });
                        }
                        else
                        {
                            await p.DisplayAlert("Ошибка", "Не удалось скачать файл", "OK");
                        }
                    }

                    //// await ShowRating();
                    //await PopupNavigation.Instance.PushAsync(new RatingBarContentView(hex, _requestInfo, false));
                    //await RefreshData();
                };
                imageA.GestureRecognizers.Add(tgr);
            }


            stackLayoutContentA.Children.Add(imageA);
            stackLayoutContentA.Children.Add(indicator);


            StackLayout stackLayoutIcon = new StackLayout();
            stackLayoutIcon.Orientation = StackOrientation.Horizontal;
            stackLayoutIcon.Spacing = 0;
            stackLayoutIcon.HorizontalOptions = LayoutOptions.End;
            frameTextA.Content = stackLayoutContentA;
            
            Grid grid = new Grid();
            grid.Children.Add(frameTextA);
            grid.Children.Add(imageHiden);
            
            stackLayoutIcon.Children.Add(grid);
            stackLayoutIcon.Children.Add(frameA);

            
            
            frameDateA.HorizontalOptions = LayoutOptions.Center;
            frameDateA.VerticalOptions = LayoutOptions.Start;
            frameDateA.BackgroundColor = Color.FromHex("#E2E2E2");
            frameDateA.Margin = new Thickness(0, 2, 0, 10);
            frameDateA.Padding = 5;
            frameDateA.CornerRadius = 15;
            frameDateA.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            LabelDateA.FontSize = 15;
            LabelDateA.TextColor = Color.FromHex("#777777");

            frameDateA.Content = LabelDateA;

            //containerDateA.Children.Add(containerFioTimeA);
            containerDateA.Children.Add(stackLayoutIcon);
            containerDateA.Children.Add(LabeltimeA);
            containerDateA.Spacing = 3;
            containerDateA.Margin = new Thickness(60, 0, 0, 0);
            containerDateA.HorizontalOptions = LayoutOptions.FillAndExpand;


            ConteinerA.Children.Add(frameDateA);
            ConteinerA.Children.Add(containerDateA);

            var dateMess = message.DateAdd;
            //if (Settings.DateUniq.Equals(dateMess))
            if (DateUniq.Equals(dateMess))
            {
                frameDateA.IsVisible = false;
                frameA.Content = new Label()
                {
                    WidthRequest = 25,
                    HeightRequest = 25
                };
                frameA.BackgroundColor = Color.Transparent;
                Settings.isSelf = message.IsSelf;
            }
            else
            {
                frameDateA.IsVisible = true;
                //Settings.DateUniq = dateMess;
                DateUniq = dateMess;
            }

            newDate = dateMess;

            LabelDateA.Text = dateMess;
            //LabelNameA.Text = Name;
            LabelTextA.Text = message.Text;

            LabeltimeA.Text = message.TimeAdd;


            Children.Add(ConteinerA);
        }

        private RestClientMP _server = new RestClientMP();
    }


    public class MessageCellService : StackLayout
    {
        private StackLayout Container = new StackLayout();
        private Image ImagePerson = new Image();
        private Label LabelName = new Label();
        private Label Labeltime = new Label();
        private Label LabelText = new Label();
        private Label LabelDate = new Label();
        Frame frameDate = new Frame();
        IconView image = new IconView();
        IconView imageHiden = new IconView();
        Frame frame = new Frame();

        private RestClientMP _server = new RestClientMP();


        public MessageCellService(RequestMessage message, Page p, string DateUniq, out string newDate,
            string prevAuthor)
        {
            frame.HorizontalOptions = LayoutOptions.Start;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(5, -30, -15, 0);
            frame.Padding = 10;
            frame.HasShadow = true;
            frame.CornerRadius = 23;
            frame.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            ImagePerson.Source = ImageSource.FromFile("ic_author");
            ImagePerson.HeightRequest = 25;
            ImagePerson.WidthRequest = 25;
            ImagePerson.VerticalOptions = LayoutOptions.Start;

            frame.Content = ImagePerson;

            StackLayout containerDate = new StackLayout();
            StackLayout containerFioTime = new StackLayout();

            LabelName.TextColor = Color.Gray;
            LabelName.FontSize = 15;
            LabelName.Margin = new Thickness(55, 0, 0, 0);
            LabelName.HorizontalOptions = LayoutOptions.Center;

            Labeltime.TextColor = Color.Gray;
            Labeltime.FontSize = 15;
            Labeltime.HorizontalTextAlignment = TextAlignment.Start;
            Labeltime.VerticalOptions = LayoutOptions.End;
            Labeltime.HorizontalOptions = LayoutOptions.Start;
            int margin = message.IsHidden ? -5 : -10;
            Labeltime.Margin = new Thickness(5, margin, 15, 0);

            containerFioTime.Orientation = StackOrientation.Horizontal;
            containerFioTime.HorizontalOptions = LayoutOptions.FillAndExpand;
            containerFioTime.Children.Add(LabelName);

            Frame frameText = new Frame();
            frameText.HorizontalOptions = LayoutOptions.Start;
            frameText.VerticalOptions = LayoutOptions.StartAndExpand;
            frameText.BackgroundColor = Color.FromHex("#E2E2E2");
            frameText.Margin = new Thickness(0, 0, 0, 10);
            frameText.Padding = new Thickness(15, 15, 15, 15);
            frameText.CornerRadius = 20;
            frameText.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            if (message.IsHidden)
            {
                frameText.SetAppThemeColor(Frame.BorderColorProperty,(Color) Application.Current.Resources["MainColor"], Color.Transparent );
                frameText.BackgroundColor = Color.FromHex("#EBEBEB");
                LabelText.TextColor = Color.Black;
            }
            else
            {
                frameText.BackgroundColor = (Color) Application.Current.Resources["MainColor"];
                LabelText.TextColor = Color.White;

            }
            StackLayout stackLayoutContent = new StackLayout();

            image.IsVisible = message.FileID != -1;
            image.HorizontalOptions = LayoutOptions.CenterAndExpand;
            image.HeightRequest = 40;
            image.WidthRequest = 40;
            image.Foreground = Color.White;
            image.Source = "ic_file_download";

            imageHiden.IsVisible = message.IsHidden;
            imageHiden.HorizontalOptions = LayoutOptions.Start;
            imageHiden.VerticalOptions = LayoutOptions.End;
            imageHiden.HeightRequest = 20;
            imageHiden.WidthRequest = 20;
            imageHiden.Margin = new Thickness(-10, -10, 0, 0);
            imageHiden.Foreground = (Color) Application.Current.Resources["MainColor"];
            imageHiden.Source = "ic_close_password";
            
            if (message.FileID != -1)
            {
                var tgr = new TapGestureRecognizer();

                tgr.Tapped += async (s, e) =>
                {
                    string fileName = message.Text.Replace("Отправлен новый файл: ", "")
                        .Replace("\"", "")
                        .Replace("\"", "");
                    if (await DependencyService.Get<IFileWorker>().ExistsAsync(fileName))
                    {
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(fileName))
                        });
                    }
                    else
                    {
                        MessagingCenter.Send<Object, string>(this, "OpenFileConst",
                            message.FileID.ToString() + "," + fileName);
                    }

                    //// await ShowRating();
                    //await PopupNavigation.Instance.PushAsync(new RatingBarContentView(hex, _requestInfo, false));
                    //await RefreshData();
                };
                image.GestureRecognizers.Add(tgr);
            }

            LabelText.FontSize = 15;
            LabelText.HorizontalTextAlignment = TextAlignment.Start;
            LabelText.HorizontalOptions = LayoutOptions.Start;

            stackLayoutContent.Children.Add(LabelText);
            stackLayoutContent.Children.Add(image);
            frameText.Content = stackLayoutContent;

            StackLayout stackLayoutIconB = new StackLayout();
            stackLayoutIconB.Orientation = StackOrientation.Horizontal;
            stackLayoutIconB.Spacing = 0;

            Grid grid = new Grid();
            grid.Children.Add(frameText);
            grid.Children.Add(imageHiden);
            
            stackLayoutIconB.Children.Add(frame);
            stackLayoutIconB.Children.Add(grid);

            // stackLayoutIconB.Children.Add(frameText);


            frameDate.HorizontalOptions = LayoutOptions.Center;
            frameDate.VerticalOptions = LayoutOptions.Start;
            frameDate.BackgroundColor = Color.FromHex("#E2E2E2");
            frameDate.Margin = new Thickness(0, 2, 0, 10);
            frameDate.Padding = new Thickness(5, 5, 5, 5);
            frameDate.CornerRadius = 15;
            frameDate.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            LabelDate.FontSize = 15;
            LabelDate.TextColor = Color.FromHex("#777777");

            frameDate.Content = LabelDate;
            LabelText.HorizontalOptions = LayoutOptions.Center;

            containerDate.Children.Add(containerFioTime);
            containerDate.Children.Add(stackLayoutIconB);
            containerDate.Children.Add(Labeltime);

            Container.Children.Add(frameDate);
            Container.Children.Add(containerDate);

            StackLayout stackLayoutContentA = new StackLayout();
            stackLayoutContentA.HorizontalOptions = LayoutOptions.End;

            StackLayout stackLayoutIcon = new StackLayout();
            stackLayoutIcon.Orientation = StackOrientation.Horizontal;
            stackLayoutIcon.Spacing = 0;
            stackLayoutIcon.HorizontalOptions = LayoutOptions.End;

            containerDate.Margin = new Thickness(0, 0, 60, 0);

            var dateMess = message.DateAdd;
            if (DateUniq.Equals(dateMess) && message.AuthorName == prevAuthor)
            {
                frameDate.IsVisible = false;

                if (Settings.isSelf == message.IsSelf)
                {
                    LabelName.IsVisible = false;
                    frame.Content = new Label()
                    {
                        WidthRequest = 25,
                        HeightRequest = 25
                    };
                    frame.BackgroundColor = Color.Transparent;
                }

                Settings.isSelf = message.IsSelf;
            }
            else
            {
                frameDate.IsVisible = true;
                Settings.DateUniq = dateMess;
            }

            newDate = dateMess;

            if (DateUniq == dateMess && message.AuthorName != prevAuthor)
            {
                frameDate.IsVisible = false;
            }

            LabelDate.Text = dateMess;
            LabelName.Text = message.AuthorName;
            LabelText.Text = message.Text;
            Labeltime.Text = message.TimeAdd;


            Children.Add(Container);
        }
    }

    //public class MessageConstCell : ViewCell
    //{
    //    RestClientMP server = new RestClientMP();
    //    private StackLayout ConteinerA = new StackLayout();
    //    private Image ImagePersonA = new Image();
    //    private Label LabelNameA = new Label();
    //    private Label LabeltimeA = new Label();
    //    private Label LabelTextA = new Label();
    //    private Label LabelDateA = new Label();
    //    Frame frameDateA = new Frame();
    //    private StackLayout Container = new StackLayout();
    //    private Image ImagePerson = new Image();
    //    private Label LabelName = new Label();
    //    private Label Labeltime = new Label();
    //    private Label LabelText = new Label();
    //    private Label LabelDate = new Label();
    //    Frame frameDate = new Frame();
    //    IconView imageA = new IconView();
    //    IconView image = new IconView();
    //    Frame frameA = new Frame();
    //    Frame frame = new Frame();

    //    public MessageConstCell()
    //    {
    //        frame.HorizontalOptions = LayoutOptions.Start;
    //        frame.VerticalOptions = LayoutOptions.Start;
    //        frame.BackgroundColor = Color.White;
    //        frame.Margin = new Thickness(5, -30, -15, 0);
    //        frame.Padding = 10;
    //        frame.HasShadow = true;
    //        frame.CornerRadius = 23;


    //        ImagePerson.Source = ImageSource.FromFile("ic_author");
    //        ImagePerson.HeightRequest = 25;
    //        ImagePerson.WidthRequest = 25;
    //        ImagePerson.VerticalOptions = LayoutOptions.Start;
    //        frame.Content = ImagePerson;

    //        frameA.HorizontalOptions = LayoutOptions.Start;
    //        frameA.VerticalOptions = LayoutOptions.Start;
    //        frameA.BackgroundColor = Color.White;
    //        frameA.Margin = new Thickness(-15, -30, 5, 0);
    //        frameA.Padding = 10;
    //        frameA.CornerRadius = 23;

    //        ImagePersonA.Source = ImageSource.FromFile("ic_not_author");
    //        ImagePersonA.HeightRequest = 25;
    //        ImagePersonA.WidthRequest = 25;
    //        ImagePersonA.VerticalOptions = LayoutOptions.Start;
    //        frameA.Content = ImagePersonA;


    //        StackLayout containerDate = new StackLayout();
    //        StackLayout containerFioTime = new StackLayout();

    //        LabelName.TextColor = Color.Gray;
    //        LabelName.FontSize = 15;
    //        LabelName.Margin = new Thickness(55, 0, 0, 0);
    //        LabelName.HorizontalOptions = LayoutOptions.Center;

    //        Labeltime.TextColor = Color.Gray;
    //        Labeltime.FontSize = 15;
    //        Labeltime.HorizontalTextAlignment = TextAlignment.Start;
    //        Labeltime.VerticalOptions = LayoutOptions.End;
    //        LabeltimeA.VerticalOptions = LayoutOptions.End;
    //        LabeltimeA.HorizontalTextAlignment = TextAlignment.End;
    //        Labeltime.HorizontalOptions = LayoutOptions.Start;
    //        Labeltime.Margin = new Thickness(5, -10, 15, 0);
    //        LabeltimeA.Margin = new Thickness(0, -10, 5, 0);

    //        containerFioTime.Orientation = StackOrientation.Horizontal;
    //        containerFioTime.HorizontalOptions = LayoutOptions.FillAndExpand;
    //        // containerFioTime.Children.Add(frame);
    //        containerFioTime.Children.Add(LabelName);
    //        // containerFioTime.Children.Add(Labeltime);

    //        Frame frameText = new Frame();
    //        frameText.HorizontalOptions = LayoutOptions.Start;
    //        frameText.VerticalOptions = LayoutOptions.StartAndExpand;
    //        frameText.BackgroundColor = Color.FromHex("#E2E2E2");
    //        frameText.Margin = new Thickness(0, 0, 0, 10);
    //        frameText.Padding = new Thickness(15, 15, 15, 15);
    //        frameText.CornerRadius = 20;

    //        StackLayout stackLayoutContent = new StackLayout();

    //        image.IsVisible = false;
    //        image.HorizontalOptions = LayoutOptions.CenterAndExpand;


    //        LabelText.TextColor = Color.Black;
    //        LabelText.FontSize = 15;
    //        LabelText.HorizontalTextAlignment = TextAlignment.Start;
    //        LabelTextA.HorizontalTextAlignment = TextAlignment.Start;
    //        LabelText.HorizontalOptions = LayoutOptions.Start;

    //        stackLayoutContent.Children.Add(LabelText);
    //        stackLayoutContent.Children.Add(image);
    //        frameText.Content = stackLayoutContent;

    //        StackLayout stackLayoutIconB = new StackLayout();
    //        stackLayoutIconB.Orientation = StackOrientation.Horizontal;
    //        stackLayoutIconB.Spacing = 0;

    //        stackLayoutIconB.Children.Add(frame);
    //        stackLayoutIconB.Children.Add(frameText);


    //        frameDate.HorizontalOptions = LayoutOptions.Center;
    //        frameDate.VerticalOptions = LayoutOptions.Start;
    //        frameDate.BackgroundColor = Color.FromHex("#E2E2E2");
    //        frameDate.Margin = new Thickness(0, 0, 0, 10);
    //        frameDate.Padding = new Thickness(5, 5, 5, 5);
    //        frameDate.CornerRadius = 15;

    //        LabelDate.FontSize = 15;
    //        LabelDate.TextColor = Color.FromHex("#777777");

    //        frameDate.Content = LabelDate;
    //        LabelText.HorizontalOptions = LayoutOptions.Center;

    //        // containerDate.Children.Add(frameDate);
    //        containerDate.Children.Add(containerFioTime);
    //        containerDate.Children.Add(stackLayoutIconB);
    //        containerDate.Children.Add(Labeltime);


    //        Container.Children.Add(frameDate);
    //        Container.Children.Add(containerDate);

    //        StackLayout containerDateA = new StackLayout();
    //        StackLayout containerFioTimeA = new StackLayout();

    //        LabelNameA.TextColor = Color.Transparent;
    //        LabelNameA.FontSize = 15;
    //        LabelNameA.IsVisible = false;
    //        LabelNameA.HorizontalTextAlignment = TextAlignment.End;
    //        LabelNameA.HorizontalOptions = LayoutOptions.Start;

    //        LabeltimeA.TextColor = Color.Gray;
    //        LabeltimeA.FontSize = 15;
    //        LabeltimeA.HorizontalOptions = LayoutOptions.End;

    //        containerFioTimeA.Orientation = StackOrientation.Horizontal;

    //        // containerFioTimeA.Children.Add(LabeltimeA);
    //        // containerFioTimeA.Children.Add(LabelNameA);
    //        // containerFioTimeA.Children.Add(frameA);


    //        Frame frameTextA = new Frame();
    //        frameTextA.HorizontalOptions = LayoutOptions.End;
    //        frameTextA.VerticalOptions = LayoutOptions.StartAndExpand;
    //        frameTextA.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
    //        frameTextA.Margin = new Thickness(0, 0, 0, 10);
    //        frameTextA.Padding = new Thickness(15, 15, 15, 15);
    //        frameTextA.CornerRadius = 20;

    //        imageA.IsVisible = false;
    //        imageA.HorizontalOptions = LayoutOptions.CenterAndExpand;
    //        image.HeightRequest = imageA.HeightRequest = 40;
    //        image.WidthRequest = imageA.WidthRequest = 40;

    //        imageA.Foreground = image.Foreground = Color.White;
    //        image.Source = imageA.Source = "ic_file_download";

    //        StackLayout stackLayoutContentA = new StackLayout();
    //        stackLayoutContentA.HorizontalOptions = LayoutOptions.End;

    //        LabelTextA.TextColor = Color.White;
    //        LabelTextA.FontSize = 15;
    //        LabelTextA.HorizontalOptions = LayoutOptions.End;

    //        stackLayoutContentA.Children.Add(LabelTextA);
    //        stackLayoutContentA.Children.Add(imageA);
    //        StackLayout stackLayoutIcon = new StackLayout();
    //        stackLayoutIcon.Orientation = StackOrientation.Horizontal;
    //        stackLayoutIcon.Spacing = 0;
    //        stackLayoutIcon.HorizontalOptions = LayoutOptions.End;
    //        frameTextA.Content = stackLayoutContentA;

    //        stackLayoutIcon.Children.Add(frameTextA);
    //        stackLayoutIcon.Children.Add(frameA);

    //        frameDateA.HorizontalOptions = LayoutOptions.Center;
    //        frameDateA.VerticalOptions = LayoutOptions.Start;
    //        frameDateA.BackgroundColor = Color.FromHex("#E2E2E2");
    //        frameDateA.Margin = new Thickness(0, 2, 0, 10);
    //        frameDateA.Padding = 5;
    //        frameDateA.CornerRadius = 15;

    //        LabelDateA.FontSize = 15;
    //        LabelDateA.TextColor = Color.FromHex("#777777");

    //        frameDateA.Content = LabelDateA;
    //        LabelTextA.HorizontalOptions = LayoutOptions.Center;

    //        // containerDateA.Children.Add(frameDateA);
    //        containerDateA.Children.Add(containerFioTimeA);
    //        containerDateA.Children.Add(stackLayoutIcon);
    //        containerDateA.Children.Add(LabeltimeA);
    //        containerDateA.Spacing = 3;
    //        containerDateA.Margin = new Thickness(60, 0, 0, 0);
    //        containerDateA.HorizontalOptions = LayoutOptions.FillAndExpand;
    //        containerDate.Margin = new Thickness(0, 0, 60, 0);

    //        // ConteinerA.HorizontalOptions = LayoutOptions.FillAndExpand;

    //        ConteinerA.Children.Add(frameDateA);
    //        ConteinerA.Children.Add(containerDateA);

    //        StackLayout stackLayout = new StackLayout();

    //        stackLayout.Children.Add(Container);
    //        stackLayout.Children.Add(ConteinerA);

    //        View = stackLayout;
    //    }

    //    public static readonly BindableProperty NameProperty =
    //        BindableProperty.Create("Name", typeof(string), typeof(MessageConstCell), "");

    //    public static readonly BindableProperty FileIDProperty =
    //        BindableProperty.Create("FileID", typeof(string), typeof(MessageConstCell), "");

    //    public static readonly BindableProperty TimeProperty =
    //        BindableProperty.Create("Time", typeof(string), typeof(MessageConstCell), "");

    //    public static readonly BindableProperty DateAppProperty =
    //        BindableProperty.Create("DateApp", typeof(string), typeof(MessageConstCell), "");

    //    public static readonly BindableProperty TextAppProperty =
    //        BindableProperty.Create("TextApp", typeof(string), typeof(MessageConstCell), "");

    //    public static readonly BindableProperty IsSelfProperty =
    //        BindableProperty.Create("IsSelf", typeof(bool), typeof(MessageConstCell), false);

    //    public string Name
    //    {
    //        get { return (string)GetValue(NameProperty); }
    //        set { SetValue(NameProperty, value); }
    //    }

    //    public string FileID
    //    {
    //        get { return (string)GetValue(FileIDProperty); }
    //        set { SetValue(FileIDProperty, value); }
    //    }

    //    public string Time
    //    {
    //        get { return (string)GetValue(TimeProperty); }
    //        set { SetValue(TimeProperty, value); }
    //    }

    //    public string DateApp
    //    {
    //        get { return (string)GetValue(DateAppProperty); }
    //        set { SetValue(DateAppProperty, value); }
    //    }

    //    public string TextApp
    //    {
    //        get { return (string)GetValue(TextAppProperty); }
    //        set { SetValue(TextAppProperty, value); }
    //    }

    //    public bool IsSelf
    //    {
    //        get { return (bool)GetValue(IsSelfProperty); }
    //        set { SetValue(IsSelfProperty, value); }
    //    }


    //    protected override async void OnBindingContextChanged()
    //    {
    //        base.OnBindingContextChanged();

    //        if (BindingContext != null)
    //        {
    //            if (IsSelf)
    //            {
    //                ConteinerA.IsVisible = true;
    //                Container.IsVisible = false;
    //            }
    //            else
    //            {
    //                ConteinerA.IsVisible = false;
    //                Container.IsVisible = true;
    //            }


    //            var strings = DateApp.Split(' ');
    //            var dateMess = strings[0];
    //            if (Settings.DateUniq.Equals(dateMess))
    //            {
    //                frameDate.IsVisible = false;
    //                frameDateA.IsVisible = false;
    //                if (Settings.isSelf == null || (bool)Settings.isSelf)
    //                {
    //                    frameA.Content = new Label()
    //                    {
    //                        WidthRequest = 25,
    //                        HeightRequest = 25
    //                    };
    //                    frameA.BackgroundColor = Color.Transparent;
    //                }
    //                else
    //                {
    //                    ConteinerA.Margin = new Thickness(0, 25, 0, 0);
    //                    frame.Content = new Label()
    //                    {
    //                        WidthRequest = 25,
    //                        HeightRequest = 25
    //                    };
    //                    frame.BackgroundColor = Color.Transparent;

    //                    LabelName.IsVisible = false;
    //                }

    //                Settings.isSelf = IsSelf;
    //            }
    //            else
    //            {
    //                frameDate.IsVisible = true;
    //                frameDateA.IsVisible = true;
    //                Settings.DateUniq = dateMess;
    //            }

    //            LabelDate.Text = LabelDateA.Text = dateMess;
    //            LabelName.Text = LabelNameA.Text = Name;
    //            LabelText.Text = LabelTextA.Text = TextApp;
    //            Labeltime.Text = LabeltimeA.Text = strings[1].Substring(0, 5);
    //            if (!FileID.Equals("-1"))
    //            {
    //                image.IsVisible = imageA.IsVisible = true;
    //            }
    //        }
    //    }
    //}
}