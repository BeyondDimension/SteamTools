using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public class SteamAppLaunchItem : ReactiveObject
    {

        public string? Label { get; set; }

        public string? Executable { get; set; }

        public string? Arguments { get; set; }

        public string? WorkingDir { get; set; }

        public string? Platform { get; set; }
    }
}
