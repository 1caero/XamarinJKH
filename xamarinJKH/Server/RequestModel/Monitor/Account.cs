namespace xamarinJKH.Server.RequestModel
{
    public class Account
    {
        public int ID { get; set; }
        public string FIO { get; set; }
        public string Ident { get; set; }
        public string Phone { get; set; }

        public string DisplayText => $"{Ident} ({FIO})";
    }
}