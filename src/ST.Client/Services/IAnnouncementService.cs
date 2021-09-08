using System.Application.Models;

namespace System.Application.Services
{
    /// <summary>
    /// 公告服务
    /// </summary>
    public interface IAnnouncementService
    {
        /// <summary>
        /// 显示公告，返回是否显示
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        bool Show(NotificationRecordDTO? notification)
        {
            //if (主窗口显示中)
            //{
            //打开提示(公告)窗口
            return true;
            //}
            //else
            //{
            //写入本地数据库待下一次打开主窗口时显示
            return false;
            //}
        }

        // 🔔右上角按钮，放入通知中，点击出现公告弹窗，待定
    }
}