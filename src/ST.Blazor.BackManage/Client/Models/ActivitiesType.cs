using System;

namespace System.Application.Models
{
    public class ActivitiesType
    {
        public string Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ActivityUser User { get; set; }
        public ActivityGroup Group { get; set; }
        public ActivityProject Project { get; set; }
        public string Template { get; set; }
    }
}