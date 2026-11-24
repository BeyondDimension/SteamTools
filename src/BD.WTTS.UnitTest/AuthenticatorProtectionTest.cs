using System.Security.Cryptography;
using BD.Common.Enums;
using BD.Common.Services;
using BD.Common.Services.Implementation;
using BD.WTTS.Entities;
using BD.WTTS.Models;
using BD.WTTS.Models.Abstractions;
using BD.WTTS.Repositories;
using BD.WTTS.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WinAuth;
using static BD.WTTS.Repositories.Abstractions.IAccountPlatformAuthenticatorRepository;

namespace BD.WTTS.UnitTest;

public sealed class AuthenticatorProtectionTest
{
    const string Password = "synthetic-vault-password";
    const string NextPassword = "different-synthetic-password";

    [Test]
    public async Task PortableBatchSharesSaltButUsesFreshNoncesAndAuthenticatesBothFields()
    {
        using var services = CreateSecurityServices();
        var repository = new AccountPlatformAuthenticatorRepository(services.GetRequiredService<ISecurityService>());
        var batch = new[] { NewToken("fixture"), NewToken("fixture") };
        var export = await repository.ExportAsync(false, Password, batch);
        var rows = ReadExport(export);
        var secondRows = ReadExport(await repository.ExportAsync(false, Password, batch));
        var salt = AuthenticatorEnvelope.PasswordSalt(rows[0].Value);
        Assert.That(salt, Has.Length.EqualTo(16));
        Assert.That(AuthenticatorEnvelope.PasswordSalt(rows[0].Name), Is.EqualTo(salt));
        Assert.That(AuthenticatorEnvelope.PasswordSalt(rows[1].Value), Is.EqualTo(salt));
        Assert.That(AuthenticatorEnvelope.PasswordSalt(secondRows[0].Value), Is.Not.EqualTo(salt));
        Assert.That(rows[0].Value!.AsSpan(23, 12).ToArray(), Is.Not.EqualTo(rows[1].Value!.AsSpan(23, 12).ToArray()));
        var correct = await repository.ImportAsync(Password, export);
        Assert.That(correct.resultCode, Is.EqualTo(ImportResultCode.Success));
        Assert.That(correct.result.Select(x => x.Name), Is.EqualTo(new[] { "fixture", "fixture" }));
        Assert.That(correct.result[0].Value.SecretKey, Is.EqualTo(batch[0].Value.SecretKey));
        var wrong = await repository.ImportAsync(NextPassword, export);
        Assert.That(wrong.resultCode, Is.EqualTo(ImportResultCode.SecondaryPasswordFail));
        Assert.That(wrong.result, Is.Empty);

        rows[1].Name![^1] ^= 1;
        var tampered = await repository.ImportAsync(Password, Serializable.SMP(rows));
        Assert.That(tampered.resultCode, Is.Not.EqualTo(ImportResultCode.Success));
        Assert.That(tampered.result, Is.Empty);
        Assert.That(tampered.sourcesCount, Is.EqualTo(2));

        rows[1].Name = rows[1].Value;
        var wrongPurpose = await repository.ImportAsync(Password, Serializable.SMP(rows));
        Assert.That(wrongPurpose.resultCode, Is.EqualTo(ImportResultCode.IncorrectFormat));
        Assert.That(wrongPurpose.result, Is.Empty);
    }

    [Test]
    public async Task WindowsCurrentUserScopeRoundTripsAndDoesNotFallBackToPortable()
    {
        using var services = CreateSecurityServices();
        var security = services.GetRequiredService<ISecurityService>();
        using var keys = new AuthenticatorEnvelope.Keys(null);
        const AuthenticatorEnvelope.Purpose purpose = AuthenticatorEnvelope.Purpose.Name;
        if (!OperatingSystem.IsWindows())
        {
            // Structurally valid prefix for Windows CurrentUser; no DPAPI call can be made here.
            byte[] windowsEnvelope = [(byte)'W', (byte)'T', (byte)'A', (byte)'!', 1, 2, (byte)purpose, 0];
            var failure = await Assert.ThrowsAsync<AuthenticatorReadException>(async () =>
                await AuthenticatorEnvelope.OpenAsync(security, windowsEnvelope, purpose, true, false, keys));
            Assert.That(failure!.ResultCode, Is.EqualTo(ImportResultCode.LocalFail));
            return;
        }

        var plaintext = "local fixture"u8.ToArray();
        var local = await AuthenticatorEnvelope.SealAsync(security, plaintext, purpose, true, null, keys);
        Assert.That(local[5], Is.EqualTo(2));
        Assert.That(await AuthenticatorEnvelope.OpenAsync(security, local, purpose, true, false, keys), Is.EqualTo(plaintext));
        var wrongScope = await Assert.ThrowsAsync<AuthenticatorReadException>(async () =>
            await AuthenticatorEnvelope.OpenAsync(security, local, purpose, false, false, keys));
        Assert.That(wrongScope!.ResultCode, Is.EqualTo(ImportResultCode.IncorrectFormat));
    }

