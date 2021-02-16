using System;
using Xamarin.Forms;
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
        public string RequestTerm { get; set; }
        public string _RequestTerm
        {
            get => string.IsNullOrWhiteSpace(RequestTerm) ? DateTime.Now.AddDays(10).ToString("dd.MM.yyyy hh:mm:ss") : RequestTerm;
        }
        public decimal Debt { get; set; }
        // источник заявки
        public string SourceType { get; set; }
        // название приоритета
        public string PriorityName { get; set; }
        private Color _textColor;
        public Color TextColor
        {
            get => Settings.GetPriorityColor(PriorityId);
            set => _textColor = value;
        }

        // ид приоритета
        public int PriorityId { get; set; }
        public string MalfunctionType { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string Performer { get; set; }
        public string RequestNumber { get; set; }
        public string Resource => "resource://xamarinJKH.Resources." + Settings.GetStatusIcon(id_Status) + ".svg";
        
    }
}