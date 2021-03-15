namespace System.Application.Models
{
    /// <summary>
    /// 游戏平台令牌可传输模型
    /// </summary>
    public interface IGAPAuthenticatorDTO
    {
        ushort Id { get; set; }

        public int Index { get; set; }

        string Name { get; set; }

        GamePlatform Platform { get; }

        Guid? ServerId { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastUpdate { get; set; }

        IGAPAuthenticatorValueDTO Value { get; set; }
    }
}