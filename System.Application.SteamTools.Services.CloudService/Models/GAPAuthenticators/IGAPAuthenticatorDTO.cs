namespace System.Application.Models
{
    /// <summary>
    /// 游戏平台令牌可传输模型
    /// </summary>
    public interface IGAPAuthenticatorDTO
    {
        ushort Id { get; set; }

        string Name { get; set; }

        GamePlatform Platform { get; }

        Guid? ServerId { get; set; }

        IGAPAuthenticatorValueDTO Value { get; set; }
    }
}