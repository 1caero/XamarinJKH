using System;
using System.Collections.Generic;
using System.Text;

namespace xamarinJKH.Server.RequestModel.Monitor
{
    public class RequestStatsQuerySettings
    {
        public int HouseId { get; set; }
        public int DistrictId { get; set; }
        public string CustomPeriodStart { get; set; }
        public string CustomPeriodEnd { get; set; }
    }
}
