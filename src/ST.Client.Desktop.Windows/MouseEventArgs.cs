using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application
{
    public class MouseEventArgs : EventArgs
    {

        public MouseEventArgs(PointInt32 point)
        {
            MousePosition = point;
        }

        public MouseEventArgs(int x, int y)
        {
            MousePosition = new PointInt32(x, y);
        }

        /// <summary>
        /// 指针位置
        /// </summary>
        public PointInt32 MousePosition { get; set; }
    }
}
