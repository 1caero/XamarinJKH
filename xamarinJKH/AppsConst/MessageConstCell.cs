using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiForms.Dialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms.Markup;

namespace xamarinJKH.AppsConst
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
        IconView imageHiden = new IconView();
        //IconView imageHiden2 = new IconView();
        Frame frameA = new Frame();

        public MessageCellAuthor(RequestMessage message, Page p, string DateUniq, out string newDate, string reqId)
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

            //if (Device.RuntimePlatform == Device.Android)
            //    LabelTextA.TextType = TextType.Html;


            StackLayout containerDateA = new StackLayout();
            

            LabeltimeA.TextColor = Color.Gray;
            LabeltimeA.FontSize = 15;
            LabeltimeA.HorizontalOptions = LayoutOptions.End;


            Frame frameTextA = new Frame();
            frameTextA.HorizontalOptions = LayoutOptions.End;
            frameTextA.VerticalOptions = LayoutOptions.StartAndExpand;
            Color currentResource = (Color)Application.Current.Resources["MainColor"];
            if (message.IsHidden)
            {
                frameTextA.SetAppThemeColor(Frame.BorderColorProperty,currentResource, Color.Transparent );
                frameTextA.BackgroundColor = Color.FromHex("#EBEBEB");
                LabelTextA.TextColor = Color.Black;
            }
            else
            {
                frameTextA.BackgroundColor = currentResource;
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
            imageA.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", $"#FFFFFF" } };
            imageA.Source = "resource://xamarinJKH.Resources.ic_file_download.svg";

            imageHiden.IsVisible = message.IsHidden;
            imageHiden.HorizontalOptions = LayoutOptions.End;
            imageHiden.VerticalOptions = LayoutOptions.End;
            imageHiden.HeightRequest = 20;
            imageHiden.WidthRequest = 20;
            imageHiden.Margin = new Thickness(0, -10, -10, 0);
            imageHiden.Foreground = currentResource;
            imageHiden.Source = "ic_close_password";

            //imageHiden.IsVisible = true;// message.IsHidden;
            //imageHiden.Source = "resource://xamarinJKH.Resources.ic_close_password.svg";
            //imageHiden.HorizontalOptions = LayoutOptions.End;
            //imageHiden.VerticalOptions = LayoutOptions.End;
            //imageHiden.HeightRequest = 20;
            //imageHiden.WidthRequest = 20;
            //imageHiden.Margin = new Thickness(0, -10, -10, 0);
            //imageA.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> 
            //{ 
            //    { 
            //        "#000000",
            //        "#FF0000"
            //    } 
            //};

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

            containerDateA.Children.Add(stackLayoutIcon);
            containerDateA.Children.Add(LabeltimeA);
            containerDateA.Spacing = 3;
            containerDateA.Margin = new Thickness(60, 0, 0, 0);
            containerDateA.HorizontalOptions = LayoutOptions.FillAndExpand;

            Label HiddenMess = new Label
            {
                TextColor = currentResource,
                Text = AppResources.Hide_,
                IsVisible = !message.IsHidden,
                Margin = new Thickness(0,0,15,0),
                HorizontalOptions = LayoutOptions.EndAndExpand,
                TextDecorations = TextDecorations.Underline,
                FontSize = 15
            };

            var hideMess = new TapGestureRecognizer();

            hideMess.Tapped += async (s, e) =>
            {
                bool answer = await p.DisplayAlert("",
                    AppResources.AreYouHide,
                    AppResources.Yes, AppResources.No);
                if (answer)
                {
                    CommonResult changeMessageVisibility =
                        await _server.ChangeMessageVisibility(reqId, message.ID.ToString(), true);
                    if (changeMessageVisibility.Error == null)
                    {
                        LabelTextA.FormattedText = Settings.FormatedLink(message.Text, Color.Black);
                        imageHiden.IsVisible = true;
                        frameTextA.SetAppThemeColor(Frame.BorderColorProperty, currentResource, Color.Transparent);
                        frameTextA.BackgroundColor = Color.FromHex("#EBEBEB");
                        LabelTextA.TextColor = Color.Black;
                        LabeltimeA.Margin = new Thickness(0, -5, 5, 0);
                        HiddenMess.IsVisible = false;
                    }
                    else
                    {
                        await p.DisplayAlert(AppResources.Error, changeMessageVisibility.Error, "OK");
                    }
                }
            };
            HiddenMess.GestureRecognizers.Add(hideMess);
            
            ConteinerA.Children.Add(HiddenMess);
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
            if (message.IsHidden)
            {
                LabelTextA.FormattedText = Settings.FormatedLink(message.Text, Color.Black);
            }
            else
            {
                LabelTextA.FormattedText = Settings.FormatedLink(message.Text, Color.White);
            }
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

            //image.IsVisible = message.FileID != -1;
            //image.HorizontalOptions = LayoutOptions.CenterAndExpand;
            //image.HeightRequest = 40;
            //image.WidthRequest = 40;
            //image.Foreground = Color.White;
            //image.Source = "ic_file_download";

            image.IsVisible = message.FileID != -1;
            image.HorizontalOptions = LayoutOptions.CenterAndExpand;
            image.HeightRequest = 40;
            image.WidthRequest = 40;
            image.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", $"#FFFFFF" } };
            image.Source = "resource://xamarinJKH.Resources.ic_file_download.svg";

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

                };
                image.GestureRecognizers.Add(tgr);
            }

            LabelText.FontSize = 15;
            LabelText.HorizontalTextAlignment = TextAlignment.Start;
            LabelText.HorizontalOptions = LayoutOptions.Start;

            //if (Device.RuntimePlatform == Device.Android) 
            //    LabelText.TextType = TextType.Html;

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
            if(message.IsHidden)
                LabelText.FormattedText = Settings.FormatedLink(message.Text, Color.Black);
            else
            {
                LabelText.FormattedText = Settings.FormatedLink(message.Text, Color.White);  
            }
            var link = new TapGestureRecognizer();
            link.Tapped += async (s, e) => { await Settings.OpenLinksMessage(message, p); };
            LabelText.GestureRecognizers.Add(link);
            Labeltime.Text = message.TimeAdd;


            Children.Add(Container);
        }
    }
}