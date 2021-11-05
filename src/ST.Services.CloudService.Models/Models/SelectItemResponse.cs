using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public class SelectItemResponse
    {
        public Guid Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
