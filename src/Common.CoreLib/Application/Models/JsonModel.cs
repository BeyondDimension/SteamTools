namespace System.Application.Models;

public abstract class JsonModel
{
    public override string ToString()
    {
        return Serializable.SJSON(this);
    }
}

public abstract class JsonModel<T> : JsonModel where T : JsonModel<T>
{
    public static T? Parse(string value)
    {
        return Serializable.DJSON<T>(value);
    }

    public static bool TryParse(string value, out T? result)
    {
        try
        {
            result = Parse(value);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}