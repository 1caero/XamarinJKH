using AiForms.Dialogs;
using FFImageLoading.Svg.Forms;
using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Apps
{
    public class MessageCellAuthor : StackLayout
    {
        private StackLayout ConteinerA = new StackLayout();
        private Image ImagePersonA = new Image();
        private Label LabeltimeA = new Label();
        private Label LabelTextA = new Label();
        private Label LabelDateA = new Label();
        Frame frameDateA = new Frame();
        SvgCachedImage imageA = new SvgCachedImage();
        Frame frameA = new Frame();

        public MessageCellAuthor(RequestMessage message, Page p, string DateUniq, out string newDate,
            bool isTech = false)
        {
            frameA.HorizontalOptions = LayoutOptions.Start;
            frameA.VerticalOptions = LayoutOptions.Start;
            frameA.BackgroundColor = Color.White;
            frameA.Margin = new Thickness(-15, -30, 5, 0);
            frameA.Padding = 10;
            frameA.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frameA.CornerRadius = 23;

            ImagePersonA.Source = ImageSource.FromFile("ic_author");
            ImagePersonA.HeightRequest = 25;
            ImagePersonA.WidthRequest = 25;
            ImagePersonA.VerticalOptions = LayoutOptions.Start;
            frameA.Content = ImagePersonA;

            LabeltimeA.VerticalOptions = LayoutOptions.End;
            LabeltimeA.HorizontalTextAlignment = TextAlignment.End;

            LabeltimeA.Margin = new Thickness(0, -10, 5, 0);

            LabelTextA.HorizontalTextAlignment = TextAlignment.Start;


            StackLayout containerDateA = new StackLayout();

            LabeltimeA.TextColor = Color.Gray;
            LabeltimeA.FontSize = 15;
            LabeltimeA.HorizontalOptions = LayoutOptions.End;


            Frame frameTextA = new Frame();
            frameTextA.HorizontalOptions = LayoutOptions.End;
            frameTextA.VerticalOptions = LayoutOptions.StartAndExpand;
            frameTextA.BackgroundColor = (Color) Application.Current.Resources["MainColor"];
            frameTextA.Margin = new Thickness(0, 0, 0, 10);
            frameTextA.Padding = new Thickness(15, 15, 15, 15);
            frameTextA.CornerRadius = 20;
            frameTextA.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            StackLayout stackLayoutContentA = new StackLayout();
            stackLayoutContentA.HorizontalOptions = LayoutOptions.End;

            LabelTextA.TextColor = Color.White;
            LabelTextA.FontSize = 15;
            

            LabelTextA.HorizontalOptions = LayoutOptions.Center;
            stackLayoutContentA.Children.Add(LabelTextA);

            imageA.IsVisible = message.FileID != -1;
            imageA.HorizontalOptions = LayoutOptions.CenterAndExpand;
            imageA.HeightRequest = 40;
            imageA.WidthRequest = 40;
            imageA.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string>
                {{"#000000", $"#FFFFFF"}};
            imageA.Source = "resource://xamarinJKH.Resources.ic_file_download.svg";



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
                        var LoadFileTask = new Task(async () =>
                        {
                            Configurations.LoadingConfig = new LoadingConfig
                            {
                                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                                OverlayColor = Color.Black,
                                Opacity = 0.8,
                                DefaultMessage = AppResources.Loading,
                            };
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await Loading.Instance.StartAsync(async progress =>
                                {
                                    byte[] memoryStream = null;
                                    if (isTech)
                                    {
                                        memoryStream = await _server.GetFileAPP_Tech(message.FileID.ToString());

                                    }
                                    else
                                    {
                                        memoryStream = await _server.GetFileAPP(message.FileID.ToString());

                                    }

                                    if (memoryStream != null)
                                    {
                                        await DependencyService.Get<IFileWorker>().SaveTextAsync(fileName, memoryStream);
                                        await Launcher.OpenAsync(new OpenFileRequest
                                        {
                                            File = new ReadOnlyFile(DependencyService.Get<IFileWorker>()
                                                .GetFilePath(fileName))
                                        });
                                    }
                                    else
                                    {
                                        await p.DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFileLoading, "OK");
                                    }
                                });
                            });
                            
                        });
                        LoadFileTask.Start();
                    }
                };
                imageA.GestureRecognizers.Add(tgr);
            }


            stackLayoutContentA.Children.Add(imageA);


            StackLayout stackLayoutIcon = new StackLayout();
            stackLayoutIcon.Orientation = StackOrientation.Horizontal;
            stackLayoutIcon.Spacing = 0;
            stackLayoutIcon.HorizontalOptions = LayoutOptions.End;
            frameTextA.Content = stackLayoutContentA;

            stackLayoutIcon.Children.Add(frameTextA);
            stackLayoutIcon.Children.Add(frameA);

            frameDateA.HorizontalOptions = LayoutOptions.Center;
            frameDateA.VerticalOptions = LayoutOptions.Start;
            frameDateA.BackgroundColor = Color.FromHex("#E2E2E2");
            frameDateA.Margin = new Thickness(0, 2, 0, 10);
            frameDateA.Padding = 5;
            frameDateA.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frameDateA.CornerRadius = 15;

            LabelDateA.FontSize = 15;
            LabelDateA.TextColor = Color.FromHex("#777777");

            frameDateA.Content = LabelDateA;

            containerDateA.Children.Add(stackLayoutIcon);
            containerDateA.Children.Add(LabeltimeA);
            containerDateA.Spacing = 3;
            containerDateA.Margin = new Thickness(60, 0, 0, 0);
            containerDateA.HorizontalOptions = LayoutOptions.FillAndExpand;


            ConteinerA.Children.Add(frameDateA);
            ConteinerA.Children.Add(containerDateA);

            var dateMess = message.DateAdd;
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
                DateUniq = dateMess;
            }

            newDate = dateMess;

            LabelDateA.Text = dateMess;
            //LabelNameA.Text = Name;
            //LabelTextA.Text = message.Text;

            LabelTextA.FormattedText = Settings.FormatedLink(message.Text);

            //if (Device.RuntimePlatform == Device.Android)
            //    LabelTextA.TextType = TextType.Html;
            
            var link = new TapGestureRecognizer();
            link.Tapped += async (s, e) => { await Settings.OpenLinksMessage(message, p); };
            LabelTextA.GestureRecognizers.Add(link);

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
        SvgCachedImage image = new SvgCachedImage();
        Frame frame = new Frame();

        private RestClientMP _server = new RestClientMP();


        public MessageCellService(RequestMessage message, Page p, string DateUniq, out string newDate, string prevAuthor, bool isTech = false)
        {
            frame.HorizontalOptions = LayoutOptions.Start;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(5, -30, -15, 0);
            frame.Padding = 10;
            frame.CornerRadius = 23;
            frame.SetOnAppTheme(Frame.HasShadowProperty, false, true);

            ImagePerson.Source = ImageSource.FromFile("ic_not_author");
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
            Labeltime.Margin = new Thickness(5, -10, 15, 0);

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

            StackLayout stackLayoutContent = new StackLayout();

            image.IsVisible = message.FileID != -1;
            image.HorizontalOptions = LayoutOptions.CenterAndExpand;
            image.HeightRequest = 40;
            image.WidthRequest = 40;
            image.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#FFFFFF" } };
            image.Source = "resource://xamarinJKH.Resources.ic_file_download.svg";

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
                        Analytics.TrackEvent($"открытие файла {fileName}");
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(fileName))
                        });
                    }
                    else
                    {

                        var LoadFileTask = new Task(async () =>
                        {

                            Configurations.LoadingConfig = new LoadingConfig
                            {
                                IndicatorColor = (Color)Application.Current.Resources["MainColor"],
                                OverlayColor = Color.Black,
                                Opacity = 0.8,
                                DefaultMessage = AppResources.Loading,
                            };

                            Device.BeginInvokeOnMainThread(async () => {
                                await Loading.Instance.StartAsync(async progress =>
                                {

                                    byte[] memoryStream = null;
                                    if (isTech)
                                    {
                                        memoryStream = await _server.GetFileAPP_Tech(message.FileID.ToString());

                                    }
                                    else
                                    {
                                        memoryStream = await _server.GetFileAPP(message.FileID.ToString());

                                    }

                                    if (memoryStream != null)
                                    {
                                        Analytics.TrackEvent($"сохранение файла {fileName}");

                                        await DependencyService.Get<IFileWorker>().SaveTextAsync(fileName, memoryStream);

                                        Analytics.TrackEvent($"открытие файла {fileName}");

                                        await Launcher.OpenAsync(new OpenFileRequest
                                        {
                                            File = new ReadOnlyFile(DependencyService.Get<IFileWorker>()
                                                .GetFilePath(fileName))
                                        });
                                    }
                                    else
                                    {
                                        await p.DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFileLoading, "OK");
                                    }
                                });
                            });
                           
                        });
                        LoadFileTask.Start();
                    }
                };
                image.GestureRecognizers.Add(tgr);
            }

            LabelText.TextColor = Color.Black;
            LabelText.FontSize = 15;
            LabelText.HorizontalTextAlignment = TextAlignment.Start;
            
            LabelText.HorizontalOptions = LayoutOptions.Start;

            stackLayoutContent.Children.Add(LabelText);
            stackLayoutContent.Children.Add(image);
            frameText.Content = stackLayoutContent;

            StackLayout stackLayoutIconB = new StackLayout();
            stackLayoutIconB.Orientation = StackOrientation.Horizontal;
            stackLayoutIconB.Spacing = 0;

            stackLayoutIconB.Children.Add(frame);
            stackLayoutIconB.Children.Add(frameText);


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

            //LabelText.Text = message.Text;
            //if(Device.RuntimePlatform==Device.Android)
            //LabelText.TextType = TextType.Html;
            var link = new TapGestureRecognizer();
            link.Tapped += async (s, e) => { await Settings.OpenLinksMessage(message, p); };
            LabelText.GestureRecognizers.Add(link);
            LabelText.FormattedText = Settings.FormatedLink(message.Text);

            Labeltime.Text = message.TimeAdd;

            
            Children.Add(Container);
        }

       
    }

    public class MessageCell : ViewCell
    {
        private StackLayout ConteinerA = new StackLayout();
        private Image ImagePersonA = new Image();
        private Label LabelNameA = new Label();
        private Label LabeltimeA = new Label();
        private Label LabelTextA = new Label();
        private Label LabelDateA = new Label();
        Frame frameDateA = new Frame();
        SvgCachedImage imageA = new SvgCachedImage();
        Frame frameA = new Frame();

        private StackLayout Container = new StackLayout();
        private Image ImagePerson = new Image();
        private Label LabelName = new Label();
        private Label Labeltime = new Label();
        private Label LabelText = new Label();
        private Label LabelDate = new Label();
        Frame frameDate = new Frame();
        SvgCachedImage image = new SvgCachedImage();
        Frame frame = new Frame();

        public MessageCell()
        {
            frame.HorizontalOptions = LayoutOptions.Start;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(5, -30, -15, 0);
            frame.Padding = 10;
            frame.HasShadow = true;
            frame.CornerRadius = 23;


            ImagePerson.Source = ImageSource.FromFile("ic_not_author");
            ImagePerson.HeightRequest = 25;
            ImagePerson.WidthRequest = 25;
            ImagePerson.VerticalOptions = LayoutOptions.Start;
            frame.Content = ImagePerson;

            frameA.HorizontalOptions = LayoutOptions.Start;
            frameA.VerticalOptions = LayoutOptions.Start;
            frameA.BackgroundColor = Color.White;
            frameA.Margin = new Thickness(-15, -30, 5, 0);
            frameA.Padding = 10;
            frameA.CornerRadius = 23;

            ImagePersonA.Source = ImageSource.FromFile("ic_author");
            ImagePersonA.HeightRequest = 25;
            ImagePersonA.WidthRequest = 25;
            ImagePersonA.VerticalOptions = LayoutOptions.Start;
            frameA.Content = ImagePersonA;


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
            LabeltimeA.VerticalOptions = LayoutOptions.End;
            LabeltimeA.HorizontalTextAlignment = TextAlignment.End;
            Labeltime.HorizontalOptions = LayoutOptions.Start;
            Labeltime.Margin = new Thickness(5, -10, 15, 0);
            LabeltimeA.Margin = new Thickness(0, -10, 5, 0);

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

            StackLayout stackLayoutContent = new StackLayout();

            image.IsVisible = false;
            image.HorizontalOptions = LayoutOptions.CenterAndExpand;


            LabelText.TextColor = Color.Black;
            LabelText.FontSize = 15;
            LabelText.HorizontalTextAlignment = TextAlignment.Start;
            LabelTextA.HorizontalTextAlignment = TextAlignment.Start;
            LabelText.HorizontalOptions = LayoutOptions.Start;

            stackLayoutContent.Children.Add(LabelText);
            stackLayoutContent.Children.Add(image);
            frameText.Content = stackLayoutContent;

            StackLayout stackLayoutIconB = new StackLayout();
            stackLayoutIconB.Orientation = StackOrientation.Horizontal;
            stackLayoutIconB.Spacing = 0;

            stackLayoutIconB.Children.Add(frame);
            stackLayoutIconB.Children.Add(frameText);


            frameDate.HorizontalOptions = LayoutOptions.Center;
            frameDate.VerticalOptions = LayoutOptions.Start;
            frameDate.BackgroundColor = Color.FromHex("#E2E2E2");
            frameDate.Margin = new Thickness(0, 0, 0, 10);
            frameDate.Padding = new Thickness(5, 5, 5, 5);
            frameDate.CornerRadius = 15;

            LabelDate.FontSize = 15;
            LabelDate.TextColor = Color.FromHex("#777777");

            frameDate.Content = LabelDate;
            LabelText.HorizontalOptions = LayoutOptions.Center;

            containerDate.Children.Add(containerFioTime);
            containerDate.Children.Add(stackLayoutIconB);
            containerDate.Children.Add(Labeltime);


            Container.Children.Add(frameDate);
            Container.Children.Add(containerDate);

            StackLayout containerDateA = new StackLayout();
            StackLayout containerFioTimeA = new StackLayout();

            LabelNameA.TextColor = Color.Transparent;
            LabelNameA.FontSize = 15;
            LabelNameA.IsVisible = false;
            LabelNameA.HorizontalTextAlignment = TextAlignment.End;
            LabelNameA.HorizontalOptions = LayoutOptions.Start;

            LabeltimeA.TextColor = Color.Gray;
            LabeltimeA.FontSize = 15;
            LabeltimeA.HorizontalOptions = LayoutOptions.End;

            containerFioTimeA.Orientation = StackOrientation.Horizontal;


            Frame frameTextA = new Frame();
            frameTextA.HorizontalOptions = LayoutOptions.End;
            frameTextA.VerticalOptions = LayoutOptions.StartAndExpand;
            frameTextA.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            frameTextA.Margin = new Thickness(0, 0, 0, 10);
            frameTextA.Padding = new Thickness(15, 15, 15, 15);
            frameTextA.CornerRadius = 20;

            imageA.IsVisible = false;
            imageA.HorizontalOptions = LayoutOptions.CenterAndExpand;
            imageA.HeightRequest = 40;
            imageA.WidthRequest = 40;
            imageA.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#FFFFFF" } };
            imageA.Source = "resource://xamarinJKH.Resources.ic_file_download.svg";

            image.HeightRequest = 40;


            image.WidthRequest = 40;


            image.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#FFFFFF" } };
            image.Source = "resource://xamarinJKH.Resources.ic_file_download.svg"; 

            StackLayout stackLayoutContentA = new StackLayout();
            stackLayoutContentA.HorizontalOptions = LayoutOptions.End;

            LabelTextA.TextColor = Color.White;
            LabelTextA.FontSize = 15;
            LabelTextA.HorizontalOptions = LayoutOptions.End;

            stackLayoutContentA.Children.Add(LabelTextA);
            stackLayoutContentA.Children.Add(imageA);
            StackLayout stackLayoutIcon = new StackLayout();
            stackLayoutIcon.Orientation = StackOrientation.Horizontal;
            stackLayoutIcon.Spacing = 0;
            stackLayoutIcon.HorizontalOptions = LayoutOptions.End;
            frameTextA.Content = stackLayoutContentA;

            stackLayoutIcon.Children.Add(frameTextA);
            stackLayoutIcon.Children.Add(frameA);

            frameDateA.HorizontalOptions = LayoutOptions.Center;
            frameDateA.VerticalOptions = LayoutOptions.Start;
            frameDateA.BackgroundColor = Color.FromHex("#E2E2E2");
            frameDateA.Margin = new Thickness(0, 2, 0, 10);
            frameDateA.Padding = 5;
            frameDateA.CornerRadius = 15;

            LabelDateA.FontSize = 15;
            LabelDateA.TextColor = Color.FromHex("#777777");

            frameDateA.Content = LabelDateA;
            LabelTextA.HorizontalOptions = LayoutOptions.Center;


            containerDateA.Children.Add(containerFioTimeA);
            containerDateA.Children.Add(stackLayoutIcon);
            containerDateA.Children.Add(LabeltimeA);
            containerDateA.Spacing = 3;
            containerDateA.Margin = new Thickness(60, 0, 0, 0);
            containerDateA.HorizontalOptions = LayoutOptions.FillAndExpand;
            containerDate.Margin = new Thickness(0, 0, 60, 0);


            ConteinerA.Children.Add(frameDateA);
            ConteinerA.Children.Add(containerDateA);

            StackLayout stackLayout = new StackLayout();

            stackLayout.Children.Add(Container);
            stackLayout.Children.Add(ConteinerA);

            View = stackLayout;
        }

        public static readonly BindableProperty NameProperty =
            BindableProperty.Create("Name", typeof(string), typeof(MessageCell), "");

        public static readonly BindableProperty FileIDProperty =
            BindableProperty.Create("FileID", typeof(int), typeof(MessageCell), -1);

        public static readonly BindableProperty TimeProperty =
            BindableProperty.Create("Time", typeof(string), typeof(MessageCell), "");

        public static readonly BindableProperty DateAppProperty =
            BindableProperty.Create("DateApp", typeof(string), typeof(MessageCell), "");

        public static readonly BindableProperty TextAppProperty =
            BindableProperty.Create("TextApp", typeof(string), typeof(MessageCell), "");

        public static readonly BindableProperty IsSelfProperty =
            BindableProperty.Create("IsSelf", typeof(bool), typeof(MessageCell), false);

        public string Name
        {
            get { return (string) GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public int FileID
        {
            get { return (int) GetValue(FileIDProperty); }
            set { SetValue(FileIDProperty, value); }
        }

        public string Time
        {
            get { return (string) GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public string DateApp
        {
            get { return (string) GetValue(DateAppProperty); }
            set { SetValue(DateAppProperty, value); }
        }

        public string TextApp
        {
            get { return (string) GetValue(TextAppProperty); }
            set { SetValue(TextAppProperty, value); }
        }

        public bool IsSelf
        {
            get { return (bool) GetValue(IsSelfProperty); }
            set { SetValue(IsSelfProperty, value); }
        }


        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                if (IsSelf)
                {
                    ConteinerA.IsVisible = true;
                    Container.IsVisible = false;
                }
                else
                {
                    ConteinerA.IsVisible = false;
                    Container.IsVisible = true;
                }


                var strings = DateApp.Split(' ');
                var dateMess = strings[0];
                if (Settings.DateUniq.Equals(dateMess))
                {
                    frameDate.IsVisible = false;
                    frameDateA.IsVisible = false;
                    if (Settings.isSelf == null || (bool) Settings.isSelf)
                    {
                        frameA.Content = new Label()
                        {
                            WidthRequest = 25,
                            HeightRequest = 25
                        };
                        frameA.BackgroundColor = Color.Transparent;
                    }
                    else
                    {
                        ConteinerA.Margin = new Thickness(0, 25, 0, 0);
                        frame.Content = new Label()
                        {
                            WidthRequest = 25,
                            HeightRequest = 25
                        };
                        frame.BackgroundColor = Color.Transparent;

                        LabelName.IsVisible = false;
                    }

                    Settings.isSelf = IsSelf;
                }
                else
                {
                    frameDate.IsVisible = true;
                    frameDateA.IsVisible = true;
                    Settings.DateUniq = dateMess;
                }

                LabelDate.Text = dateMess;
                LabelDateA.Text = dateMess;
                LabelName.Text = Name;
                LabelNameA.Text = Name;
                LabelText.Text = TextApp;
                LabelTextA.Text = TextApp;
                Labeltime.Text = strings[1].Substring(0, 5);
                LabeltimeA.Text = strings[1].Substring(0, 5);

                bool imgVisible = !FileID.Equals("-1");

                if (imgVisible)
                {
                    image.IsVisible = true;
                    imageA.IsVisible = true;
                }
                else
                {
                    image.IsVisible = imageA.IsVisible = false;
                }
            }
        }
    }
}