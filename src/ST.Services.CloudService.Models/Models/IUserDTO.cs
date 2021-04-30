using System.Application.Columns;

namespace System.Application.Models
{
    public interface IUserDTO
    {
        Guid Id { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        Guid? Avatar { get; set; }

        protected static string GetDebuggerDisplay(IUserDTO user) => $"{user.NickName}: {user.Id}";
    }
}