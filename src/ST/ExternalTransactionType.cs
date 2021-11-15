using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application
{
    public enum ExternalTransactionType : byte
    {
        Afdian = 1,
        Patreon,
        Ko_fi,
        WeChatPay,
        Alipay,
    }
}
