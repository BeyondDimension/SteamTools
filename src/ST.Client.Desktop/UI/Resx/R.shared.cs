using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.UI.Resx
{
    public sealed class R : Abstractions.R<R>
    {
        static R()
        {
            Current = new();
        }

        private R()
        {

        }
    }
}
