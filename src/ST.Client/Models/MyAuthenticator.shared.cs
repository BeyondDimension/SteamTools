using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public partial class MyAuthenticator : Abstractions.MyAuthenticator
    {
        public MyAuthenticator() : base()
        {

        }

        public MyAuthenticator(IGAPAuthenticatorDTO data) : base(data)
        {

        }

        public static List<MyAuthenticator> Get(IEnumerable<IGAPAuthenticatorDTO> items)
            => items.Select(x => new MyAuthenticator(x)).ToList();
    }
}
