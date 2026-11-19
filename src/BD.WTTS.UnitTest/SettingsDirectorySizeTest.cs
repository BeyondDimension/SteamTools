using BD.WTTS.UI.ViewModels;

namespace BD.WTTS.UnitTest;

public sealed class SettingsDirectorySizeTest
{
    [Test]
    public async Task RapidVisitsReuseTheScanAndChangingDirectoryGetsItsOwnSize()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var cache = new SettingsPageViewModel.DirectorySizeMeasurement(TimeSpan.FromMinutes(1));
            var other = directory.CreateSubdirectory("other");
            File.WriteAllBytes(Path.Combine(directory.FullName, "data"), new byte[17]);
            var scans = Enumerable.Range(0, 20).AsParallel().Select(_ => cache.GetAsync(directory.FullName)).ToArray();
            Assert.That(await Task.WhenAll(scans), Is.All.EqualTo(17L));
            Assert.That(scans.Distinct().Count(), Is.EqualTo(1));
            File.WriteAllBytes(Path.Combine(directory.FullName, "data"), new byte[31]);
            Assert.That(await cache.GetAsync(directory.FullName), Is.EqualTo(17L));
            Assert.That(await cache.GetAsync(other.FullName), Is.Zero);

            var expired = new SettingsPageViewModel.DirectorySizeMeasurement(TimeSpan.Zero);
            Assert.That(await expired.GetAsync(directory.FullName), Is.EqualTo(31L));
            File.WriteAllBytes(Path.Combine(directory.FullName, "data"), new byte[42]);
            Assert.That(await expired.GetAsync(directory.FullName), Is.EqualTo(42L));
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
