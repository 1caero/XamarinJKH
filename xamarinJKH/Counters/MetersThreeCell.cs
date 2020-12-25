using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using xamarinJKH.CustomRenderers;
using xamarinJKH.DialogViews;
using xamarinJKH.Server;
using xamarinJKH.Utils;
using xamarinJKH.Server.RequestModel;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms.Markup;

namespace xamarinJKH.Main
{
    public class MetersThreeCell : StackLayout
    {
        private Image img = new Image();
        private SvgCachedImage Edit = new SvgCachedImage();
        private Label resource = new Label();
        private Label adress = new Label();
        private Label number = new Label();
        private Label checkup_date = new Label();
        private Label recheckup = new Label();
        private Label tarif1 = new Label();
        private StackLayout tarif1Stack = new StackLayout();
        //private Label tarif1t2 = new Label();
        //private Label tarif1t3 = new Label();

        private StackLayout count1t2Stack = new StackLayout() { IsVisible = false };
        private StackLayout count1t3Stack = new StackLayout() { IsVisible = false };
        private Label counterDate1 = new Label();
        private Label count1 = new Label();
        private Label count1t2 = new Label();
        private Label count1t3 = new Label();
        private Label tarif2 = new Label();
        private StackLayout tarif2Stack = new StackLayout();
        //private Label tarif2t2 = new Label();
        //private Label tarif2t3 = new Label();
        private StackLayout count2t2Stack = new StackLayout() { IsVisible = false };
        private StackLayout count2t3Stack = new StackLayout() { IsVisible = false };
        private Label counterDate2 = new Label();
        private Label count2 = new Label();
        private Label count2t2 = new Label();
        private Label count2t3 = new Label();
        //private Label tarif3 = new Label();
        private StackLayout tarif3Stack = new StackLayout();
        //private Label tarif3t2 = new Label();
        //private Label tarif3t3 = new Label();

        private StackLayout count3t2Stack = new StackLayout() { IsVisible = false };
        private StackLayout count3t3Stack = new StackLayout() { IsVisible = false };
        private Label counterDate3 = new Label();
        private Label count3 = new Label();
        private Label count3t2 = new Label();
        private Label count3t3 = new Label();
        StackLayout containerBtn = new StackLayout();
        private Label canCount = new Label();
        Frame frameBtn = new Frame();

        StackLayout count1Stack = new StackLayout();
        StackLayout count2Stack = new StackLayout();
        StackLayout count3Stack = new StackLayout();

        private Label labelЗPeriod = new Label();
        private Label editLabel = new Label();

        private Label labelDisable = new Label();

        MaterialFrame frame = new MaterialFrame();

        public MeterInfo meterInfo { get; set; }      

        public MetersThreeCell(/*List<MeterValueInfo> Values, int DecimalPoint, int MeterID, bool IsDisabled, string Resource, string Address,
            string CustomName, string UniqueNum, string Units, string CheckupDate, string RecheckInterval,*/ MeterInfo mInfo)
        {
            meterInfo = mInfo;

            frame.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"],
                Color.White);
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frame.SetOnAppTheme(MaterialFrame.ElevationProperty, 0, 20);
            frame.Margin = new Thickness(10, 0, 10, 10);
            frame.Padding = new Thickness(15, 15, 15, 15);
            frame.CornerRadius = 30;

            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Vertical;

            StackLayout header = new StackLayout();
            header.Orientation = StackOrientation.Horizontal;
            header.HorizontalOptions = LayoutOptions.Center;

            resource.FontSize = 15;
            resource.TextColor = Color.Black;
            resource.HorizontalTextAlignment = TextAlignment.Center;

            img.WidthRequest = 25;
            // imgEdit.WidthRequest = 20;
            // imgEdit.HeightRequest = 20;
            // imgEdit.Source = "edit";
            // imgEdit.Foreground = (Color)Application.Current.Resources["MainColor"];

            Edit = new SvgCachedImage();
            Edit.WidthRequest = 25;
            Edit.HeightRequest = 25;
            Edit.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}" } };
            Edit.Source = "resource://xamarinJKH.Resources.edit.svg";

            header.Children.Add(img);
            header.Children.Add(resource);
            header.Children.Add(Edit);

            StackLayout addressStack = new StackLayout();
            addressStack.Orientation = StackOrientation.Horizontal;
            addressStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            

            StackLayout grid = new StackLayout();
            grid.Orientation = StackOrientation.Horizontal;
            grid.HorizontalOptions = LayoutOptions.FillAndExpand;


            StackLayout grid0 = new StackLayout();
            grid0.Orientation = StackOrientation.Horizontal;
            grid0.HorizontalOptions = LayoutOptions.FillAndExpand;
            grid0.VerticalOptions = LayoutOptions.Start;


            Label adressLbl = new Label();
            adressLbl.Text = $"{AppResources.Adress}:";
            adressLbl.FontSize = 15;
            adressLbl.TextColor = Color.FromHex("#A2A2A2");
            adressLbl.HorizontalTextAlignment = TextAlignment.Start;
            adressLbl.HorizontalOptions = LayoutOptions.Fill;
            adressLbl.MinimumWidthRequest = 60;

