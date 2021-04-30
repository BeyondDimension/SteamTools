namespace System.Application.Entities
{
    public interface IUser<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 昵称
        /// </summary>
        string? NickName { get; set; }
    }
}