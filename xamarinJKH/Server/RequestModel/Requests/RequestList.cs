using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;
using xamarinJKH.Server.DataModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Server.RequestModel
{
    public class RequestList
    {
        public List<RequestInfo> Requests { get; set; }
        public string UpdateKey { get; set; }
        public string Error { get; set; }
    } 
    public class RequestListDao
    {
        public List<RequestInfoDao> Requests { get; set; }
        public string UpdateKey { get; set; }
        public string Error { get; set; }
    }
    
    public class RequestInfo
    {
        public static RequestInfoDao InfoToDao(RequestInfo requestInfo)
        {
            return new RequestInfoDao
            {
                ID = requestInfo.ID,
                TypeID = requestInfo.TypeID,
                RequestTerm = requestInfo.RequestTerm,
                IsReadedByClient = requestInfo.IsReadedByClient,
                RequestNumber=requestInfo.RequestNumber,
                Added=requestInfo.Added,
                Name=requestInfo.Name,
                Status=requestInfo.Status,
                StatusID=requestInfo.StatusID,
                IsClosed=requestInfo.IsClosed,
                IsPerformed=requestInfo.IsPerformed,
                Address=requestInfo.Address,
                IsReaded=requestInfo.IsReaded,
                SourceType=requestInfo.SourceType,
                MalfunctionType=requestInfo.MalfunctionType,
                PerofmerName=requestInfo.PerofmerName,
                PriorityName=requestInfo.PriorityName,
                PriorityId=requestInfo.PriorityId,
                Debt=requestInfo.Debt,
                Source=requestInfo.Source,
                HasPass=requestInfo.HasPass,
                PassIsConstant=requestInfo.PassIsConstant,
                PassExpiration=requestInfo.PassExpiration,
            };
        }
        public static RequestInfo DaoToInfo(RequestInfoDao requesDao)
        {
            return new RequestInfo
            {
                ID = requesDao.ID,
                TypeID = requesDao.TypeID,
                RequestTerm = requesDao.RequestTerm,
                IsReadedByClient = requesDao.IsReadedByClient,
                RequestNumber=requesDao.RequestNumber,
                Added=requesDao.Added,
                Name=requesDao.Name,
                Status=requesDao.Status,
                StatusID=requesDao.StatusID,
                IsClosed=requesDao.IsClosed,
                IsPerformed=requesDao.IsPerformed,
                Address=requesDao.Address,
                IsReaded=requesDao.IsReaded,
                SourceType=requesDao.SourceType,
                MalfunctionType=requesDao.MalfunctionType,
                PerofmerName=requesDao.PerofmerName,
                PriorityName=requesDao.PriorityName,
                PriorityId=requesDao.PriorityId,
                Debt=requesDao.Debt,
                Source=requesDao.Source,
                HasPass=requesDao.HasPass,
                PassIsConstant=requesDao.PassIsConstant,
                PassExpiration=requesDao.PassExpiration,
            };
        }
        public int ID { get; set; }
        public int TypeID { get; set; }
        private int index = 0;
        public string RequestNumber { get; set; }
        public string Added { get; set; }
        
        public string RequestTerm { get; set; }

        public DateTime _RequestTerm
        {
            get => string.IsNullOrWhiteSpace(RequestTerm) ? DateTime.Now.AddDays(10).AddHours(index++) : DateTime.ParseExact(RequestTerm, "dd.MM.yyyy HH:mm:ss",
                CultureInfo.CurrentCulture);
        }
// долг по лсч
        public decimal Debt { get; set; }
        public string _Added
        {
            get => Added.Trim();
            set => Added = value;
        }

        private bool isEnableMass = !Settings.MobileSettings.disableBulkRequestsClosing;
        

        public bool IsEnableMass
        {
            get => isEnableMass;
            set => isEnableMass = value;
        }
        
        public string Name { get; set; }
        
        public string NotNullName
        {
            get => string.IsNullOrWhiteSpace(Name) ? "" : Name;
            set => Name = value;
        }
        public string Text  { get; set; }
        public string HalfName
        {
            get => Name.Length > 50 ? Settings.GetHalfAddress(Name) : Name;
            set => Name = value;
        }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public bool IsClosed { get; set; }
        
        public bool HasPass  { get; set; }
        public bool PassIsConstant   { get; set; }

        public string TextPassIsConstant
        {
            get => PassIsConstant ? AppResources.ConstantPass : AppResources.OneOffPass;
        }
        public bool IsCheked { get; set; } = false;
        
        public string Address { get; set; }
        public string Source { get; set; }

        private bool _isVisibleAddress;

        public bool IsVisibleAddress
        {
            get => !string.IsNullOrWhiteSpace(Address);
            set => _isVisibleAddress = value;
        }

        public bool IsPerformed { get; set; }
        public string PaidRequestStatus { get; set; } //- статус заказа
        public string PaidRequestCompleteCode { get; set; }//  - код подтверждения(подтягивается только для жителя)
        public bool IsPaidByUser { get; set; }
        public bool IsPaid { get; set; }
        public int ShopId { get; set; }
        // Флаг "Прочитана сотрудником"
        public bool IsReaded { get; set; }
        public bool IsReadedByClient { get; set; }
        // название приоритета
        public string PriorityName { get; set; }

        private Color _textColor;

        public Color TextColor
        {
            get => Settings.GetPriorityColor(PriorityId);
            set => _textColor = value;
        }

        // ид приоритета
        public int PriorityId { get; set; }
        public bool _IsReadedByClient
        {
            get=>!IsReadedByClient && StatusID != 6;
            set=> s = value;
        }
        public string PassExpiration { get; set; }
        private bool s;
        
        private string _resource { get; set; }

        public string Resource => "resource://xamarinJKH.Resources." + Settings.GetStatusIcon(StatusID) + ".svg";

        // источник заявки
        public string SourceType { get; set; }
        // тип неисправности
        public string MalfunctionType { get; set; }

        public string _MalfunctionType
        {
            get => string.IsNullOrWhiteSpace(MalfunctionType)? "" :  " (" + MalfunctionType + ")";
            set => MalfunctionType = value;
        }
        // исполнитель
        public string PerofmerName { get; set; }

        //public override string ToString()
        //{
        //    return $"Id={ID} isClosed={IsClosed} RequestNumber={RequestNumber}";
        //}
    }

    public class RequestContent : RequestInfo
    {
        public string Phone { get; set; }
        public string Address { get; set; }

        public List<RequestCall> Calls { get; set; }
        public string HalfAdress
        {
            get => !string.IsNullOrWhiteSpace(Address) && Address.Length > 50 ? Settings.GetHalfAddress(Address) : Address;
            set => Address = value;
        }
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public string AuthorName { get; set; }
        public List<RequestMessage> Messages { get; set; }
        public List<RequestFile> Files { get; set; }
        public List<RequestsReceiptItem> ReceiptItems { get; set; }
        public decimal PaidSumm { get; set; }
        public string PaidServiceText { get; set; }
        public string Error { get; set; }
        // информация о пропуске
        public RequestPass PassInfo { get; set; }

        public string AcceptedDispatcher { get; set; }

        public RequestContent Copy()
        {
            return (RequestContent)this.MemberwiseClone();
        }
    }

    public class RequestMessage
    {
        private string dateAdd;
        private string timeAdd;
        private bool dateVisible1;

        public int ID { get; set; }
        public string Added { get; set; }
        public string AuthorName { get; set; }
        public string Text { get; set; }
        public bool IsHidden  { get; set; }
        public int FileID { get; set; }
        // сообщение создано текущим пользователем
        public bool IsSelf { get; set; }
        public string DateAdd { get => Added.Split(' ')[0]; set => dateAdd = value; }
        public string TimeAdd { get => Added.Split(' ')[1].Substring(0, 5); set => timeAdd = value; }
        public bool dateVisible
        {
            get
            {
                if (Settings.DateUniq.Equals(DateAdd))
                    return false;
                else
                {
                    Settings.DateUniq = DateAdd;
                    return true;
                }

            }
            set => dateVisible1 = value;
        }
    }

    public class MessageCall : RequestMessage
    {
        public double Duration { get; set; }
        public string Direction { get; set; }
        public string Phone { get; set; }
        public string Link => $"{RestClientMP.SERVER_ADDR}/SupportService/DownloadCall/{ID}?acx={Uri.EscapeDataString(Settings.Person.acx ?? string.Empty)}";
        public static MessageCall CallToMessage(RequestCall call)
        {
            return new MessageCall
            {
                ID = call.ID,
                Added = call.Added,
                AuthorName = call.AuthorName,
                IsSelf = call.IsSelf,
                Duration = call.Duration,
                Direction = call.Direction,
                Phone = call.Phone,
                FileID = -1,
                Text = call.Direction == "IN" ? "Входящий звонок с номера " + call.Phone : "Исходящий звонок на номер " + call.Phone
            };
        }
        
        
    }
    
    public class RequestFile
    {
        public int ID { get; set; }
        public string Added { get; set; }
        public string AuthorName { get; set; }
        public string Name { get; set; }
        // файл загружен текущим пользователем
        public bool IsSelf { get; set; }
        public int FileSize { get; set; }
    }

}