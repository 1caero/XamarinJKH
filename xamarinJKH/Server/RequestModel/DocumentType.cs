using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class DocumentType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<long> Approvers { get; set; }
    }
}