            grid0.Children.Add(adressLbl);
     
            BoxView b = new BoxView();
            b.VerticalOptions = LayoutOptions.Center;
            b.HeightRequest = 1;
            b.Margin = new Thickness(0, 2, 0, 0);
            b.HorizontalOptions = LayoutOptions.FillAndExpand;
            b.Color = Color.LightGray;
            b.MinimumWidthRequest = 10;

            grid0.Children.Add(b);

            grid.Children.Add(grid0);


            adress.FontSize = 15;
            adress.TextColor = Color.Black;
            adress.HorizontalTextAlignment = TextAlignment.End;
            adress.HorizontalOptions = LayoutOptions.Fill;
            adress.MaxLines = 3;
            if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 700)
            {
                adress.WidthRequest = 450;
                adressLbl.FontSize = 13;
                adress.FontSize = 13;
            }
            else if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                adress.WidthRequest = 500;
                adressLbl.FontSize = 14;
                adress.FontSize = 14;
            }
            else
                adress.WidthRequest = Convert.ToInt32(Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width * 0.7);


            grid.Children.Add(adress);

            container.Children.Add(header);
            container.Children.Add(grid);

            StackLayout numberStack = new StackLayout();
            numberStack.Orientation = StackOrientation.Horizontal;
            numberStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            Label numberLbl = new Label();
            numberLbl.Text = AppResources.FacNum;
            numberLbl.FontSize = 12;
            numberLbl.TextColor = Color.FromHex("#A2A2A2");
            numberLbl.HorizontalTextAlignment = TextAlignment.Start;
            numberLbl.HorizontalOptions = LayoutOptions.Start;
            numberLbl.MaxLines = 1;

            number.FontSize = 12;
            number.HorizontalOptions = LayoutOptions.End;
            number.TextColor = Color.Black;
            number.HorizontalTextAlignment = TextAlignment.End;
            number.MaxLines = 1;

            Label linesNumb = new Label();
            linesNumb.HeightRequest = 1;
            linesNumb.BackgroundColor = Color.LightGray;
            linesNumb.Margin = new Thickness(0, 2, 0, 0);
            linesNumb.VerticalOptions = LayoutOptions.Center;
            linesNumb.HorizontalOptions = LayoutOptions.FillAndExpand;

            numberStack.Children.Add(numberLbl);
            numberStack.Children.Add(linesNumb);
            numberStack.Children.Add(number);
            container.Children.Add(numberStack);

            StackLayout checkupDateStack = new StackLayout();
            checkupDateStack.Orientation = StackOrientation.Horizontal;
            checkupDateStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            checkupDateStack.Margin = new Thickness(0, -7, 0, 0);
            Label checkupDateLbl = new Label();
            checkupDateLbl.Text = AppResources.LastCheck;
            checkupDateLbl.FontSize = 12;
            checkupDateLbl.TextColor = Color.FromHex("#A2A2A2");
            checkupDateLbl.HorizontalTextAlignment = TextAlignment.Start;
            checkupDateLbl.HorizontalOptions = LayoutOptions.Start;
            checkupDateLbl.MaxLines = 1;

            checkup_date.FontSize = 12;
            checkup_date.TextColor = Color.Black;
            checkup_date.HorizontalTextAlignment = TextAlignment.End;
            checkup_date.HorizontalOptions = LayoutOptions.End;
            checkup_date.MaxLines = 1;

            Label linesPover = new Label();
            linesPover.HeightRequest = 1;
            linesPover.BackgroundColor = Color.LightGray;
            ;
            linesPover.VerticalOptions = LayoutOptions.Center;
            linesPover.Margin = new Thickness(0, 2, 0, 0);
            linesPover.HorizontalOptions = LayoutOptions.FillAndExpand;

            checkupDateStack.Children.Add(checkupDateLbl);
            checkupDateStack.Children.Add(linesPover);
            checkupDateStack.Children.Add(checkup_date);
            container.Children.Add(checkupDateStack);

            StackLayout recheckStack = new StackLayout();
            recheckStack.Orientation = StackOrientation.Horizontal;
            recheckStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            recheckStack.Margin = new Thickness(0, -7, 0, 0);
            Label recheckLbl = new Label();
            recheckLbl.Text = AppResources.CheckInterval;
            recheckLbl.FontSize = 12;
            recheckLbl.TextColor = Color.FromHex("#A2A2A2");
            recheckLbl.HorizontalTextAlignment = TextAlignment.Start;
            recheckLbl.HorizontalOptions = LayoutOptions.Start;
            recheckLbl.MaxLines = 1;
            // recheckLbl.WidthRequest = 150;
            recheckup.FontSize = 12;
            recheckup.TextColor = Color.Black;
            recheckup.HorizontalTextAlignment = TextAlignment.End;
            recheckup.HorizontalOptions = LayoutOptions.End;
            recheckup.MaxLines = 1;

            Label linesInterv = new Label();
            linesInterv.HeightRequest = 1;
            linesInterv.BackgroundColor = Color.LightGray;
            ;
            linesInterv.VerticalOptions = LayoutOptions.Center;
            linesInterv.Margin = new Thickness(0, 2, 0, 0);
            linesInterv.HorizontalOptions = LayoutOptions.FillAndExpand;

            recheckStack.Children.Add(recheckLbl);
            recheckStack.Children.Add(linesInterv);
            recheckStack.Children.Add(recheckup);
            container.Children.Add(recheckStack);

            Label separator = new Label();

            separator.HeightRequest = 1;
            separator.BackgroundColor = Color.LightGray;
            separator.Margin = new Thickness(0, 5, 0, 5);
            container.Children.Add(separator);

            tarif1.FontSize = 13;
            tarif1.TextColor = Color.FromHex("#A2A2A2"); //Color.Red;
            tarif1.HorizontalTextAlignment = TextAlignment.Center;

            if (mInfo.TariffNumberInt > 1)
            {                
                    tarif1.Text = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
            }
            else
                tarif1.IsVisible = false;

            //tarif1t2.FontSize = 13;
            //tarif1t2.TextColor = Color.FromHex("#A2A2A2"); //Color.Red;
            //tarif1t2.HorizontalTextAlignment = TextAlignment.Center;

            //tarif1t3.FontSize = 13;
            //tarif1t3.TextColor = Color.FromHex("#A2A2A2"); //Color.Red;
            //tarif1t3.HorizontalTextAlignment = TextAlignment.Center;


            //tarif2.FontSize = 13;
            //tarif2.TextColor = Color.FromHex("#A2A2A2");
            //tarif2.HorizontalTextAlignment = TextAlignment.Center;

            //tarif2.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            //tarif3.FontSize = 13;
            //tarif3.TextColor = Color.FromHex("#A2A2A2");
            //tarif3.HorizontalTextAlignment = TextAlignment.Center;

            //tarif3.Text = string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name;

            count1Stack.Orientation = StackOrientation.Horizontal;
            count1Stack.HorizontalOptions = LayoutOptions.FillAndExpand;
                        
            counterDate1.FontSize = 15;
            counterDate1.TextColor = Color.FromHex("#A2A2A2");
            counterDate1.HorizontalTextAlignment = TextAlignment.Start;
            counterDate1.HorizontalOptions = LayoutOptions.Start;
            counterDate1.MaxLines = 1;
            
            count1.FontSize = 15;
            count1.TextColor = Color.Black;
            count1.HorizontalTextAlignment = TextAlignment.End;
            count1.HorizontalOptions = LayoutOptions.End;
            count1.VerticalOptions = LayoutOptions.Start;
            count1.MaxLines = 1;

            count1t2.FontSize = 15;
            count1t2.TextColor = Color.Black;
            count1t2.HorizontalTextAlignment = TextAlignment.End;
            count1t2.HorizontalOptions = LayoutOptions.End;
            count1t2.VerticalOptions = LayoutOptions.Start;
            count1t2.MaxLines = 1;

      
            //#if DEBUG
            //            count1t2.Text = "100500.111";
            //#endif                      

            count1t3.FontSize = 15;
            count1t3.TextColor = Color.Black;
            count1t3.HorizontalTextAlignment = TextAlignment.End;
            count1t3.HorizontalOptions = LayoutOptions.End;
            count1t3.VerticalOptions = LayoutOptions.Start;
            count1t3.MaxLines = 1;

            
            //#if DEBUG
            //            count1t3.Text = "100502.213";
            //#endif

            BoxView lines = new BoxView();
            lines.HeightRequest = 1;
            //lines.BackgroundColor = Color.LightGray;
            lines.Color= Color.LightGray;
            lines.VerticalOptions = LayoutOptions.Center;
            lines.HorizontalOptions =  LayoutOptions.FillAndExpand;

            count1Stack.Children.Add(counterDate1);
            count1Stack.Children.Add(lines);
            count1Stack.Children.Add(tarif1);
            count1Stack.Children.Add(count1);           

            container.Children.Add(count1Stack);

            count1t2Stack.Orientation = StackOrientation.Horizontal;
            count1t2Stack.HorizontalOptions = LayoutOptions.End;

            var t21 = new Label() { FontSize = 13 , TextColor = Color.FromHex("#A2A2A2") , HorizontalTextAlignment = TextAlignment.Center };
            t21.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            count1t2Stack.Children.Add(t21);
            count1t2Stack.Children.Add(count1t2);
                        
            container.Children.Add(count1t2Stack);

            count1t3Stack.Orientation = StackOrientation.Horizontal;
            count1t3Stack.HorizontalOptions = LayoutOptions.End;

            var t31 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t31.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            count1t3Stack.Children.Add(t31);
            count1t3Stack.Children.Add(count1t3);

            container.Children.Add(count1t3Stack);

            count2Stack.Orientation = StackOrientation.Horizontal;
            count2Stack.HorizontalOptions = LayoutOptions.FillAndExpand;
            counterDate2.FontSize = 15;
            counterDate2.TextColor = Color.FromHex("#A2A2A2");
            counterDate2.HorizontalTextAlignment = TextAlignment.Start;
            counterDate2.HorizontalOptions = LayoutOptions.Start;
            counterDate2.MaxLines = 1;
            // counterDate2.WidthRequest = 150;
            count2.FontSize = 15;
            count2.TextColor = Color.Black;
            count2.HorizontalTextAlignment = TextAlignment.End;
            count2.HorizontalOptions = LayoutOptions.End;
            count2.VerticalOptions = LayoutOptions.Center;
            count2.MaxLines = 1;

            count2t2.FontSize = 15;
            count2t2.TextColor = Color.Black;
            count2t2.HorizontalTextAlignment = TextAlignment.End;
            count2t2.HorizontalOptions = LayoutOptions.End;
            count2t2.VerticalOptions = LayoutOptions.Start;
            count2t2.MaxLines = 1;

