using WinAuth;

namespace BD.WTTS.Models;

public abstract class AuthenticatorFileImportBase : AuthenticatorImportBase
{
    public abstract override string Name { get; }

    public abstract override string Description { get; }

    protected abstract string? FileExtension { get; }

    public abstract override ICommand AuthenticatorImportCommand { get; set; }

    protected async Task<string?> SelectFolderPath()
    {
        var filePath = (await SelectFile())?.FullPath.TrimStart("file:///");
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) Toast.Show(ToastIcon.Warning, Strings.FilePathNotExist);
        return filePath;
    }

    async Task<IFileResult?> SelectFile()
    {
        var options = new PickOptions();
        if (string.IsNullOrEmpty(FileExtension)) return await FilePicker2.PickAsync(options);

        FilePickerFileType fileTypes = new[] { ($"{FileExtension} Files", new[] { "*" + FileExtension, }), };
        options.FileTypes = fileTypes;
        return await FilePicker2.PickAsync(options);
    }

    protected bool ReadXml(ref AuthenticatorDTO authenticatorDto, XmlReader reader, string? password)
    {
        bool changed = false;

        var authenticatorType = reader.GetAttribute("type");
        switch (authenticatorType)
        {
            case "WinAuth.SteamAuthenticator":
                authenticatorDto.Value = new SteamAuthenticator();
                break;
            case "WinAuth.BattleNetAuthenticator":
                authenticatorDto.Value = new BattleNetAuthenticator();
                break;
            case "WinAuth.GoogleAuthenticator":
                authenticatorDto.Value = new GoogleAuthenticator();
                break;
            case "WinAuth.HOTPAuthenticator":
                authenticatorDto.Value = new HOTPAuthenticator();
                break;
            case "WinAuth.MicrosoftAuthenticator":
                authenticatorDto.Value = new MicrosoftAuthenticator();
                break;
            default:
                return false;
        }

        reader.MoveToContent();

        if (reader.IsEmptyElement)
        {
            reader.Read();
            return changed;
        }

        reader.Read();
        while (reader.EOF == false)
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "name":
                        authenticatorDto.Name = reader.ReadElementContentAsString();
                        break;

                    case "created":
                        long t = reader.ReadElementContentAsLong();
                        t += Convert.ToInt64(new TimeSpan(new DateTime(1970, 1, 1).Ticks).TotalMilliseconds);
                        t *= TimeSpan.TicksPerMillisecond;
                        authenticatorDto.Created = new DateTimeOffset(new DateTime(t).ToLocalTime());
                        break;

                    case "authenticatordata":
                        try
                        {
                            // we don't pass the password as they are locked till clicked
                            changed = authenticatorDto.Value!.ReadXml(reader) || changed;
                        }
                        catch (WinAuthEncryptedSecretDataException)
                        {
                            // no action needed
                        }
                        catch (WinAuthBadPasswordException)
                        {
                            // no action needed
                        }

                        break;

                    // v2
                    case "authenticator":
                        authenticatorDto.Value = AuthenticatorValueDTO.ReadXmlv2(reader, password);
                        break;
                    // v2
                    case "servertimediff":
                        authenticatorDto.Value!.ServerTimeDiff = reader.ReadElementContentAsLong();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }
            else
            {
                reader.Read();
                break;
            }
        }

        return changed;
    }

    protected IEnumerable<string> ReadUrlsByFilePath(string filePath)
    {
        StringBuilder lines = new();
        bool retry;
        do
        {
            retry = false;
            lines.Length = 0;
            // read a plain text file
            lines.Append(File.ReadAllText(filePath));
        } while (retry);

        using var sr = new StringReader(lines.ToString());
        while (sr.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}