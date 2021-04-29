using System;

namespace System.Application.Models
{
    public class StepFormModel
    {
        public string ReceiverAccountType { get; set; } = "ant-design@alipay.com";
        public string ReceiverAccount { get; set; } = "test@example.com";
        public string ReceiverName { get; set; } = "Alex";
        public string PayAccount { get; set; }
        public string Password { get; set; } = "500";
        public string Amount { get; set; } = "12345678";
    }

    public class AdvancedFormModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Owner { get; set; }
        public string Approver { get; set; }
        public DateTime?[] DateRange { get; set; }
        public string Type { get; set; }
        public string Name2 { get; set; }
        public string Url2 { get; set; }
        public string Owner2 { get; set; }
        public string Approver2 { get; set; }
        public DateTime? DateRange2 { get; set; }
        public string Type2 { get; set; }
    }

    public class BasicFormModel
    {
        public string Title { get; set; }
        public string Client { get; set; }
        public string Invites { get; set; }
        public int Disclosure { get; set; }
        public int Weight { get; set; }
        public string Standard { get; set; }
        public string Goal { get; set; }
        public DateTime?[] DateRange { get; set; }
    }

    public class Owner
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}