//#if DEBUG
//            count2t2.Text = "200500.111";
//#endif                      

            count2t3.FontSize = 15;
            count2t3.TextColor = Color.Black;
            count2t3.HorizontalTextAlignment = TextAlignment.End;
            count2t3.HorizontalOptions = LayoutOptions.End;
            count2t3.VerticalOptions = LayoutOptions.Start;
            count2t3.MaxLines = 1;
//#if DEBUG
//            count2t3.Text = "200502.213";
//#endif


            Label lines2 = new Label();
            lines2.HeightRequest = 1;
            lines2.BackgroundColor = Color.LightGray;
            lines2.VerticalOptions = LayoutOptions.Center;
            lines2.HorizontalOptions = LayoutOptions.FillAndExpand;

            count2Stack.Children.Add(counterDate2);
            count2Stack.Children.Add(lines2);

            //count2Stack.Children.Add(tarif1);
            Label t1 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center};
            if (mInfo.TariffNumberInt > 1)
                if (!string.IsNullOrWhiteSpace(mInfo.Tariff1Name))
                    t1.Text = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
                else
                    t1.IsVisible = false;
            
            count2Stack.Children.Add(t1 );
            //tarif1.FontSize = 13;
            //tarif1.TextColor = Color.FromHex("#A2A2A2"); //Color.Red;
            //tarif1.HorizontalTextAlignment = TextAlignment.Center;

            //if (mInfo.TariffNumberInt > 1)
            //    if (!string.IsNullOrWhiteSpace(mInfo.Tariff1Name))
            //        tarif1.Text = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
            //    else
            //        tarif1.IsVisible = false;

            count2Stack.Children.Add(count2);
            container.Children.Add(count2Stack);

            count2t2Stack.Orientation = StackOrientation.Horizontal;
            count2t2Stack.HorizontalOptions = LayoutOptions.End;

            var t22 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t22.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            count2t2Stack.Children.Add(t22);
            count2t2Stack.Children.Add(count2t2);

            container.Children.Add(count2t2Stack);

            count2t3Stack.Orientation = StackOrientation.Horizontal;
            count2t3Stack.HorizontalOptions = LayoutOptions.End;

            var t32 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t32.Text = string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name;

            count2t3Stack.Children.Add(t32);
            count2t3Stack.Children.Add(count2t3);

            container.Children.Add(count2t3Stack);
           

            count3Stack.Orientation = StackOrientation.Horizontal;
            count3Stack.HorizontalOptions = LayoutOptions.FillAndExpand;
            counterDate3.FontSize = 15;
            counterDate3.TextColor = Color.FromHex("#A2A2A2");
            counterDate3.HorizontalTextAlignment = TextAlignment.Start;
            counterDate3.HorizontalOptions = LayoutOptions.Start;
            counterDate3.MaxLines = 1;
            // counterDate3.WidthRequest = 150;
            count3.FontSize = 15;
            count3.TextColor = Color.Black;
            count3.HorizontalTextAlignment = TextAlignment.End;
            count3.HorizontalOptions = LayoutOptions.End;
            count3.VerticalOptions = LayoutOptions.Center;
            count3.MaxLines = 1;

            count3t2.FontSize = 15;
            count3t2.TextColor = Color.Black;
            count3t2.HorizontalTextAlignment = TextAlignment.End;
            count3t2.HorizontalOptions = LayoutOptions.End;
            count3t2.VerticalOptions = LayoutOptions.Start;
            count3t2.MaxLines = 1;

