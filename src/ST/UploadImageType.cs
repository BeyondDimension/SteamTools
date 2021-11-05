namespace System.Application
{
    /// <summary>
    /// 上传图片用途
    /// </summary>
    [Flags]
    public enum UploadImageType
    {
        /// <summary>
        /// 用户头像
        /// </summary>
        Avatar = 0x1,

        /// <summary>
        /// 通知封面，内容图片
        /// </summary>
        Notice = 0x4,

        //占位符1 = 0x2,
        //占位符2 = 0x4,
        //占位符3 = 0x8,
        //占位符4 = 0x10,
    }
}