using System;
using System.Collections.Generic;
using System.Linq;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using FFImageLoading.Svg.Forms;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.CustomRenderers;
using xamarinJKH.DialogViews;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

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
        private Label del = new Label();


        private StackLayout count1t2Stack = new StackLayout() { IsVisible = false };
        private StackLayout count1t3Stack = new StackLayout() { IsVisible = false };
        private Label counterDate1 = new Label();
        private Label count1 = new Label();
        private Label count1t2 = new Label();
        private Label count1t3 = new Label();
        private Label tarif2 = new Label();
        private StackLayout tarif2Stack = new StackLayout();
        private StackLayout count2t2Stack = new StackLayout() { IsVisible = false };
        private StackLayout count2t3Stack = new StackLayout() { IsVisible = false };
        private Label counterDate2 = new Label();
        private Label count2 = new Label();
        private Label count2t2 = new Label();
        private Label count2t3 = new Label();
        private StackLayout tarif3Stack = new StackLayout();

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
        StackLayout editStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            IsVisible = false,
            Spacing = 3
        }; 
        StackLayout AllPenanseStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 3
        }; 
        StackLayout delStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            IsVisible = false,
            Spacing = 3
        };
        private Label labelЗPeriod = new Label();
        private Label editLabel = new Label();

        private Label labelDisable = new Label();

        MaterialFrame frame = new MaterialFrame();
        Label separator = new Label();

        public MeterInfo meterInfo { get; set; }
        private CountersPage countersPage;
        public MetersThreeCell(/*List<MeterValueInfo> Values, int DecimalPoint, int MeterID, bool IsDisabled, string Resource, string Address,
            string CustomName, string UniqueNum, string Units, string CheckupDate, string RecheckInterval,*/ MeterInfo mInfo, CountersPage countersPage)
        {
            meterInfo = mInfo;
            this.countersPage = countersPage;

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
            resource.VerticalTextAlignment = TextAlignment.Center;
            resource.HorizontalTextAlignment = TextAlignment.Center;

            img.WidthRequest = 25;
            img.HeightRequest = 25;

            Edit = new SvgCachedImage();
            Edit.WidthRequest = 17;
            Edit.HeightRequest = 17;
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
            adressLbl.TextColor = Color.Black;
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


            grid.Children.Add(grid0);


            adress.FontSize = 15;
            adress.TextColor = Color.Black;
            adress.HorizontalTextAlignment = TextAlignment.Start;
            adress.HorizontalOptions = LayoutOptions.Fill;
            adress.FontAttributes = FontAttributes.Bold;
            adress.MaxLines = 3;
            if (DeviceDisplay.MainDisplayInfo.Width < 700)
            {
                adress.WidthRequest = 450;
                adressLbl.FontSize = 13;
                adress.FontSize = 13;
            }
            else if (DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                adress.WidthRequest = 500;
                adressLbl.FontSize = 14;
                adress.FontSize = 14;
            }
            else
                adress.WidthRequest = Convert.ToInt32(DeviceDisplay.MainDisplayInfo.Width * 0.7);


            grid.Children.Add(adress);

            container.Children.Add(header);
            container.Children.Add(grid);

            StackLayout numberStack = new StackLayout();
            numberStack.Orientation = StackOrientation.Horizontal;
            numberStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            Label numberLbl = new Label();
            numberLbl.Text = AppResources.FacNum;
            numberLbl.FontSize = 12;
            numberLbl.TextColor = Color.Black;
            numberLbl.HorizontalTextAlignment = TextAlignment.Start;
            numberLbl.HorizontalOptions = LayoutOptions.Start;
            numberLbl.MaxLines = 1;

            number.FontSize = 11;
            number.HorizontalOptions = LayoutOptions.End;
            number.TextColor = Color.Black;
            number.VerticalOptions = LayoutOptions.Center;
            number.FontAttributes = FontAttributes.Bold;
            number.HorizontalTextAlignment = TextAlignment.End;
            number.MaxLines = 1;

            Label linesNumb = new Label();
            linesNumb.HeightRequest = 1;
            linesNumb.BackgroundColor = Color.LightGray;
            linesNumb.Margin = new Thickness(0, 2, 0, 0);
            linesNumb.VerticalOptions = LayoutOptions.Center;
            linesNumb.HorizontalOptions = LayoutOptions.FillAndExpand;

            numberStack.Children.Add(numberLbl);
            numberStack.Children.Add(number);
            container.Children.Add(numberStack);

            StackLayout checkupDateStack = new StackLayout();
            checkupDateStack.Orientation = StackOrientation.Horizontal;
            checkupDateStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            checkupDateStack.Margin = new Thickness(0, -7, 0, 0);
            Label checkupDateLbl = new Label();
            checkupDateLbl.Text = AppResources.LastCheck;
            checkupDateLbl.FontSize = 12;
            checkupDateLbl.TextColor = Color.Black;
            checkupDateLbl.HorizontalTextAlignment = TextAlignment.Start;
            checkupDateLbl.HorizontalOptions = LayoutOptions.Start;
            checkupDateLbl.MaxLines = 1;

            checkup_date.FontSize = 11;
            checkup_date.TextColor = Color.Black;
            checkup_date.FontAttributes = FontAttributes.Bold;
            checkup_date.HorizontalTextAlignment = TextAlignment.Start;
            checkup_date.VerticalOptions = LayoutOptions.Center;
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
            checkupDateStack.Children.Add(checkup_date);
            container.Children.Add(checkupDateStack);

            StackLayout recheckStack = new StackLayout();
            recheckStack.Orientation = StackOrientation.Horizontal;
            recheckStack.HorizontalOptions = LayoutOptions.FillAndExpand;
            recheckStack.Margin = new Thickness(0, -7, 0, 0);
            Label recheckLbl = new Label();
            recheckLbl.Text = AppResources.CheckInterval;
            recheckLbl.FontSize = 12;
            recheckLbl.TextColor = Color.Black;
            recheckLbl.HorizontalTextAlignment = TextAlignment.Start;
            recheckLbl.HorizontalOptions = LayoutOptions.Start;
            recheckLbl.MaxLines = 1;
            recheckup.FontSize = 12;
            recheckup.TextColor = Color.Black;
            recheckup.FontAttributes = FontAttributes.Bold;
            recheckup.HorizontalTextAlignment = TextAlignment.Start;
            recheckup.VerticalOptions = LayoutOptions.Center;
            recheckup.HorizontalOptions = LayoutOptions.End;
            recheckup.MaxLines = 1;

            Label linesInterv = new Label();
            linesInterv.HeightRequest = 1;
            linesInterv.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            ;
            linesInterv.VerticalOptions = LayoutOptions.Center;
            linesInterv.Margin = new Thickness(0, 2, 0, 0);
            linesInterv.HorizontalOptions = LayoutOptions.FillAndExpand;

            recheckStack.Children.Add(recheckLbl);
            recheckStack.Children.Add(recheckup);
            container.Children.Add(recheckStack);


            separator.HeightRequest = 1;
            separator.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            separator.Margin = new Thickness(0, 5, 0, 5);
            container.Children.Add(separator);

            count1Stack.Orientation = StackOrientation.Horizontal;
            count1Stack.Spacing = 15;
            count1Stack.HorizontalOptions = LayoutOptions.FillAndExpand;

            
            SvgCachedImage EditPenanse = new SvgCachedImage();
            EditPenanse.WidthRequest = 12;
            EditPenanse.HeightRequest = 12;
            EditPenanse.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}" } };
            EditPenanse.Source = "resource://xamarinJKH.Resources.edit.svg";
            
            SvgCachedImage delPenanse = new SvgCachedImage();
            delPenanse.WidthRequest = 12;
            delPenanse.HeightRequest = 12;
            delPenanse.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}" } };
            delPenanse.Source = "resource://xamarinJKH.Resources.ic_close.svg";
            
            editLabel = new Label()
            {
                Text = AppResources.ChangePenance,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextDecorations = TextDecorations.Underline,
                TextColor = (Color)Application.Current.Resources["MainColor"],
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start
            };
            
            
           
            
            SvgCachedImage AllPenanse = new SvgCachedImage();
            AllPenanse.WidthRequest = 12;
            AllPenanse.HeightRequest = 12;
            AllPenanse.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}" } };
            AllPenanse.Source = "resource://xamarinJKH.Resources.ic_all_penance.svg";
            
            Label AllPenanceLabel = new Label()
            {
                Text = AppResources.AllPenance,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextDecorations = TextDecorations.Underline,
                TextColor = (Color)Application.Current.Resources["MainColor"],
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start
            };
            
            AllPenanseStack.Children.Add(AllPenanse);
            AllPenanseStack.Children.Add(AllPenanceLabel);
            
            editStack.Children.Add(EditPenanse);
            editStack.Children.Add(editLabel);
            
            delStack.Children.Add(delPenanse);
            delStack.Children.Add(del);
            
            count1Stack.Children.Add(editStack);
            count1Stack.Children.Add(delStack);
            count1Stack.Children.Add(AllPenanseStack);
            
            tarif1.FontSize = 13;
            tarif1.TextColor = Color.FromHex("#A2A2A2"); //Color.Red;
            tarif1.HorizontalTextAlignment = TextAlignment.Center;

            if (mInfo.TariffNumberInt > 1)
            {                
                    tarif1.Text = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
            }
            else
                tarif1.IsVisible = false;
                        
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

      

            count1t3.FontSize = 15;
            count1t3.TextColor = Color.Black;
            count1t3.HorizontalTextAlignment = TextAlignment.End;
            count1t3.HorizontalOptions = LayoutOptions.End;
            count1t3.VerticalOptions = LayoutOptions.Start;
            count1t3.MaxLines = 1;

        
            BoxView lines = new BoxView();
            lines.HeightRequest = 1;
            lines.Color= Color.LightGray;
            lines.VerticalOptions = LayoutOptions.Center;
            lines.HorizontalOptions =  LayoutOptions.FillAndExpand;

            container.Children.Add(count1Stack);

            count1t2Stack.Orientation = StackOrientation.Horizontal;
            count1t2Stack.HorizontalOptions = LayoutOptions.End;

            var t21 = new Label() { FontSize = 13 , TextColor = Color.FromHex("#A2A2A2") , HorizontalTextAlignment = TextAlignment.Center };
            t21.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            count1t2Stack.Children.Add(t21);
            count1t2Stack.Children.Add(count1t2);
                        
            count1t3Stack.Orientation = StackOrientation.Horizontal;
            count1t3Stack.HorizontalOptions = LayoutOptions.End;

            var t31 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t31.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            count1t3Stack.Children.Add(t31);
            count1t3Stack.Children.Add(count1t3);

            count2Stack.Orientation = StackOrientation.Horizontal;
            count2Stack.HorizontalOptions = LayoutOptions.FillAndExpand;
            counterDate2.FontSize = 15;
            counterDate2.TextColor = Color.FromHex("#A2A2A2");
            counterDate2.HorizontalTextAlignment = TextAlignment.Start;
            counterDate2.HorizontalOptions = LayoutOptions.Start;
            counterDate2.MaxLines = 1;
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


            count2t3.FontSize = 15;
            count2t3.TextColor = Color.Black;
            count2t3.HorizontalTextAlignment = TextAlignment.End;
            count2t3.HorizontalOptions = LayoutOptions.End;
            count2t3.VerticalOptions = LayoutOptions.Start;
            count2t3.MaxLines = 1;

            Label lines2 = new Label();
            lines2.HeightRequest = 1;
            lines2.BackgroundColor = Color.LightGray;
            lines2.VerticalOptions = LayoutOptions.Center;
            lines2.HorizontalOptions = LayoutOptions.FillAndExpand;

            count2Stack.Children.Add(counterDate2);
            count2Stack.Children.Add(lines2);

            Label t1 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center};
            if (mInfo.TariffNumberInt > 1)
                if (!string.IsNullOrWhiteSpace(mInfo.Tariff1Name))
                    t1.Text = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
                else
                    t1.IsVisible = false;
            
            count2Stack.Children.Add(t1 );

            count2Stack.Children.Add(count2);

            count2t2Stack.Orientation = StackOrientation.Horizontal;
            count2t2Stack.HorizontalOptions = LayoutOptions.End;

            var t22 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t22.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            count2t2Stack.Children.Add(t22);
            count2t2Stack.Children.Add(count2t2);


            count2t3Stack.Orientation = StackOrientation.Horizontal;
            count2t3Stack.HorizontalOptions = LayoutOptions.End;

            var t32 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t32.Text = string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name;

            count2t3Stack.Children.Add(t32);
            count2t3Stack.Children.Add(count2t3);

           

            count3Stack.Orientation = StackOrientation.Horizontal;
            count3Stack.HorizontalOptions = LayoutOptions.FillAndExpand;
            counterDate3.FontSize = 15;
            counterDate3.TextColor = Color.FromHex("#A2A2A2");
            counterDate3.HorizontalTextAlignment = TextAlignment.Start;
            counterDate3.HorizontalOptions = LayoutOptions.Start;
            counterDate3.MaxLines = 1;
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
                   

            count3t3.FontSize = 15;
            count3t3.TextColor = Color.Black;
            count3t3.HorizontalTextAlignment = TextAlignment.End;
            count3t3.HorizontalOptions = LayoutOptions.End;
            count3t3.VerticalOptions = LayoutOptions.Start;
            count3t3.MaxLines = 1;

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

            count3Stack.Children.Add(count3);

            count3t2Stack.Orientation = StackOrientation.Horizontal;
            count3t2Stack.HorizontalOptions = LayoutOptions.End;

            var t23 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t23.Text = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;

            
            count3t2Stack.Children.Add(t23);
            count3t2Stack.Children.Add(count3t2);


            count3t3Stack.Orientation = StackOrientation.Horizontal;
            count3t3Stack.HorizontalOptions = LayoutOptions.End;

            var t33 = new Label() { FontSize = 13, TextColor = Color.FromHex("#A2A2A2"), HorizontalTextAlignment = TextAlignment.Center };
            t33.Text = string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name;

            count3t3Stack.Children.Add(t33);
            count3t3Stack.Children.Add(count3t3);

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
            canCount.TextColor = (Color)Application.Current.Resources["MainColor"];
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

        void SetEditButton(string Period, MeterInfo mInfo, List<MeterValueInfo> value, MeterInfo meterInfo)
        {
            var stack = frame.Content as StackLayout;
            try
            {
                int currDay = DateTime.Now.Day;
                if (mInfo.ValuesCanAdd)
                {
                    int indexOf = stack.Children.IndexOf(separator);
                    int index = stack.Children.IndexOf(labelЗPeriod);
                    
                    FormattedString formattedDateLastValue = new FormattedString();
                    formattedDateLastValue.Spans.Add(new Span
                    {
                        Text = $"{AppResources.LastPenanse} {Period}: ",
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.None,
                        FontSize = 14
                    });
                    if (meterInfo.TariffNumberInt == 1)
                    {
                        formattedDateLastValue.Spans.Add(new Span
                        {
                            Text = $"{value[0].Value}",
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 14
                        });
                    }
                    else if (meterInfo.TariffNumberInt == 2)
                    {
                        String tarif1 = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
                        String tarif2 = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;
                        formattedDateLastValue.Spans.Add(new Span
                        {
                            Text = $"\n{tarif1} - {value[0].Value}\n",
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 14
                        }); 
                        formattedDateLastValue.Spans.Add(new Span
                        {
                            Text = $"{tarif2} - {value[0].ValueT2}",
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 14
                        });
                    }else if (meterInfo.TariffNumberInt == 3)
                    {
                        String tarif1 = string.IsNullOrWhiteSpace(mInfo.Tariff1Name) ? AppResources.tarif1 : mInfo.Tariff1Name;
                        String tarif2 = string.IsNullOrWhiteSpace(mInfo.Tariff2Name) ? AppResources.tarif2 : mInfo.Tariff2Name;
                        String tarif3 = string.IsNullOrWhiteSpace(mInfo.Tariff3Name) ? AppResources.tarif3 : mInfo.Tariff3Name;
                        formattedDateLastValue.Spans.Add(new Span
                        {
                            Text = $"\n{tarif1} - {value[0].Value}\n",
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 14
                        }); 
                        formattedDateLastValue.Spans.Add(new Span
                        {
                            Text = $"{tarif2} - {value[0].ValueT2}\n",
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 14
                        }); 
                        formattedDateLastValue.Spans.Add(new Span
                        {
                            Text = $"{tarif3} - {value[0].ValueT3}",
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 14
                        });
                    }
                    

                    labelЗPeriod = new Label()
                    {
                        FormattedText = formattedDateLastValue,
                        FontSize = 14,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Start
                    };
                    stack.Children.Insert(indexOf + 1,labelЗPeriod);
                    if (value[0].IsCurrentPeriod)
                    {
                        editStack.IsVisible = true;
                        frameBtn.IsVisible = false;
                        canCount.IsVisible = true;
                        int indexframeBtn = stack.Children.IndexOf(frameBtn);
                        if (indexframeBtn != -1)
                            stack.Children.RemoveAt(indexframeBtn);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private static bool CheckPeriod(int currDay, MeterInfo meterInfo)
        {
            if (meterInfo.ValuesEndDay < meterInfo.ValuesStartDay)
            {
                return GetPeriodEnabled() || (meterInfo.ValuesStartDay == 0 &&
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
                
                var meterValues = new TapGestureRecognizer();
                meterValues.Tapped += async (s, e) =>
                {
                    await PopupNavigation.Instance.PushAsync(new MeterValuesDialog(meterInfo,countersPage));
                };
                AllPenanseStack.GestureRecognizers.Add(meterValues);
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
                    
                }else if (Values.Count > 0 &&  !meterInfo.AutoValueGettingOnly)
                {
                    SetEditButton(Values[0].Period, mInfo, Values, meterInfo);
                    if(Values[0].IsCurrentPeriod)
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
                    }
                }


                if (Resource.ToLower().Contains("холод") || Resource.ToLower().Contains("хвс"))
                {
                    img.Source = ImageSource.FromFile("ic_cold_water");
                }
                else if (Resource.ToLower().Contains("горяч") || Resource.ToLower().Contains("гвс"))
                {
                    img.Source = ImageSource.FromFile("ic_heat_water");
                }else if (Resource.ToLower().Contains("подог") || Resource.ToLower().Contains("отопл")|| Resource.ToLower().Contains("тепл"))
                {
                    img.Source = ImageSource.FromFile("ic_heat_energ");
                }
                else if (Resource.ToLower().Contains("эле"))
                {
                    img.Source = ImageSource.FromFile("ic_electr");
                }else if (Resource.ToLower().Contains("газ"))
                {
                    img.Source = ImageSource.FromFile("ic_gas");
                }
                else
                {
                    img.Source = ImageSource.FromFile("ic_cold_water");
                }
                frameBtn.IsVisible = true;
                if (Settings.Person != null)
                    if (Settings.Person.Accounts != null)
                        if (Settings.Person.Accounts.Count > 0)
                        {
                            canCount.FormattedText = mInfo.PeriodMessage;
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
            if (meterInfo.ValuesCanAdd)
            {
                del.TextColor = (Color) Application.Current.Resources["MainColor"];
                del.Text = AppResources.Delete;
                del.FontSize = 12;
                del.TextDecorations = TextDecorations.Underline;
                del.FontAttributes = FontAttributes.Bold;
                del.VerticalOptions = LayoutOptions.Center;
                del.VerticalTextAlignment = TextAlignment.Center;
                del.HorizontalTextAlignment = TextAlignment.Start;
                del.HorizontalOptions = LayoutOptions.FillAndExpand;
                delStack.IsVisible = true;
                
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