namespace xamarinJKH.Server.RequestModel
{
    public class RequestCall
    {
        public int ID { get; set; }
        public string Added { get; set; }
        public string AuthorName { get; set; }
        public bool IsSelf { get; set; }
        public int Duration { get; set; }
        public string Direction { get; set; }
        public string Phone { get; set; }
    }
}