    [Test]
    public async Task LegacyEncryptedAndPlainRowsCanBeReadTogether()
    {
        using var services = CreateSecurityServices();
        var security = services.GetRequiredService<ISecurityService>();
        var repository = new AccountPlatformAuthenticatorRepository(security);
        var serialized = Serializable.SMP(NewToken("fixture").Value);
        var rows = new[]
        {
            new AccountPlatformAuthenticator
            {
                IsNotLocal = true,
                Name = "plain"u8.ToArray(),
                Value = serialized,
            },
            new AccountPlatformAuthenticator
            {
                IsNotLocal = true,
                IsNeedSecondaryPassword = true,
                Name = await security.E("legacy", EncryptionMode.EmbeddedAes, null),
                Value = await security.EB(serialized, EncryptionMode.EmbeddedAesWithSecondaryPassword, Password),
            },
        };
        var decoded = await repository.ConvertToListAsync(rows, Password);
        Assert.That(decoded.Select(x => x.Name), Is.EqualTo(new[] { "plain", "legacy" }));
        var failure = await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.ConvertToListAsync(rows, NextPassword));
        Assert.That(failure!.ResultCode, Is.Not.EqualTo(ImportResultCode.Success));
    }

    [Test]
    public async Task SwitchRollsBackOnSqlFailureAndOnlyAcceptsNewPasswordAfterCommit()
    {
        using var services = CreateSecurityServices();
        SQLitePCL.Batteries_V2.Init();
        var folder = Directory.CreateTempSubdirectory();
        var connection = new SQLiteAsyncConnection(Path.Combine(folder.FullName, "vault.db"));
        try
        {
            await connection.CreateTableAsync<AccountPlatformAuthenticator>();
            var security = services.GetRequiredService<ISecurityService>();
            var repository = new AccountPlatformAuthenticatorRepository(security, connection);
            var batch = new[] { NewToken("first"), NewToken("second") };
            await repository.InsertOrUpdateAsync(batch, false);
            var legacyProtectedValue = await security.EB(Serializable.SMP(batch[1].Value), EncryptionMode.EmbeddedAesWithSecondaryPassword, Password);
            await connection.ExecuteAsync($"UPDATE [{AccountPlatformAuthenticator.TableName}] SET [60FAF486] = ?, [4AF8A895] = 1 " +
                $"WHERE [{AccountPlatformAuthenticator.ColumnName_Id}] = ?", legacyProtectedValue, batch[1].Id);
            var before = await repository.GetAllSourceAsync();
            Assert.That(await repository.HasSecondaryPasswordAsync(), Is.True);
            Assert.That(before.Select(x => x.IsNeedSecondaryPassword), Is.EqualTo(new[] { false, true }));
            await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.SwitchEncryptionModeAsync(false, NextPassword, null));

            await connection.ExecuteAsync($"CREATE TRIGGER reject_second BEFORE UPDATE ON [{AccountPlatformAuthenticator.TableName}] " +
                $"WHEN OLD.[{AccountPlatformAuthenticator.ColumnName_Id}] = {before[1].Id} BEGIN SELECT RAISE(ABORT, 'fixture failure'); END");
            await Assert.ThrowsAsync<SQLiteException>(() => repository.SwitchEncryptionModeAsync(false, Password, NextPassword));
            var afterFailure = await repository.GetAllSourceAsync();
            Assert.That(afterFailure.Select(x => x.Value), Is.EqualTo(before.Select(x => x.Value)));
            Assert.That(afterFailure.Select(x => x.Name), Is.EqualTo(before.Select(x => x.Name)));
            Assert.That(afterFailure.Select(x => x.IsNeedSecondaryPassword), Is.EqualTo(before.Select(x => x.IsNeedSecondaryPassword)));

            await connection.ExecuteAsync("DROP TRIGGER reject_second");
            await repository.SwitchEncryptionModeAsync(false, Password, NextPassword);
            await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.InsertOrUpdateAsync(NewToken("stale writer"), false, Password));
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.InsertOrUpdateAsync(NewToken("downgrade"), false));
            await repository.RenameAsync(batch[1].Id, "renamed", true, NextPassword); // Caller hint is deliberately wrong.
            var after = await repository.GetAllSourceAsync();
            Assert.That(after.Select(x => x.Id), Is.EqualTo(before.Select(x => x.Id)));
            Assert.That(after.Select(x => x.LastUpdate), Is.EqualTo(before.Select(x => x.LastUpdate)));
            Assert.That(after.Select(x => x.IsNotLocal), Is.EqualTo(before.Select(x => x.IsNotLocal)));
            Assert.That(after.All(x => x.IsNeedSecondaryPassword && AuthenticatorEnvelope.IsEnvelope(x.Name) && AuthenticatorEnvelope.IsEnvelope(x.Value)), Is.True);
            Assert.That(AuthenticatorEnvelope.PasswordSalt(after[0].Value), Is.EqualTo(AuthenticatorEnvelope.PasswordSalt(after[1].Name)));
            var decoded = await repository.ConvertToListAsync(after, NextPassword);
            Assert.That(decoded.Select(x => x.Name), Is.EqualTo(new[] { "first", "renamed" }));
        }
        finally
        {
            await connection.CloseAsync();
            folder.Delete(recursive: true);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task HelloModeNeedsVerificationWithoutAStoredPassword(bool migratePassword)
    {
        if (!OperatingSystem.IsWindows()) Assert.Ignore("Windows CurrentUser DPAPI is required.");
        using var services = CreateSecurityServices();
        var folder = Directory.CreateTempSubdirectory();
        var connection = new SQLiteAsyncConnection(Path.Combine(folder.FullName, "hello-vault.db"));
        try
        {
            await connection.CreateTableAsync<AccountPlatformAuthenticator>();
            var repository = new AccountPlatformAuthenticatorRepository(services.GetRequiredService<ISecurityService>(), connection);
            var item = NewToken("original");
            var originalPassword = migratePassword ? Password : null;
            await repository.InsertOrUpdateAsync(item, false, originalPassword);
            var before = (await repository.GetAllSourceAsync()).Single();

            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.SwitchEncryptionModeAsync(
                true, originalPassword, null, windowsHello: true));
            if (migratePassword)
                await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.SwitchEncryptionModeAsync(
                    true, NextPassword, null, windowsHello: true, verifiedWithHello: true));
            Assert.That((await repository.GetAllSourceAsync()).Single().Value, Is.EqualTo(before.Value));

            await repository.SwitchEncryptionModeAsync(true, originalPassword, null, windowsHello: true, verifiedWithHello: true);
            var local = await repository.GetAllSourceAsync();
            Assert.That(local.Single().IsNotLocal, Is.False);
            Assert.That(local.Single().IsNeedSecondaryPassword, Is.False);
            var blocked = await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.ConvertToListAsync(local, originalPassword));
            Assert.That(blocked!.ResultCode, Is.EqualTo(ImportResultCode.WindowsHelloRequired));

            var unlocked = await repository.ConvertToListAsync(local, verifiedWithHello: true);
            Assert.That(unlocked.Single().Value.SecretKey, Is.EqualTo(item.Value.SecretKey));
            await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.InsertOrUpdateAsync(NewToken("blocked"), true));
            await repository.InsertOrUpdateAsync(NewToken("added"), true, verifiedWithHello: true);
            await repository.RenameAsync(item.Id, "renamed", true, verifiedWithHello: true);
            var updated = await repository.GetAllSourceAsync();
            Assert.That(updated.All(AccountPlatformAuthenticatorRepository.RequiresWindowsHello), Is.True);
            var backup = await repository.ExportAsync(false, NextPassword,
                await repository.ConvertToListAsync(updated, verifiedWithHello: true));
            Assert.That((await repository.ImportAsync(NextPassword, backup)).result.Select(x => x.Name),
                Is.EqualTo(new[] { "renamed", "added" }));

            await Assert.ThrowsAsync<AuthenticatorReadException>(() => repository.SwitchEncryptionModeAsync(true, null, NextPassword));
            await repository.SwitchEncryptionModeAsync(true, null, NextPassword, verifiedWithHello: true);
            var passwordRows = await repository.GetAllSourceAsync();
            Assert.That(passwordRows.Any(AccountPlatformAuthenticatorRepository.RequiresWindowsHello), Is.False);
            Assert.That((await repository.ConvertToListAsync(passwordRows, NextPassword)).Select(x => x.Name),
                Is.EqualTo(new[] { "renamed", "added" }));
        }
        finally
        {
            await connection.CloseAsync();
            folder.Delete(recursive: true);
        }
    }

    static ServiceProvider CreateSecurityServices() => new ServiceCollection()
        .AddSecurityService<FixtureEmbeddedAes, FixtureLocalProtection>().BuildServiceProvider();

    static AccountPlatformAuthenticator[] ReadExport(byte[] bytes) =>
        Serializable.DMP<AccountPlatformAuthenticator[]>(bytes)!;

    static AuthenticatorDTO NewToken(string name) => new()
    {
        Name = name,
        Value = new GoogleAuthenticator { SecretKey = [1, 3, 5, 7, 9] },
    };

    sealed class FixtureEmbeddedAes : EmbeddedAesDataProtectionProviderBase, IDisposable
    {
        public FixtureEmbeddedAes() => Aes = [System.Security.Cryptography.Aes.Create()];

        public override Aes[] Aes { get; }

        public void Dispose() => Aes[0].Dispose();
    }

    sealed class FixtureLocalProtection : LocalDataProtectionProviderBase
    {
        public FixtureLocalProtection(ILocalDataProtectionProvider.IProtectedData protectedData,
            ILocalDataProtectionProvider.IDataProtectionProvider dataProtectionProvider) : base(protectedData, dataProtectionProvider)
        {
        }

        protected override (byte[] key, byte[] iv) MachineSecretKey => (new byte[32], new byte[16]);
    }
}
