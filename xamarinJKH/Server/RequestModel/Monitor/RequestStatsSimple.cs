using System;
using System.Collections.Generic;
using System.Text;

namespace xamarinJKH.Server.RequestModel.Monitor
{
    public class RequestStatsSimple
    {
        public PeriodStatsSimple Today { get; set; }
        public PeriodStatsSimple Week { get; set; }
        public PeriodStatsSimple Month { get; set; }
        public PeriodStatsSimple CustomPeriod { get; set; }
    }
}
