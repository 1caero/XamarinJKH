using System;
using System.Collections.Generic;
using System.Text;

namespace xamarinJKH.Server.RequestModel.Monitor
{
    public class PeriodStatsSimple
    {
        public int RequestsCount { get; set; }
        public int UnperformedRequestsCount { get; set; }
        public int OverdueRequestsCount { get; set; }
        public int ClosedRequestsWithMark1Count { get; set; }
        public int ClosedRequestsWithMark2Count { get; set; }
        public int ClosedRequestsWithMark3Count { get; set; }
        public int ClosedRequestsWithMark4Count { get; set; }
        public int ClosedRequestsWithMark5Count { get; set; }
    }
}
