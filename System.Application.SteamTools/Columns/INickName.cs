namespace System.Application.Columns
{
    public interface INickName
    {
        string? NickName { get; set; }
    }

    public interface IReadOnlyNickName
    {
        string? NickName { get; }
    }
}