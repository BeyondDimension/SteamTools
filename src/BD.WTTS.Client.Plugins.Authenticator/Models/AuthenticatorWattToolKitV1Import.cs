namespace BD.WTTS.Models;

public class AuthenticatorWattToolKitV1Import : AuthenticatorFileImportBase
{
    public override string Name => "WattToolKitV1 导入";
    
    public override string Description => "通过 WattToolKitV1 工具箱导出的文件，导入令牌";

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    protected override string FileExtension => FileEx.Dat;

    public AuthenticatorWattToolKitV1Import(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await ImportFromWattToolKitV1(password);
        });
    }
    
    async Task ImportFromWattToolKitV1(string? password = null)
    {
        var filePath = await SelectFolderPath();
        
        if (string.IsNullOrEmpty(filePath)) return;

        if (IOPath.TryReadAllText(filePath, out var content, out var _))
        {
            string authString;
            try
            {
                authString = content.DecompressString();
            }
            catch
            {
                return;
            }

            if (!string.IsNullOrEmpty(authString))
            {
                var reader = XmlReader.Create(new StringReader(authString));
                await reader.ReadAsync();
                while (reader is { EOF: false, IsEmptyElement: true })
                {
                    await reader.ReadAsync();
                }

                await reader.MoveToContentAsync();
                while (reader.EOF == false)
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "Auth")
                        {
                            await reader.ReadAsync();
                        }

                        if (reader.Name != "WinAuthAuthenticator") continue;
                        var authDto = new AuthenticatorDTO();
                        ReadXml(ref authDto, reader, null);
                        await SaveAuthenticator(authDto);
                    }
                    else
                    {
                        await reader.ReadAsync();
                        break;
                    }
                }
            }
        }
    }
}