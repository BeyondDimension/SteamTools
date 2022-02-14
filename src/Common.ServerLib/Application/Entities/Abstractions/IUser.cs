namespace System.Application.Entities.Abstractions
{
    public interface IUser : IEntity<Guid>
    {
        /// <summary>
        /// 昵称
        /// </summary>
        string? NickName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        string? UserName { get; set; }

        /// <summary>
        /// 封禁到解封日期
        /// </summary>
        DateTimeOffset? BanEnd { get; set; }
    }
}