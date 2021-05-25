using System;
using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class R731PremiseWithAccounts
    {
        public int ID { get; set; }
        public string Entrance { get; set; }
        public int? EntranceID { get; set; }
        public bool IsNonresidential { get; set; }
        public string Number { get; set; }
        public string UniqueNum { get; set; }

        public string Type => IsNonresidential ? "нежилое" : "жилое";
        public string DisplayText => $"{Number} ({Type})";

        public List<Account> Accounts { get; set; }
    }
}