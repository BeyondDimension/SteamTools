using System.Application.Columns;

namespace System.Application.Models
{
    /// <summary>
    /// 游戏平台令牌可传输模型
    /// </summary>
    public interface IGAPAuthenticatorDTO : IOrderGAPAuthenticator
    {
        string Name { get; set; }

        GamePlatform Platform { get; }

        Guid? ServerId { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastUpdate { get; set; }

        IGAPAuthenticatorValueDTO Value { get; set; }
    }
}