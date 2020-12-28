using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class RequestStats
    {
        public PeriodStats Today { get; set; }
        public PeriodStats Week { get; set; }
        public PeriodStats Month { get; set; }
        public List<Requests> TotalUnperformedRequestsList { get; set; }
        public string Error { get; set; }
        // статистика за произвольный период
        public PeriodStats CustomPeriod{ get; set; }
    }
}