namespace xamarinJKH.Server.RequestModel
{
    public class CustomSearchCriteria
    {
        // числовое значение критерия
        public long? ItemID { get; set; }
        // текстовое значение критерия
        public string Text { get; set; }
        // критерий
        public string SearchCriteriaType { get; set; }
    }
}