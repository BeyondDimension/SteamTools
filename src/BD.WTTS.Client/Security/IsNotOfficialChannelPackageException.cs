namespace BD.WTTS.Security;

public sealed class IsNotOfficialChannelPackageException : Exception
{
    public IsNotOfficialChannelPackageException()
    {
    }

    public IsNotOfficialChannelPackageException(string message) : base(message)
    {
    }

    public IsNotOfficialChannelPackageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public IsNotOfficialChannelPackageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// <see cref="AppSettings.IsOfficialChannelPackage"/>
    /// </summary>
    /// <param name="showMessageBox"></param>
    /// <returns></returns>
    public static bool Check(bool showMessageBox = true)
    {
        return default;
    }
}