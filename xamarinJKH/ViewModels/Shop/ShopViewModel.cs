using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Shop;

namespace xamarinJKH.ViewModels.Shop
{
    public class ShopViewModel : BaseViewModel
    {
        public ObservableCollection<Goods> Goods { get; set; }
        public ObservableCollection<string> Categories { get; set; }
        public List<Goods> AllGoods { get; set; }
        public Command LoadGoods { get; set; }
        string shopName;
        public string ShopName
        {
            get => shopName;
            set
            {
                shopName = value;
                OnPropertyChanged(nameof(ShopName));
            }
        }
        string selected;
        public string SelectedCategory
        {
            get => selected;
            set
            {
                if (selected != value && value != " ")
                {
                    List<Goods> categ_list = new List<Goods>();
                    Goods.Clear();
                    if (!Asending)
                    {
                        categ_list = AllGoods.OrderBy(_ => _.Price).Where(x => x.Categories != null || x.Categories.Count > 0).Where(y => y.Categories.Contains(value)).ToList();
                    }
                    else
                    {
                        categ_list = AllGoods.OrderByDescending(_ => _.Price).Where(x => x.Categories != null || x.Categories.Count > 0).Where(y => y.Categories.Contains(value)).ToList();
                        
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var good in categ_list)
                        {
                            if (good.Categories.Contains(value))
                            {
                                good.IsLast = false;
                                Goods.Add(good);
                            }
                        }
                        Goods.Last().IsLast = true;
                    });
                    
                }
                selected = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }
        public Command Increase { get; set; }
        public Command Decrease { get; set; }
        public Command Sort { get; set; }
        bool asending;
        public bool Asending
        {
            get => asending;
            set
            {
                asending = value;
                OnPropertyChanged(nameof(Asending));
            }
        }

