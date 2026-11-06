using System.Text;
using BD.SteamClient.Models;
using BD.SteamClient.Services.Implementation;
using BD.SteamClient.Services.Mvvm;
using Microsoft.Extensions.Logging.Abstractions;

namespace BD.WTTS.UnitTest;

public sealed class SteamServicePathTest
{
    string directory = null!;
    string first = null!;
    string second = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "WttsSteamPathTest-" + Guid.NewGuid());
        first = Path.Combine(directory, "first");
        second = Path.Combine(directory, "second");
        CreateSteam(first, "first-account", 1001, 129);
        CreateSteam(second, "second-account", 2001, 1);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(directory, recursive: true);

    [Test]
    public async Task ConfiguredProgramChangesExecutableDirectoryAndSteamFilesWithoutRecreatingService()
    {
        var service = new TestSteamService();
        var automaticProgram = service.SteamProgramPath;
        var automaticDirectory = service.SteamDirPath;

        service.ConfiguredPath = Path.Combine(first, "steam.exe");
        Assert.That(service.SteamProgramPath, Is.EqualTo(service.ConfiguredPath));
        Assert.That(service.SteamDirPath, Is.EqualTo(first));
        Assert.That(service.GetRememberUserList().Select(user => user.AccountName), Is.EqualTo(new[] { "first-account" }));
        Assert.That((await service.GetAppInfos()).Select(app => app.AppId), Is.EqualTo(Enumerable.Range(1001, 129).Select(id => (uint)id)));

        service.ConfiguredPath = Path.Combine(second, "steam.exe");
        Assert.That(service.SteamProgramPath, Is.EqualTo(service.ConfiguredPath));
        Assert.That(service.SteamDirPath, Is.EqualTo(second));
        Assert.That(service.GetRememberUserList().Select(user => user.AccountName), Is.EqualTo(new[] { "second-account" }));
        Assert.That((await service.GetAppInfos()).Select(app => app.AppId), Is.EqualTo(new uint[] { 2001 }));

        service.ConfiguredPath = null;
        Assert.That(service.SteamProgramPath, Is.EqualTo(automaticProgram));
        Assert.That(service.SteamDirPath, Is.EqualTo(automaticDirectory));
    }

    [Test]
    public async Task ChangingPathDuringBatchReadDoesNotMixTheTwoInstallations()
    {
        var service = new TestSteamService { ConfiguredPath = Path.Combine(first, "steam.exe") };
        var firstBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var resume = new ManualResetEventSlim();
        var ids = new List<uint>();
        var read = service.ReadAppInfosInBatchesAsync(batch =>
        {
            ids.AddRange(batch.Select(app => app.AppId));
            if (ids.Count == 128)
            {
                firstBatch.TrySetResult();
                if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Second batch was not released.");
            }
        });

        try
        {
            await firstBatch.Task.WaitAsync(TimeSpan.FromSeconds(10));
            service.ConfiguredPath = Path.Combine(second, "steam.exe");
        }
        finally { resume.Set(); }
        await read.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.That(ids, Is.EqualTo(Enumerable.Range(1001, 129).Select(id => (uint)id)));
        Assert.That((await service.GetAppInfos()).Select(app => app.AppId), Is.EqualTo(new uint[] { 2001 }));
    }

    static void CreateSteam(string root, string account, uint firstId, int count)
    {
        Directory.CreateDirectory(Path.Combine(root, "config"));
        Directory.CreateDirectory(Path.Combine(root, "appcache"));
        Directory.CreateDirectory(Path.Combine(root, "steamapps"));
        File.WriteAllBytes(Path.Combine(root, "steam.exe"), Array.Empty<byte>());
        File.WriteAllText(Path.Combine(root, "config", "loginusers.vdf"),
            $"\"users\"\n{{\n\"76561198000000001\"\n{{\n\"AccountName\" \"{account}\"\n\"PersonaName\" \"{account}\"\n\"Timestamp\" \"1700000000\"\n\"WantsOfflineMode\" \"0\"\n}}\n}}", Encoding.UTF8);
        using var writer = new BinaryWriter(File.Create(Path.Combine(root, "appcache", "appinfo.vdf")), Encoding.UTF8);
        writer.Write(123094055u);
        writer.Write(1u);
        for (var i = 0; i < count; i++)
        {
            using var payload = new MemoryStream();
            using (var data = new BinaryWriter(payload, Encoding.UTF8, true))
            {
                data.Write(new byte[16]);
                data.Write(new byte[20]);
                data.Write(1u);
                data.Write((byte)0); data.WriteAppInfoString("appinfo");
                data.Write((byte)0); data.WriteAppInfoString("common");
                data.Write((byte)1); data.WriteAppInfoString("name"); data.WriteAppInfoString($"Game {i}");
                data.Write((byte)1); data.WriteAppInfoString("type"); data.WriteAppInfoString("game");
                data.Write(new byte[] { 8, 8, 8 });
            }
            writer.Write(firstId + (uint)i);
            writer.Write((int)payload.Length);
            writer.Write(payload.GetBuffer(), 0, (int)payload.Length);
        }
        writer.Write(0u);
    }

    sealed class TestSteamService : SteamServiceImpl
    {
        public TestSteamService() : base(NullLoggerFactory.Instance) { }
        public string? ConfiguredPath { get; set; }
        protected override string? ConfiguredSteamProgramPath => ConfiguredPath;
        public override ISteamConnectService Conn => throw new NotSupportedException();
        protected override string? StratSteamDefaultParameter => null;
        protected override bool IsRunSteamAdministrator => false;
        protected override Dictionary<uint, string?>? HideGameList => null;
        protected override string? GetString(string name) => null;
    }
}
