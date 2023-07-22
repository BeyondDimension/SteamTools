using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorSdaFileImport : AuthenticatorFileImportBase
{
    public override string Name => Strings.LocalAuth_Import.Format(Strings.Mafile);

    public override string Description => Strings.LocalAuth_SDAImport;

    public override ResIcon IconName => ResIcon.OpenFile;

    protected override string FileExtension => FileEx.maFile;

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSdaFileImport(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await ImportFromSdaFile(password);
        });
    }
    
    async Task ImportFromSdaFile(string? password = null)
    {
        try
        {
            var filePath = await SelectFolderPath();
            
            if (string.IsNullOrEmpty(filePath)) return;

            var text = await File.ReadAllTextAsync(filePath);
            
            var index = text.IndexOf("\"server_time\":", StringComparison.Ordinal) + 14;
            if (index != -1)
            {
                var temp = text.Substring(index, text[index..].IndexOf(",", StringComparison.Ordinal));
                if (!temp.Contains('"'))
                {
                    var temp2 = "\"" + temp + "\"";
                    text = text.Replace(temp, temp2);
                }
            }

            var sdaFileModel = JsonSerializer.Deserialize(text, ImportFileModelJsonContext.Default.SdaFileModel);
            sdaFileModel.ThrowIsNull();
            var steamDataModel = new SdaFileConvertToSteamDataModel(sdaFileModel);
            
            SteamAuthenticator steamAuthenticator = new()
            {
                DeviceId = sdaFileModel.DeviceId,
                Serial = sdaFileModel.SerialNumber,
                SecretKey = Convert.FromBase64String(sdaFileModel.SharedSecret),
                SteamData = JsonSerializer.Serialize(steamDataModel, ImportFileModelJsonContext.Default.SdaFileConvertToSteamDataModel),
            };
            
            var authDto =
                new AuthenticatorDTO()
                {
                    Name = $"(Steam){steamAuthenticator.AccountName}", Value = steamAuthenticator, Created = DateTimeOffset.Now,
                };
            await SaveAuthenticator(authDto);
            Toast.Show(ToastIcon.Success, Strings.ModelContent_ImportSuccessful_.Format(authDto.Name));
        }
        catch (Exception e)
        {
            e.LogAndShowT();
        }
    }
}