        decimal? totalPrice;
        public decimal? TotalPrice
        {
            get => totalPrice;
            set
            {
                totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        decimal? totalWeight;
        public decimal? TotalWeight
        {
            get => totalWeight;
            set
            {
                totalWeight = value;
                OnPropertyChanged(nameof(TotalWeight));
            }
        }

        readonly INavigation Navigation;
        public Command GoToBasket { get; set; }
        public Command GoToPay { get; set; }
        public AdditionalService Service { get; set; }
        bool showWeight;
        public bool ShowWeight
        {
            get => showWeight;
            set
            {
                showWeight = value;
                OnPropertyChanged(nameof(ShowWeight));
            }
        }

        public string ColumnAdditionWidth {get;set;}
        public ObservableCollection<Goods> ChosenGoods { get; set; }
        public ShopViewModel(AdditionalService service, INavigation navigation)
        {
            
            this.Navigation = navigation;
            if (Device.RuntimePlatform == Device.iOS)
            {
                if (DeviceDisplay.MainDisplayInfo.Width > 700)
                    ColumnAdditionWidth = "0.5*";
                else
                    ColumnAdditionWidth = "0.4*";
            }
            else
                ColumnAdditionWidth = "0.4*";

            Service = service;
            ShopName = service.ShopName;
            ShowWeight = this.Service.ShopType == "goods";
            Goods = new ObservableCollection<Goods>();
            Categories = new ObservableCollection<string>();
            var categories = new List<string>();
            TotalPrice = 0;
            TotalWeight = 0;

            LoadGoods = new Command(() =>
            {
                try
                {
                    Task.Run(async () =>
                    {
                        var goods = await Server.GetShopGoods(service.ShopID);
                        if (goods.Error == null)
                        {
                            AllGoods = new List<Goods>();
                            AllGoods.AddRange(goods.Data.Where(x => x.Categories != null || x.Categories.Count > 0));
                            foreach (var good in goods.Data.Where(x => x.Categories != null || x.Categories.Count > 0))
                            {
                                foreach (var category in good.Categories)
                                {
                                    if (!categories.Contains(category.Trim()))
                                    {
                                        categories.Add(category);
                                    }
                                }
                            }
                            Analytics.TrackEvent("начало добавления списка товаров категории 0");
                            var first_category = AllGoods.Where(x => x.Categories.Contains(categories[0])).ToList();
                            var last = first_category.LastOrDefault();
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                foreach (var good in first_category)
                                {
                                    good.IsLast = false;
                                    Goods.Add(good);
                                }
                                Goods.Last().IsLast = true;
                            });
                            
                        }
                    }).ContinueWith((result) =>
                    {
                        Categories.Clear();
                        foreach (var category in categories)
                        {
                            Device.BeginInvokeOnMainThread(() => Categories.Add(category));
                        }
                        //Пока костыль, чтобы места было больше для скролла, т.к. пока категорий мало
                        for (int i = 0; i < 20; i++)
                        {
                            Device.BeginInvokeOnMainThread(() => Categories.Add(" "));
                        }
                        Analytics.TrackEvent("назначение нулевого элемента категории выбранным");
                        Device.BeginInvokeOnMainThread(() => SelectedCategory = Categories[0]);
                    });
                }
                catch(Exception exc)
                {
                    Crashes.TrackError( exc);
                }
                
            });
            Increase = new Command<Goods>(item =>
            {
                item.ColBusket++;
                TotalPrice = AllGoods.Select(x => x.Price * x.ColBusket).Sum();
                TotalWeight = AllGoods.Sum(x => x.Weight * x.ColBusket);
            });

            Decrease = new Command<Goods>(item =>
            {
                if (item.ColBusket == 0)
                    return;
                if (item.ColBusket > 0)
                {
                    item.ColBusket--;
                }
                TotalPrice = AllGoods.Select(x => x.Price * x.ColBusket).Sum();
                TotalWeight = AllGoods.Sum(x => x.Weight * x.ColBusket);
                if (item.ColBusket == 0)
                {
                    ChosenGoods = new ObservableCollection<Goods>();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var item in AllGoods.Where(x => x.ColBusket > 0).ToList())
                        {
                            item.IsLast = false;
                            ChosenGoods.Add(item);
                        }
                        if (ChosenGoods.Count > 0)
                        ChosenGoods.Last().IsLast = true;
                    });
                }
            });

            Sort = new Command(() =>
            {
                Goods.Clear();
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Asending)
                    {
                        foreach (var good in AllGoods.OrderBy(_ => _.Price).ToList())
                        {
                            if (good.Categories.Contains(SelectedCategory))
                            {
                                good.IsLast = false;
                               Goods.Add(good);
                            }
                        }
                    }
                    else
                    {
                        foreach (var good in AllGoods.OrderByDescending(_ => _.Price).ToList())
                        {
                            if (good.Categories.Contains(SelectedCategory))
                            {
                                good.IsLast = false;
                                Goods.Add(good);
                            }
                        }
                    }
                    Asending = !Asending;
                    Goods.Last().IsLast = true;
                });
            });
                

            GoToBasket = new Command(async () =>
            {
                ChosenGoods = new ObservableCollection<Goods>();
                var chosen = AllGoods.Where(x => x.ColBusket > 0).ToList();
                foreach (var item in chosen)
                {
                    ChosenGoods.Add(item);
                }
                foreach (var good in ChosenGoods)
                {
                    good.IsLast = false;
                }
                if(ChosenGoods!=null && ChosenGoods.Count >0)
                    ChosenGoods.Last().IsLast = true;
                if (this.TotalPrice > 0)
                    await Navigation.PushAsync(new BasketPageNew(this));
                else
                    this.ShowError(AppResources.ErrorBasketEmpty);
            });

            GoToPay = new Command(async () =>
            {
                var goodset = new Dictionary<string, Goods>();
                foreach (var item in AllGoods)
                {
                    if (item.ColBusket > 0)
                    {
                        goodset.Add(item.ID.ToString(), item);
                    }
                }

                if (goodset.Count > 0)
                {
                    await Navigation.PushAsync(new PayShopPage(goodset, Service));
                }
                else
                {
                    this.ShowError(AppResources.ErrorBasketEmpty);
                }
            });
        }
    }
}
