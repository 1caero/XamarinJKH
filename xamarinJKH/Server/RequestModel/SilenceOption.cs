namespace xamarinJKH.Server.RequestModel
{
    public class SilenceOption
    {
        public int ID { get; set; }
        // Дата начала - если задана, то условие будет выполняться только после этой даты
        public string FromDate { get; set; }
        // Дата окончания - если задана, то условие будет выполняться только до этой даты
        public string ToDate { get; set; }
        // День недели пн ... вс - нужно обязательно выбрать хотябы один. Если все дни - нужно выбрать все.
        public bool OnMonday { get; set; }
        public bool OnTuesday { get; set; }
        public bool OnWednesday { get; set; }
        public bool OnThursday { get; set; }
        public bool OnFriday { get; set; }
        public bool OnSaturday { get; set; }
        public bool OnSunday { get; set; }
        // Время с - если задано, условие выполняется только после указанного времени суток
        public string FromTime { get; set; }
        // Время по - если задано, условие выполняется только до указанного времени суток
        public string ToTime { get; set; }
    }
}