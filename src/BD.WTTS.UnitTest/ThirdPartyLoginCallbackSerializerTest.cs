using BD.Common.Enums;
using MessagePack;

namespace BD.WTTS.UnitTest;

public sealed class ThirdPartyLoginCallbackSerializerTest
{
    [TestCase(false)]
    [TestCase(true)]
    public void ReadsLegacyLoginResponseWithTokensAndLinkedAccount(bool compress)
    {
        var expires = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.FromHours(8));
        var userId = Guid.Parse("2D40216E-C944-4EB8-8F37-EDC4B820C34A");
        var token = new string('a', 100);
        var original = ApiRspHelper.Ok(new LoginOrRegisterResponse
        {
            AuthToken = new JWTEntity { ExpiresIn = expires, AccessToken = token, RefreshToken = "offline-refresh" },
            User = new IdentityUserInfoDTO
            {
                Id = userId,
                NickName = "离线用户",
                Gender = Gender.Female,
                UserType = UserType.Sponsor,
                Balance = 12.34m,
                SteamAccountId = 76561198000000001,
                AvatarUrl = new() { [ExternalLoginChannel.Steam] = "https://example.invalid/avatar" },
            },
            IsLoginOrRegister = true,
            PhoneNumber = "offline-phone",
            ExternalLoginChannel = ExternalLoginChannel.Steam,
        });
        var bytes = compress ? Serializable.SMP(original) : MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);

        var result = ApiRspHelper.Deserialize<LoginOrRegisterResponse>(bytes);
        var login = result.Content!;
        var user = login.User!;
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(login.IsLoginOrRegister, Is.True);
            Assert.That(login.AuthToken!.ExpiresIn.EqualsExact(expires), Is.True);
            Assert.That(login.AuthToken.AccessToken, Is.EqualTo(token));
            Assert.That(login.AuthToken.RefreshToken, Is.EqualTo("offline-refresh"));
            Assert.That(login.PhoneNumber, Is.EqualTo("offline-phone"));
            Assert.That(login.ExternalLoginChannel, Is.EqualTo(ExternalLoginChannel.Steam));
            Assert.That(user.Id, Is.EqualTo(userId));
            Assert.That(user.NickName, Is.EqualTo("离线用户"));
            Assert.That(user.Gender, Is.EqualTo(Gender.Female));
            Assert.That(user.UserType, Is.EqualTo(UserType.Sponsor));
            Assert.That(user.Balance, Is.EqualTo(12.34m));
            Assert.That(user.SteamAccountId, Is.EqualTo(76561198000000001L));
            Assert.That(user.AvatarUrl![ExternalLoginChannel.Steam], Is.EqualTo("https://example.invalid/avatar"));
        });
    }

    [Test]
    public void ReadsLegacyErrorWithoutAContentSlot()
    {
        var bytes = Serializable.SMP(ApiRspHelper.Code(ApiRspCode.Unauthorized, "offline rejection"));
        var result = ApiRspHelper.Deserialize<LoginOrRegisterResponse>(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Code, Is.EqualTo(ApiRspCode.Unauthorized));
            Assert.That(ApiRspHelper.GetInternalMessage(result), Is.EqualTo("offline rejection"));
            Assert.That(result.Content, Is.Null);
        });
    }
}
