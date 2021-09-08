using System.Diagnostics;

namespace System.Application.Models
{
    public class HandleWindow
    {
        public IntPtr Handle { get; set; }
        public string? Title { get; set; }
        public string? ClassName { get; set; }
        public Process? Process { get; set; }
        public string? Path { get; set; }
        public string? Name { get; set; }
    }
}