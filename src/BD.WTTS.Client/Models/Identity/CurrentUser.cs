// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

/// <summary>
/// 当前登录用户模型，如需增加字段，还需要在 <see cref="Clone"/> 中赋值新添加字段
/// </summary>
[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class CurrentUser : IExplicitHasValue, IPhoneNumber
{
    [MPKey(0), MP2Key(0)]
    [N_JsonProperty("0")]
    [S_JsonProperty("0")]
    public Guid UserId { get; set; }

    [MPKey(1), MP2Key(1)]
    [N_JsonProperty("1")]
    [S_JsonProperty("1")]
    public JWTEntity? AuthToken { get; set; }

    [MPKey(2), MP2Key(2)]
    [N_JsonProperty("2")]
    [S_JsonProperty("2")]
    public string? PhoneNumber { get; set; }

    [MPKey(3), MP2Key(3)]
    [N_JsonProperty("3")]
    [S_JsonProperty("3")]
    public JWTEntity? ShopAuthToken { get; set; }

    bool IExplicitHasValue.ExplicitHasValue()
    {
        if (!AuthToken.HasValue()) return false;
        return true;
    }

    public CurrentUser? Clone() => this.HasValue() ?
        new()
        {
            UserId = UserId,
            AuthToken = AuthToken,
            PhoneNumber = PhoneNumber,
            ShopAuthToken = ShopAuthToken,
        } : null;
}