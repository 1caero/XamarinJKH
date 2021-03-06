﻿using System.Collections.Generic;
using xamarinJKH.ViewModels;

namespace xamarinJKH.Server.RequestModel
{
    public class NamedValue:BaseViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public int Value { get; set; }

        public string _Name
        {
            get => Name.Length > 40 ? Name.Substring(0, 40) + "..." : Name;
            set => Name = value;
        } 
        public string _NamePicker
        {
            get
            {
                return Name.Length > 35 ? Name.Substring(0, 30) + "..." : Name;
            }
            set => Name = value;
        }

        bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
            }
        }
        
        public class AccountingInfoModel
        {
            public List<NamedValue> AllAcc { get; set; }
            public NamedValue SelectedAcc { get; set; }
        }
    }
    
    public class RequestType : NamedValue
    {

        public List<RequestType> SubTypes { get; set; }
        public bool HasSubTypes { get; set; }

    }
}