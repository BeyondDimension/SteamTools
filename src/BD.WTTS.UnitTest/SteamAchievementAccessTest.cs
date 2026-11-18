using BD.SteamClient.Models;
using BD.SteamClient.Services;
using Moq;

namespace BD.WTTS.UnitTest;

public sealed class SteamAchievementAccessTest
{
    static readonly AchievementSet Dlc = new(2, "Expansion", 3075500, 0);

    [Test]
    public void DisplayFiltersByOwnershipInsteadOfInstallation()
    {
        var steam = new Mock<ISteamworksLocalApiService>();
        steam.Setup(x => x.OwnsApps(Dlc.DlcAppId)).Returns(true);
        steam.Setup(x => x.IsAppInstalled(Dlc.DlcAppId)).Returns(false);
        var missingDlc = new AchievementSet(3, "Unowned expansion", 3725110, 1);
        var achievements = new[] {
            new AchievementInfo { Id = "base" },
            new AchievementInfo { Id = "owned-1", Set = Dlc },
            new AchievementInfo { Id = "owned-2", Set = Dlc },
            new AchievementInfo { Id = "missing", Set = missingDlc },
        };
        var access = new AchievementAccess(steam.Object);

        Assert.That(achievements.Where(access.OwnsRequiredDlc).Select(x => x.Id), Is.EqualTo(new[] { "base", "owned-1", "owned-2" }));
        steam.Verify(x => x.OwnsApps(Dlc.DlcAppId), Times.Once);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RevokedDlcBlocksBothDirectionsBeforeStagingAnyAchievement(bool unlock)
    {
        var ownsDlc = true;
        var steam = new Mock<ISteamworksLocalApiService>();
        steam.Setup(x => x.OwnsApps(Dlc.DlcAppId)).Returns(() => ownsDlc);
        var baseAchievement = new AchievementInfo { Id = "base", IsAchieved = !unlock };
        var dlcAchievement = new AchievementInfo { Id = "dlc", Set = Dlc, IsAchieved = !unlock };
        Assert.That(new AchievementAccess(steam.Object).OwnsRequiredDlc(dlcAchievement), Is.True);
        ownsDlc = false;

        var result = new AchievementAccess(steam.Object).StageChanges([(baseAchievement, unlock), (dlcAchievement, unlock)], out var failed);

        Assert.That(result, Is.EqualTo(AchievementWriteResult.Restricted));
        Assert.That(failed, Is.SameAs(dlcAchievement));
        steam.Verify(x => x.SetAchievement(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [TestCase(2, false)]
    [TestCase(0, true)]
    public void BatchCannotModifyProtectedOrArchivedAchievements(int permission, bool archived)
    {
        var steam = new Mock<ISteamworksLocalApiService>();
        var entry = new AchievementInfo { Id = "retained", Permission = permission, IsArchived = archived };

        Assert.That(new AchievementAccess(steam.Object).StageChanges([(entry, true)], out _), Is.EqualTo(AchievementWriteResult.Restricted));
        steam.Verify(x => x.SetAchievement(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void OwnedDlcCanBeStagedWithoutOptimisticallyMarkingItUnlocked()
    {
        var steam = new Mock<ISteamworksLocalApiService>();
        steam.Setup(x => x.OwnsApps(Dlc.DlcAppId)).Returns(true);
        steam.Setup(x => x.SetAchievement("owned", true)).Returns(true);
        var entry = new AchievementInfo { Id = "owned", Set = Dlc };

        Assert.That(new AchievementAccess(steam.Object).StageChanges([(entry, true)], out _), Is.EqualTo(AchievementWriteResult.Staged));
        Assert.That(entry.IsAchieved, Is.False);
        steam.Verify(x => x.SetAchievement("owned", true), Times.Once);
    }
}
