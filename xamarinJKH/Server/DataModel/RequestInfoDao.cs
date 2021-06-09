using Realms;

namespace xamarinJKH.Server.DataModel
{
    public class RequestInfoDao : RealmObject
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int TypeID { get; set; }
        public string RequestTerm { get; set; }
        public string RequestNumber { get; set; }
        public string Added { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public bool IsClosed { get; set; }
        public bool IsPerformed { get; set; }
        public bool IsPaid { get; set; }
        public string Address { get; set; }
        // Флаг "Прочитана сотрудником"
        public bool IsReaded { get; set; }
        
        public bool IsReadedByClient { get; set; }
        // источник заявки
        public string SourceType { get; set; }
        // тип неисправности
        public string MalfunctionType { get; set; }
        // исполнитель
        public string PerofmerName { get; set; }
        // название приоритета
        public string PriorityName { get; set; }
        // ид приоритета
        public int PriorityId { get; set; }
        // долг по лсч
        public decimal Debt { get; set; }
        // источник
        public string Source { get; set; }
    
        // к заявке привязан пропуск
        public bool HasPass { get; set; }
        // привязанный пропуск постоянный
        public bool PassIsConstant { get; set; }
        // дата окончания
        public string PassExpiration { get; set; }
        public string PaidRequestStatus { get; set; }
        public bool IsPaidByUser { get; set; }
        public int ShopId { get; set; }
        
        public string PaidRequestCompleteCode { get; set; }//  - код подтверждения(подтягивается только для жителя)
    }
}