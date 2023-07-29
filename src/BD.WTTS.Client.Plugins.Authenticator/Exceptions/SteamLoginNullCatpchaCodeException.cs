namespace BD.WTTS.Exceptions;

public class SteamLoginNullCatpchaCodeException : Exception
{
    public SteamLoginNullCatpchaCodeException() : base()
    {

    }

    public SteamLoginNullCatpchaCodeException(string message) : base(message)
    {

    }
}

public class SteamLoginRequires2FAException : Exception
{
    public SteamLoginRequires2FAException(string message) : base(message)
    {

    }
}