using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorSdaFileImport : AuthenticatorFileImportBase
{
    public override string Name => "Mafile 文件导入";

    public override string Description => "从 Mafile 文件导入令牌";

    protected override string FileExtension => FileEx.maFile;

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSdaFileImport(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            await ImportFromSdaFile(password);
        });
    }
    
    async Task ImportFromSdaFile(string? password = null)
    {
        try
        {
            var filePath = await SelectFolderPath();
            
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
            await AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(authDto, password);
            Toast.Show(ToastIcon.Success, $"{authDto.Name} 导入成功");
        }
        catch (Exception e)
        {
            Log.Error(nameof(AuthenticatorSdaFileImport), e, nameof(ImportFromSdaFile));
            Toast.Show(ToastIcon.Error, $"导入失败，错误信息：{e.Message}");
        }
    }
}