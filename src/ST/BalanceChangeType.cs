using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application
{
    public enum BalanceChangeType : short
    {
        /// <summary>
        /// 充值
        /// </summary>
        Recharge = 1,
        /// <summary>
        /// 购买商品
        /// </summary>
        Shop = 2,
        /// <summary>
        /// 爱发电赞助
        /// </summary>
        Afdian = 3,

    }
}
