using System;
using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class AddDocumentArguments
    {
        public int? AccountId { get; set; }
        public int? HouseId { get; set; }
        public string Fio { get; set; }
        public string Phone { get; set; }
        public int Type { get; set; }
        public long Priority { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Floor { get; set; }
        public string Intercom { get; set; }
        public DateTime? DateStartReport { get; set; }

        public long? id_SourceType { get; set; }

        public List<long> Approvers { get; set; }
    }
}