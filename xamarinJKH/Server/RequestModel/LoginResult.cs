using System;
using System.Collections.Generic;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;

namespace xamarinJKH.Server.RequestModel
{
    public class LoginResult
    {
        public string Login { get; set; } = "";
        public bool IsDispatcher { get; set; }
        public bool accessOSS { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; } = "";
        public string companyPhone { get; set; }
        public string Birthday { get; set; }
        public string FIO { get; set; }
        // токен доступа. 
        //Необходимо передавать как заголовок в методы, требующие авторизации
        public string acx { get; set; } 

        public List<AccountInfo> Accounts { get; set; }
        public string Error { get; set; }
        
        public string Code { get; set; }
        public override string ToString()
        {
            return FIO + " " + Login;
        }
        public UserSettings UserSettings { get; set; }
    }

    public class UserSettings
    {
        public bool RightCloseRequest { get; set; }
        public bool RightPerformRequest { get; set; }
        public bool RightCreateAnnouncements { get; set; }
        
        public bool disableGeolocation { get; set; }
        public bool AlwaysPostHiddenMessage { get; set; }
    }

    public class AccountInfo:BaseViewModel
    {
        public AccountInfo(string ident, int metersStartDay, int metersEndDay, int id, string fio, string address, string company, bool metersAccessFlag)
        {
            Ident = ident.Trim();
            MetersStartDay = metersStartDay;
            MetersEndDay = metersEndDay;
            ID = id;
            FIO = fio;
            Address = address;
            Company = company;
            MetersAccessFlag = metersAccessFlag;
        }

        public AccountInfo()
        {
        }

        public int ID { get; set; }
        public string Ident { get; set; } = "";
        public string FIO { get; set; } = "";
        public string Address { get; set; } = "";
        public string AdressHalf
        {
            get => Settings.GetHalfAddress(Address);
        }
        public string Company { get; set; }
        
        public string DenyRequestCreationMessage { get; set; }
        public int MetersStartDay { get; set; }
        public int MetersEndDay { get; set; }
        public bool MetersAccessFlag { get; set; }
        public bool MetersPeriodStartIsCurrent { get; set; }
        public bool MetersPeriodEndIsCurrent { get; set; }

        public bool AllowPassRequestCreation { get; set; }
        
        public bool DenyRequestCreation { get; set; }

        bool isfirst;
        public bool IsFirst
        {
            get => isfirst;
            set
            {
                isfirst = value;
                OnPropertyChanged("IsFirst");
            }
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
        public override string ToString()
        {
            return Ident;
        }
        
    }
    
   
    
}