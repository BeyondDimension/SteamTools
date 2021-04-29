namespace System.Application.Models
{
    public class ListFormModel
    {
        public string Owner { get; set; } = "wzj";

        public string ActiveUser { get; set; }

        public string Satisfaction { get; set; }
    }

    public class BasicListFormModel
    {
        public string Status { get; set; } = "all";
        public string SearchKeyword { get; set; }
    }
}