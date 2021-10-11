using ReactiveUI;
using System;
using System.Application.Properties;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <inheritdoc cref="Abstractions.ToastService"/>
    public partial class ToastService : Abstractions.ToastService<ToastService>
    {
        public ToastService()
        {

        }
    }
}
