using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public class HandleWindow
    {
        public IntPtr Handle { get; set; }
        public string? Title { get; set; }
        public string? ClassName { get; set; }
        public Process? Process { get; set; }
        public string? Path => Process?.MainModule?.FileName;
        public string? Name => Process?.ProcessName;
    }
}
