namespace System.Application.Models
{
    public interface IGameAccountPlatformAuthenticatorDTO
    {
        ushort Id { get; set; }

        string Name { get; set; }

        GamePlatform Platform { get; }

        Guid? ServerId { get; set; }

        IGameAccountPlatformAuthenticatorValueDTO Value { get; set; }
    }
}