using System.Collections.Generic;
using System.ComponentModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Server.RequestModel
{
    public class RequestList
    {
        public List<RequestInfo> Requests { get; set; }
        public string UpdateKey { get; set; }
        public string Error { get; set; }
    }
    
    public class RequestInfo
    {
        public int ID { get; set; }
        public string RequestNumber { get; set; }
        public string Added { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCheked { get; set; } = false;
        
        public string Address { get; set; }
        public bool IsPerformed { get; set; }
        public string PaidRequestStatus { get; set; } //- статус заказа
        public string PaidRequestCompleteCode { get; set; }//  - код подтверждения(подтягивается только для жителя)
        public bool IsPaidByUser { get; set; }
        public bool IsPaid { get; set; }
        public int ShopId { get; set; }
        // Флаг "Прочитана сотрудником"
        public bool IsReaded { get; set; }
        public bool IsReadedByClient { get; set; }

        //public override string ToString()
        //{
        //    return $"Id={ID} isClosed={IsClosed} RequestNumber={RequestNumber}";
        //}
    }

    public class RequestContent : RequestInfo
    {
        public string Phone { get; set; }
        public string Address { get; set; }
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