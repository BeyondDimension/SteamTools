namespace System.Application
{
    /// <summary>
    /// 通知入口点
    /// </summary>
    public enum Entrance : byte
    {
        /// <summary>
        /// 点击该通知将打开主页
        /// </summary>
        Main = 1,

        /// <summary>
        /// 点击该通知将在浏览器中打开
        /// </summary>
        Browser,
    }
}