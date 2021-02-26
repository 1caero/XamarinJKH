using System;
using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class EventBlockData
    {
        // Опросы
        public List<PollInfo> Polls { get; set; }
        // Объявления
        public List<AnnouncementInfo> Announcements { get; set; }
        // Новости
        public List<NewsInfo> News { get; set; }
        // Доп. услуги
        public List<AdditionalService> AdditionalServices { get; set; }
        
        // Доп. услуги c группировкой и сортировкой
        public Dictionary<string, List<AdditionalService>> AdditionalServicesByGroups { get; set; }
        public string Error { get; set; }
    }

    public class PollInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        
        public bool IsComplite { get; set; }

        public bool IsReaded { get; set; }
        public List<QuestionInfo> Questions { get; set; }
    }

    public class QuestionInfo
    {
        public int ID { get; set; }
        public string Text { get; set; }
        
        public bool IsCompleteByUser { get; set; }
        public List<AnswerInfo> Answers { get; set; }
    }

    public class AnswerInfo
    {
        public string Text { get; set; }
        public int ID { get; set; }
        public bool IsUserAnswer { get; set; }
    }

    public class AnnouncementInfo
    {
        public int ID { get; set; }
        public string Created { get; set; }
        public string Text { get; set; }
        public int QuestionGroupID { get; set; }
        public int AdditionalServiceId { get; set; }
        public bool IsReaded { get; set; }
        public string Header { get; set; }
        // файлы объявлений
        public List<FileInfo> Files { get; set; }
    }

    public class NewsInfo
    {
        public int ID { get; set; }
        public string Header { get; set; }
        public string ShortContent { get; set; }
        public string Created { get; set; }
        public bool HasImage { get; set; }
        public bool IsReaded { get; set; }
    }
    public class NewsInfoFull
    {
        public int ID { get; set; }
        public string Created { get; set; }
        public string Header { get; set; }
        public string Text { get; set; }
        public bool HasImage { get; set; }
        public string Error { get; set; }
    }

    public class AdditionalService
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public bool HasLogo { get; set; }
        public string Phone { get; set; }
        public string Group { get; set; }
        public int? id_RequestType { get; set; }
        public int? id_Account { get; set; }
        public bool CanBeOrdered { get; set; }
        public string ShowInAdBlock { get; set; }

        public string NotNullShowInAdBlock => string.IsNullOrWhiteSpace(ShowInAdBlock) ? "" : ShowInAdBlock;
        public string ShopName { get; set; }
        public int? ShopID { get; set; }
        string lat;
        // координаты магазина на карте - широта
        public string  ShopLat
        {
            get
            {
                if (lat == null) return "0";
                return lat;
            }
            set
            {
                lat = value;
            }
        }
        string lng;
        // координаты магазина на карте - долгота
        public string ShopLng
        {
            get
            {
                if (lng == null) return "0";
                return lng;
            }
            set
            {
                lng = value;
            }
        }
        // ссылка на логотип
        public string LogoLink { get; set; }
        // id файла
        public string LogoFileId { get; set; }
        // макс процент списания бонусов для лицевых счетов текущего пользователя
        public List<BonusDiscountRate> BonusDiscountRates { get; set; }
        public Xamarin.Forms.Maps.Position Position { get => new Xamarin.Forms.Maps.Position(double.Parse(this.ShopLat.Replace(',','.')), double.Parse(this.ShopLng.Replace(',', '.'))); set => this.Position = value; }
        public string ShopType { get; set; }
    }

    public class BonusDiscountRate
    {
        // id л/сч
        public string AccountId { get; set; }
        // номер л/сч
        public string Ident { get; set; }
        // макс процент списания бонусов
        public decimal Rate { get; set; }
    }

}