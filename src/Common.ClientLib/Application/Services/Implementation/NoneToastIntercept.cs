namespace System.Application.Services.Implementation
{
    public sealed class NoneToastIntercept : IToastIntercept
    {
        bool IToastIntercept.OnShowExecuting(string text)
        {
            return false;
        }
    }
}