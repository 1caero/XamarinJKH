using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.ViewModels.AppsConst
{
    public class AddressSearchViewModel : BaseViewModel
    {
        public List<NamedValue> Districts { get; set; }
        public List<NamedValue> Streets { get; set; }
        public List<NamedValue> Quarts { get; set; }
        public List<HouseProfile> Houses { get; set; }
        public int? DistrictID;
        public int? HouseID;
        public int? PremiseID;
        public Command LoadDistricts { get; set; }
        public Command LoadHouses { get; set; }

        string district;
        public string District
        {
            get => district;
            set
            {
                district = value;
                OnPropertyChanged(nameof(District));
            }
        }

        string street;
        public string Street
        {
            get => street;
            set
            {
                street = value;
                OnPropertyChanged(nameof(Street));
            }
        }

        string house;
        public string House
        {
            get => house;
            set
            {
                house = value;
                OnPropertyChanged(nameof(House));
            }
        }

        string flat;
        public string Flat
        {
            get => flat;
            set
            {
                flat = value;
                OnPropertyChanged(nameof(Flat));
            }
        }

        public NamedValue DistrictObject;
        public NamedValue HouseObject;
        public NamedValue FlatObject;
        public AddressSearchViewModel(Tuple<NamedValue, NamedValue, NamedValue> selected)
        {
            Districts = new List<NamedValue>();
            Streets = new List<NamedValue>();
            Quarts = new List<NamedValue>();
            Houses = new List<HouseProfile>();

            LoadDistricts = new Command(async () =>
            {
                var response = await Server.GetHouseGroups();
                foreach (var district in response.Data)
                {
                    Districts.Add(district);
                }
            });

            LoadHouses = new Command(async () =>
            {
                var response = await Server.GetHouse(string.IsNullOrEmpty(this.Street) ? null : this.Street);
                Houses.Clear();
                foreach (var house in response.Data)
                {
                    Houses.Add(house);
                }
            });
            DistrictObject = selected.Item1;
            HouseObject = selected.Item2;
            FlatObject = selected.Item3;

            if (selected.Item1 != null)
            {

                District = selected.Item1.Name;
                DistrictID = selected.Item1.ID;
            }
            if (selected.Item2 != null)
            {
                House = selected.Item2.Name;
                HouseID = selected.Item2.ID;
            }
            if (selected.Item3 != null)
            {
                Flat = selected.Item3.Name;
                PremiseID = selected.Item3.ID;
            }


            MessagingCenter.Subscribe<Object, NamedValue>(this, "SetDistrict", (sender, args) =>
            {
                if (DistrictID != args.ID)
                {
                    HouseID = null;
                    House = null;
                    HouseObject = null;
                    PremiseID = null;
                    Flat = null;
                    FlatObject = null;
                }
                DistrictID = args.ID;
                District = args.Name;
                DistrictObject = args;
                
            });
            MessagingCenter.Subscribe<Object, NamedValue>(this, "SetHouse", (sender, args) =>
            {
                if (HouseID != args.ID)
                {
                    PremiseID = null;
                    Flat = null;
                    FlatObject = null;
                }
                HouseID = args.ID;
                House = args.Name;
                HouseObject = args;
            });
            MessagingCenter.Subscribe<Object, NamedValue>(this, "SetPremise", (sender, args) =>
            {
                PremiseID = args.ID;
                Flat = args.Name;
                FlatObject = args;
            });

        }
    }
}