//#if DEBUG
//            count3t2.Text = "100500.111";
//#endif                      

            count3t3.FontSize = 15;
            count3t3.TextColor = Color.Black;
            count3t3.HorizontalTextAlignment = TextAlignment.End;
            count3t3.HorizontalOptions = LayoutOptions.End;
            count3t3.VerticalOptions = LayoutOptions.Start;
            count3t3.MaxLines = 1;
//#if DEBUG
//            count3t3.Text = "100502.213";
//#endif

            Label lines3 = new Label();
            lines3.HeightRequest = 1;
            lines3.BackgroundColor = Color.LightGray;            
            lines3.VerticalOptions = LayoutOptions.Center;
            lines3.HorizontalOptions = LayoutOptions.FillAndExpand;

            count3Stack.Children.Add(counterDate3);
            count3Stack.Children.Add(lines3);

            Label t2 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            if (mInfo.TariffNumberInt > 1)
                if (!string.IsNullOrWhiteSpace(mInfo.Tariff1Name))
                    t2.Text = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
                else
                    t2.IsVisible = false;

            count3Stack.Children.Add(t2);

            //count3Stack.Children.Add(tarif1);
            count3Stack.Children.Add(count3);
            container.Children.Add(count3Stack);

            count3t2Stack.Orientation = StackOrientation.Horizontal;
            count3t2Stack.HorizontalOptions = LayoutOptions.End;

            var t23 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t23.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            
            count3t2Stack.Children.Add(t23);
            count3t2Stack.Children.Add(count3t2);

            container.Children.Add(count3t2Stack);

            count3t3Stack.Orientation = StackOrientation.Horizontal;
            count3t3Stack.HorizontalOptions = LayoutOptions.End;

            var t33 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t33.Text = string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name;

            count3t3Stack.Children.Add(t33);
            count3t3Stack.Children.Add(count3t3);

            container.Children.Add(count3t3Stack);

            //container.Children.Add(count3t2);
            //container.Children.Add(count3t3);

            frameBtn.HorizontalOptions = LayoutOptions.FillAndExpand;
            frameBtn.VerticalOptions = LayoutOptions.Start;
            frameBtn.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            frameBtn.CornerRadius = 10;
            frameBtn.Margin = new Thickness(0, 10, 0, 0);
            frameBtn.Padding = 12;
            frameBtn.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frameBtn.SetOnAppTheme(MaterialFrame.ElevationProperty, 0, 20);

            containerBtn.Orientation = StackOrientation.Horizontal;
            containerBtn.HorizontalOptions = LayoutOptions.CenterAndExpand;

            Label btn = new Label();
            btn.Margin = new Thickness(0, 0, 0, 0);
            btn.TextColor = Color.White;
            btn.FontAttributes = FontAttributes.Bold;
            btn.VerticalTextAlignment = TextAlignment.Center;
            btn.FontSize = 15;
            btn.Text = AppResources.PassPenance;
            containerBtn.Children.Add(new SvgCachedImage
            {
                Source= "resource://xamarinJKH.Resources.ic_counter.svg",
                ReplaceStringMap = new Dictionary<string, string> { { "#000000","#FFFFFF"} },
                HeightRequest = 20
            });
            containerBtn.Children.Add(btn);

            frameBtn.Content = containerBtn;

            container.Children.Add(frameBtn);

            canCount.Text = AppResources.MetersThreeCellCanCount;
            canCount.FontSize = 12;
            canCount.TextDecorations = TextDecorations.Underline;
            canCount.FontAttributes = FontAttributes.Bold;
            canCount.TextColor = Color.Black;
            canCount.HorizontalTextAlignment = TextAlignment.End;
            canCount.HorizontalOptions = LayoutOptions.CenterAndExpand;
            canCount.HorizontalTextAlignment = TextAlignment.Center;

            labelDisable = new Label()
            {
                Text = AppResources.CounterLeave,
                FontSize = 14,
                IsVisible = false,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            
            container.Children.Add(canCount);
            container.Children.Add(labelDisable);
            frame.Content = container;

            ext(mInfo, mInfo.Values, mInfo.NumberOfDecimalPlaces, mInfo.ID, mInfo.IsDisabled, mInfo.Resource, mInfo.Address,
             mInfo.CustomName, mInfo.FactoryNumber, mInfo.UniqueNum, mInfo.Units, mInfo.NextCheckupDate, mInfo.RecheckInterval.ToString(),mInfo.Tariff1Name, mInfo.Tariff2Name, mInfo.Tariff3Name);

           Children.Add(frame);
        }

        void SetEditButton(string Period, MeterInfo mInfo)
        {
            frameBtn.IsVisible = false;
            var stack = frame.Content as StackLayout;
            try
            {
                int currDay = DateTime.Now.Day;
                if (mInfo.ValuesCanAdd)
                {
                    int indexOf = stack.Children.IndexOf(editLabel);
                    int index = stack.Children.IndexOf(labelЗPeriod);
                    int indexframeBtn = stack.Children.IndexOf(frameBtn);
                    if (indexframeBtn != -1)
                        stack.Children.RemoveAt(indexframeBtn);
                    if (indexOf != -1 && index != -1)
                    {
                        stack.Children.RemoveAt(indexOf);
                        stack.Children.RemoveAt(index);
                    }

                    labelЗPeriod = new Label()
                    {
                        Text = $"{AppResources.PenencePassed} {Period}",
                        FontSize = 14,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    };
                    stack.Children.Add(labelЗPeriod);
                    editLabel = new Label()
                    {
                        Text = AppResources.ChangePenance,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = (Color)Application.Current.Resources["MainColor"],
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    };
                    stack.Children.Add(editLabel);
                    
                }
            }
            catch (Exception e)
            {
            }
        }

        private static bool CheckPeriod(int currDay, MeterInfo meterInfo)
        {
//#if DEBUG
//            return true;
//#endif
            if (meterInfo.ValuesEndDay < meterInfo.ValuesStartDay)
            {
                return MetersThreeCell.GetPeriodEnabled() || (meterInfo.ValuesStartDay == 0 &&
                                                              meterInfo.ValuesEndDay == 0);

            }
            
            return (meterInfo.ValuesStartDay <= currDay &&
                    meterInfo.ValuesEndDay >= currDay) ||
                   (meterInfo.ValuesStartDay == 0 &&
                    meterInfo.ValuesEndDay == 0);
        }

        public static bool GetPeriodEnabled()
        {
            DateTime now = DateTime.Now;
            bool flag = false;
            for (int i = 1; i <= 12; i++)
            {
                var nowYear = DateTime.Now.Year;
                DateTime starDay = new DateTime(nowYear, i, Settings.Person.Accounts[0].MetersStartDay); ;
                int month = i+1;
                // Проверяем переход на следующий год ( декабрь -> январь)
                if (i == 12)
                {
                    month = 1;
                    nowYear += 1;
                }
                DateTime endDay = new DateTime(nowYear, month, Settings.Person.Accounts[0].MetersEndDay);
                if (now >= starDay && now <= endDay)
                {
                    return true;
                }
            }

            return flag;
        }
        
        string GetFormat(int DecimalPoint)
        {
            var dec = DecimalPoint;
            switch (dec)
            {
                case 0: return "{0:0}";
                case 1: return "{0:0.0}";
                case 2: return "{0:0.00}";
                case 3: return "{0:0.000}";
            }

            return "{0:0.000}";
        }

        void ext(MeterInfo mInfo, List<MeterValueInfo> Values, int DecimalPoint, int MeterID, bool IsDisabled, string Resource, string Address,
            string CustomName, string FactoryNumber, string UniqueNum, string Units, string CheckupDate, string RecheckInterval, string Tariff1Name, string Tariff2Name, string Tariff3Name)
        {   
            
                var editName = new TapGestureRecognizer();
                editName.Tapped += async (s, e) =>
                {
                    if (PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x is EditCounterNameDialog) == null)
                    {
                        await PopupNavigation.Instance.PushAsync(
                            new EditCounterNameDialog((Color) Application.Current.Resources["MainColor"], UniqueNum));
                    }
                };
                if (Edit.GestureRecognizers.Count > 0)
                {
                    Edit.GestureRecognizers[0] = editName;
                }
                else
                {
                    Edit.GestureRecognizers.Add(editName);
                }

                string name = (!string.IsNullOrWhiteSpace(CustomName)) ? CustomName : Resource;

                FormattedString formattedResource = new FormattedString();
                formattedResource.Spans.Add(new Span
                {
                    Text = name + ", " + Units,
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18
                });

                resource.FormattedText = formattedResource;
                adress.Text = Address;
                number.Text = FactoryNumber;
                checkup_date.Text = CheckupDate;
                recheckup.Text = RecheckInterval + " лет";
                GetFormat( DecimalPoint);

            if (Values.Count == 1)
            {
                counterDate1.Text = Values[0].Period.Equals("01.01.0001")? "-" : Values[0].Period;
                count1.Text =
                    String.Format(GetFormat(DecimalPoint), Values[0].Value);
                                
                if (Values[0].ValueT2 != null)
                {
                    count1t2.Text = String.Format(GetFormat(DecimalPoint), Values[0].ValueT2); count1t2Stack.IsVisible = true;
                }
                else
                {
                    
                }

                if (Values[0].ValueT3 != null)
                {
                    count1t3.Text = String.Format(GetFormat(DecimalPoint), Values[0].ValueT3);
                    count1t3Stack.IsVisible = true;
                }
                else
                {
                    count1t3Stack.IsVisible = false;                    
                }

                count2Stack.IsVisible = count3Stack.IsVisible = false;
            }
            else if (Values.Count == 2)
            {
                counterDate1.Text = Values[0].Period.Equals("01.01.0001")? "-" : Values[0].Period;
                count1.Text =
                    String.Format(GetFormat(DecimalPoint), Values[0].Value); //.ToString(CultureInfo.InvariantCulture);

                if (Values[0].ValueT2 != null)
                {
                    count1t2.Text = String.Format(GetFormat(DecimalPoint), Values[0].ValueT2);
                    count1t2Stack.IsVisible = true;
                }
                else
                {
                    count1t2Stack.IsVisible = false;
                }

                if (Values[0].ValueT3 != null)
                {
                    count1t3.Text = String.Format(GetFormat(DecimalPoint), Values[0].ValueT3);
                    count1t3Stack.IsVisible = true;
                }
                else
                {
                    count1t3Stack.IsVisible = false;
                }

                counterDate2.Text = Values[1].Period.Equals("01.01.0001")? "-" : Values[1].Period;
                count2.Text =
                    String.Format(GetFormat(DecimalPoint), Values[1].Value); //.ToString(CultureInfo.InvariantCulture);

                if (Values[1].ValueT2 != null)
                {
                    count2t2.Text = String.Format(GetFormat(DecimalPoint), Values[1].ValueT2);
                    count2t2Stack.IsVisible = true;
                }
                else
                {
                    count2t2Stack.IsVisible = false;
                }

                if (Values[1].ValueT3 != null)
                { count2t3.Text = String.Format(GetFormat(DecimalPoint), Values[1].ValueT3);
                    count2t3Stack.IsVisible = true;
                }
                else
                {
                    count2t3Stack.IsVisible = false;
                }

                count3Stack.IsVisible = false;
            }
            else if (Values.Count == 3)
            {
                counterDate1.Text = Values[0].Period.Equals("01.01.0001")? "-" : Values[0].Period;
                count1.Text =
                    String.Format(GetFormat(DecimalPoint), Values[0].Value);

                if (Values[0].ValueT2 != null)
                {
                    count1t2.Text = String.Format(GetFormat(DecimalPoint), Values[0].ValueT2);
                    count1t2Stack.IsVisible = true;
                }
                else
                {
                    count1t2Stack.IsVisible = false;
                }

                if (Values[0].ValueT3 != null)
                {
                    count1t3.Text = String.Format(GetFormat(DecimalPoint), Values[0].ValueT3);
                    count1t3Stack.IsVisible = true;
                }
                else
                {
                    count1t3Stack.IsVisible = false;
                }

                counterDate2.Text = Values[1].Period.Equals("01.01.0001")? "-" : Values[1].Period;
                count2.Text =
                    String.Format(GetFormat(DecimalPoint), Values[1].Value); //.ToString(CultureInfo.InvariantCulture);

                if (Values[1].ValueT2 != null)
                {
                    count2t2.Text = String.Format(GetFormat(DecimalPoint), Values[1].ValueT2);
                    count2t2Stack.IsVisible = true;
                }
                else
                {
                    count2t2Stack.IsVisible = false;
                }
                if (Values[1].ValueT3 != null)
                {
                    count2t3.Text = String.Format(GetFormat(DecimalPoint), Values[1].ValueT3);
                    count2t3Stack.IsVisible = true;
                }
                else
                {
                    count2t3Stack.IsVisible = false;

                    //count2t3.IsVisible = false;
                }

                counterDate3.Text = Values[2].Period.Equals("01.01.0001")? "-" : Values[2].Period;
                count3.Text =
                    String.Format(GetFormat(DecimalPoint), Values[2].Value); //.ToString(CultureInfo.InvariantCulture);

                if (Values[2].ValueT2 != null)
                {
                    count3t2.Text = String.Format(GetFormat(DecimalPoint), Values[2].ValueT2);
                    count3t2Stack.IsVisible = true;
                }
                else
                {
                    count3t2Stack.IsVisible = false;
                }

                if (Values[2].ValueT3 != null)
                {
                    count3t3.Text = String.Format(GetFormat(DecimalPoint), Values[2].ValueT3);
                    count3t3Stack.IsVisible = true;
                }
                else
                {
                    count3t3Stack.IsVisible = false;
                }
            }
            else if (Values.Count == 0)
            {
                count1Stack.IsVisible = count2Stack.IsVisible = count3Stack.IsVisible = false;
                //count1t2Stack.IsVisible = count1t3Stack.IsVisible = count2t2Stack.IsVisible = count2t3Stack.IsVisible = count3t2Stack.IsVisible = count3t3Stack.IsVisible = false; 
            }

                if (IsDisabled)
                {
                    labelDisable.IsVisible = true;
                    Edit.IsVisible = false;
                    try
                    {
                        var stack = frame.Content as StackLayout;
                        stack.Children.RemoveAt(stack.Children.IndexOf(frameBtn));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                }else if (Values.Count > 0 && Values[0].IsCurrentPeriod && !meterInfo.AutoValueGettingOnly)
                {
                    SetEditButton(Values[0].Period, mInfo);
                    SetDellValue(MeterID, mInfo);
                }
                else
                {
                    frameBtn.IsVisible = true;
                    var stack = frame.Content as StackLayout;
                    try
                    {
                        stack.Children.RemoveAt(stack.Children.IndexOf(editLabel));
                        stack.Children.RemoveAt(stack.Children.IndexOf(labelЗPeriod));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    stack.Children.Add(frameBtn);
                    if (Values.Count > 0)
                     {
                        Label lines = new Label();
                        lines.HeightRequest = 1;
                        lines.BackgroundColor = Color.LightGray;
                        lines.VerticalOptions = LayoutOptions.Center;
                        lines.HorizontalOptions = LayoutOptions.FillAndExpand;
                        count1Stack.Children[1] = lines;
                    }
                }


                if (Resource.ToLower().Contains("холодное") || Resource.ToLower().Contains("хвс"))
                {
                    img.Source = ImageSource.FromFile("ic_cold_water");
                }
                else if (Resource.ToLower().Contains("горячее") || Resource.ToLower().Contains("гвс") ||
                         Resource.ToLower().Contains("подог") || Resource.ToLower().Contains("отопл"))
                {
                    img.Source = ImageSource.FromFile("ic_heat_water");
                }
                else if (Resource.ToLower().Contains("эле"))
                {
                    img.Source = ImageSource.FromFile("ic_electr");
                }
                else
                {
                    img.Source = ImageSource.FromFile("ic_cold_water");
                }

                string month = AppResources.CountersCurrentMonth;
                if (mInfo.ValuesEndDay < mInfo.ValuesStartDay)
                {
                    month = AppResources.NextMonth;
                }
                int currDay = DateTime.Now.Day;
                // currDay = 16;
                frameBtn.IsVisible = true;
                canCount.IsVisible = false;
                if (Settings.Person != null)
                    if (Settings.Person.Accounts != null)
                        if (Settings.Person.Accounts.Count > 0)
                        {
                            FormattedString formattedDate = new FormattedString();
                            formattedDate.Spans.Add(new Span
                            {
                                Text = $"{AppResources.CountersInfo1} ",
                                TextColor = (Color)Application.Current.Resources["MainColor"],
                                FontAttributes = FontAttributes.None,
                                FontSize = 12
                            });
                            if (mInfo.ValuesStartDay != null &&
                                mInfo.ValuesEndDay != null)
                            {
                                if (mInfo.ValuesStartDay != 0 &&
                                    mInfo.ValuesEndDay != 0)
                                {
                                    formattedDate.Spans.Add(new Span
                                    {
                                        Text = $"{AppResources.From} " + mInfo.ValuesStartDay +
                                               $" {AppResources.To} " +
                                               mInfo.ValuesEndDay + " числа ",
                                        TextColor = (Color)Application.Current.Resources["MainColor"],
                                        FontAttributes = FontAttributes.Bold,
                                        FontSize = 12
                                    });
                                    formattedDate.Spans.Add(new Span
                                    {
                                        Text = month,
                                        TextColor = (Color)Application.Current.Resources["MainColor"],
                                        FontAttributes = FontAttributes.None,
                                        FontSize = 12
                                    });
                                }
                                else
                                {
                                    formattedDate.Spans.Add(new Span
                                    {
                                        Text = month,
                                        TextColor = (Color)Application.Current.Resources["MainColor"],
                                        FontAttributes = FontAttributes.Bold,
                                        FontSize = 12
                                    });
                                }
                            }
                            else
                            {
                                formattedDate.Spans.Add(new Span
                                {
                                    Text = AppResources.CountersCurrentMonth,
                                    TextColor = (Color)Application.Current.Resources["MainColor"],
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = 12
                                });
                            }

                            canCount.FormattedText = formattedDate;
                            if (mInfo.ValuesCanAdd)
                            {
                                frameBtn.IsVisible = true;
                                canCount.IsVisible = false;
                            }
                            else
                            {
                                frameBtn.IsVisible = false;
                                canCount.IsVisible = true;
                            }
                        }
                if (meterInfo.AutoValueGettingOnly)
                {
                    var stack = frame.Content as StackLayout;
                    var auto_label = new Label
                    {
                        Text = AppResources.AutoPennance,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = (Color)Application.Current.Resources["MainColor"],
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    };
                    stack.Children.Add(auto_label);
                    canCount.IsVisible = false;
                    frameBtn.IsVisible = false;
                }
            
        }

        private void SetDellValue(int MeterID, MeterInfo meterInfo)
        {
            if (Settings.Person.Accounts.Count == 0)
            {
                return;
            }
            
            int currentDay = DateTime.Now.Day;
            if (meterInfo.ValuesCanAdd)
            {
                Label del = new Label();
                del.TextColor = (Color) Application.Current.Resources["MainColor"];
                del.Text = "Удалить";
                del.TextDecorations = TextDecorations.Underline;
                del.FontAttributes = FontAttributes.Bold;
                del.VerticalOptions = LayoutOptions.Center;
                del.VerticalTextAlignment = TextAlignment.Center;
                del.HorizontalTextAlignment = TextAlignment.Center;
                del.HorizontalOptions = LayoutOptions.FillAndExpand;
                count1Stack.Children[1] = del;
                var dellClick = new TapGestureRecognizer();
                RestClientMP server = new RestClientMP();
                dellClick.Tapped += async (s, e) =>
                {
                    Configurations.LoadingConfig = new LoadingConfig
                    {
                        IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                        OverlayColor = Color.Black,
                        Opacity = 0.8,
                        DefaultMessage = "",
                    };
                    bool displayAlert = await Settings.mainPage.DisplayAlert("", AppResources.DellCouneter,
                        AppResources.Yes, AppResources.Cancel);
                    if (displayAlert)
                    {
                        await Loading.Instance.StartAsync(async progress =>
                        {
                            CommonResult result = await server.DeleteMeterValue(MeterID);
                            if (result.Error == null)
                            {
                                MessagingCenter.Send<Object>(this, "UpdateCounters");
                            }
                            else
                            {
                                await Settings.mainPage.DisplayAlert(AppResources.ErrorTitle, result.Error,
                                    "OK");
                            }

                            // });
                        });
                    }
                };
                del.GestureRecognizers.Add(dellClick);
            }
        }
    }
}