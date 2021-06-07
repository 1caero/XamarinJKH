using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class MobileSettings
    {
        public string showAds { get; set; }
        public int adsType { get; set; }
        // Id заявки на пропуск
        public int requestTypeForPassRequest { get; set; }
        public string adsCodeIOS { get; set; }
        public string adsCodeAndroid { get; set; }
        public string bonusOfertaFile { get; set; }
        
        public string startScreen { get; set; }
        public bool enableOSS { get; set; }    
        public bool blockUserAuth { get; set; }    
        
        // Возможноть выбора подъезда
        public bool isRequiredEntrance { get; set; } 
        
        // Возможность выбора квартиры
        public bool isRequiredFloor { get; set; }    
        // Возможность создавать заявку на проауск
        public bool enableCreationPassRequests { get; set; }    
        
        public bool useAccountPinCode { get; set; }    
        // Отключение массового выполнени закрытия
        public bool disableBulkRequestsClosing { get; set; }    
        
        public bool chooseIdentByHouse { get; set; }    
        public bool disableCommentingRequests { get; set; }    
        public bool useBonusSystem { get; set; }    
        public bool useDispatcherAuth { get; set; }

        public string main_name { get; set; }

        public string color { get; set; }

        //public string main_name { get { return "ООО Авалон Эко"; } set { } }
        //public string color { get { return "92ab1b"; } set { } }
        //public string main_name { get { return "Центр инвестиций 50"; } set { } }
        //public string color { get { return "3e4b82"; } set { } }
        //public string main_name { get { return "Универсальные решения"; } set { } }
        //public string color { get { return "0c54a0"; } set { } }

        //public string main_name { get { return "Чистый город г. Одинцово"; } set { } }
        //public string color { get { return "359031"; } set { } }






        public double servicePercent { get; set; }
        public bool DontShowDebt { get; set; }
        public bool registerWithoutSMS { get; set; }
        public bool сheckCrashSystem { get; set; }
        public bool disablePermanentPasses { get; set; } // галку, отключающую в МП создание "Постоянных" пропусков. 
        public bool hidePassRequestLifetime { get; set; } // галку, которая бы убирала в форме создания заявки на пропуск поле "срок действия пропуска"

        public List<MobileMenu> menu { get; set; }
        public string Error { get; set; }
        public string appLinkIOS { get; set; }
        public string appLinkAndroid { get; set; }

        public string appTheme { get; set; }
        
        public int MockupCount { get; set; }
        public bool requireBirthDate { get; set; }
        public bool districtsExists { get; set; }
        public bool housesExists { get; set; }
        public bool streetsExists { get; set; }
        public bool premisesExists { get; set; }

        public bool showOurService { get; set; }
    }

    public class MobileMenu
    {
        public int id { get; set; }
        public string name_app { get; set; }
        public string simple_name { get; set; }
        public int visible { get; set; }
    }
}