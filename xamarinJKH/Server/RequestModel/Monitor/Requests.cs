using System;
using xamarinJKH.Utils;

namespace xamarinJKH.Server.RequestModel
{
    public class Requests
    {
        public DateTime? DateStartReport { get; set; }
        public DateTime? Added { get; set; }
        public int Number { get; set; }
        public bool IsComplete { get; set; }
        public bool IsActive { get; set; }
        public int? Mark { get; set; }
        public string Status { get; set; }
        public int id_Status { get; set; }
        
        public string Address { get; set; }
        public string Name { get; set; }
        public string Performer { get; set; }
        public string RequestNumber { get; set; }
        public string Resource => "resource://xamarinJKH.Resources." + Settings.GetStatusIcon(id_Status) + ".svg";
        
    }
}