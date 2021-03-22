namespace System.Diagnostics
{
    /// <summary>
    /// 在调试器变量窗口中的显示方式
    /// <para>https://docs.microsoft.com/zh-cn/dotnet/api/system.diagnostics.debuggerdisplayattribute?view=net-5.0</para>
    /// </summary>
    public interface IDebuggerDisplay
    {
        string GetDebuggerDisplayValue(object obj);

        public static string GetValue(object obj)
        {
            try
            {
                var debuggerDisplay = DI.Get<IDebuggerDisplay>();
                return debuggerDisplay.GetDebuggerDisplayValue(obj);
            }
            catch
            {
                return obj.ToString();
            }
        }
    }
}