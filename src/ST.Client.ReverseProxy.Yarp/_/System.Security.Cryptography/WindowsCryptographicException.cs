using System.Globalization;
using System.Text;

namespace System.Security.Cryptography;

public sealed class WindowsCryptographicException : CryptographicException
{
    const int E_FAIL = unchecked((int)0x80004005);

    public WindowsCryptographicException(CryptographicException inner) : base("", inner)
    {
        HResult = inner.HResult;
    }

    public override string ToString()
    {
        if (HResult == 0)
        {
            return base.ToString();
        }

        string message = Message;
        string className = GetType().ToString();
        StringBuilder s = new StringBuilder(className);
        string nativeErrorString = HResult < 0
            ? $"0x{HResult:X8}"
            : HResult.ToString(CultureInfo.InvariantCulture);
        if (HResult == E_FAIL)
        {
            s.Append($" ({nativeErrorString})");
        }
        else
        {
            s.Append($" ({HResult:X8}, {nativeErrorString})");
        }

        if (!string.IsNullOrEmpty(message))
        {
            s.Append(": ");
            s.Append(message);
        }

        Exception? innerException = InnerException;
        if (innerException != null)
        {
            s.Append(" ---> ");
            s.Append(innerException.ToString());
        }

        string? stackTrace = StackTrace;
        if (stackTrace != null)
        {
            s.AppendLine();
            s.Append(stackTrace);
        }

        return s.ToString();
    }
}
