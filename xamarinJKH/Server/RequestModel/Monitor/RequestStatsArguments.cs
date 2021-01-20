using System;
using System.Collections.Generic;
using System.Text;

namespace xamarinJKH.Server.RequestModel.Monitor
{
    public class RequestStatsArguments
    {
        public List<RequestStatsQuerySettings> Queries { get; set; }
    }
}
