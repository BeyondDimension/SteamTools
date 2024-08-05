using BsDiff;
using JsonSerializer = System.Text.Json.JsonSerializer;
using BD.WTTS.Models;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface IGenerateBsPatch : ICommand
{
    const string commandName = "bs";
    const string BaseUrl =
#if DEBUG
        "https://localhost:9881";

#else
        "http://192.168.2.16:19001";
#endif

    static Command ICommand.GetCommand()
    {
        var type = new Option<int>("--type", "type 0:Create 1:Apply 2:UploadFile 3:传入版本号生成补丁");
        //旧文件
        var oldFile = new Option<string>("--old", "old file");
        //新文件
        var newFile = new Option<string>("--new", "new file");
        //保存路径
        var patchFile = new Option<string>("--path", "new file");
        //程序版本号
        var clientPlatform = new Option<ClientPlatform>("--platform", "ClientPlatform");

        var command = new Command(commandName, "Bridge csproj xml generate")
        {
         type,  oldFile, newFile, patchFile, clientPlatform
        };
        command.SetHandler(Handler, type, oldFile, newFile, patchFile, clientPlatform);
        return command;
    }

    internal static void Handler(int type, string oldFile, string newFile, string patchFile, ClientPlatform platform = ClientPlatform.Win32X64)
    {
        if (type < 2)
        {
            if (!File.Exists(oldFile))
            {
                Console.WriteLine("old file not exists");
                return;
            }
            if (!File.Exists(newFile))
            {
                Console.WriteLine("new file not exists");
                return;
            }
            if (string.IsNullOrWhiteSpace(patchFile))
            {
                Console.WriteLine("The patch file path cannot be empty");
                return;
            }
        }
        switch (type)
        {
            case 0:
                using (var output = new FileStream(patchFile, FileMode.Create))
                    GenerateBsPatch(File.ReadAllBytes(oldFile), File.ReadAllBytes(newFile), output);
                break;
            case 1:
                using (var input = new FileStream(oldFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var output = new FileStream(newFile, FileMode.Create))
                    BinaryPatch.Apply(input, () => new FileStream(patchFile, FileMode.Open, FileAccess.Read, FileShare.Read), output);
                break;
            case 2:
                UploadFiles(oldFile).Wait();
                break;
            case 3:
                GenerateAppPublishPatchBundleInfo(oldFile, newFile, platform, patchFile).Wait();
                break;
            case 4:
                UploadAppVer(oldFile, newFile, platform).Wait();
                break;
            case 5:
                ApplyPatch(oldFile, newFile, patchFile).Wait();
                break;

        }
        Console.WriteLine("end");
    }

    /// <summary>
    /// 应用补丁
    /// </summary>
    /// <param name="applyPath"></param>
    /// <param name="saveFilePath"></param>
    /// <param name="patchPath"></param>
    /// <returns></returns>
    internal static async ThreadTask ApplyPatch(string applyPath, string saveFilePath, string patchPath)
    {
        if (!File.Exists(patchPath))
        {
            Console.WriteLine("PatchPath not exists");
            return;
        }

        if (!Directory.Exists(applyPath))
        {
            Console.WriteLine("ApplyPath not exists");
            return;
        }
        var fileStream = File.OpenRead(patchPath);
        var jsonData = new BrotliStream(fileStream, CompressionMode.Decompress);
        var dic = await MemoryPackSerializer.DeserializeAsync<Dictionary<string, PatchBundleInfo?>>(jsonData);
        if (dic is null)
        {
            Console.WriteLine("Patch Error");
            return;
        }
        if (!Directory.Exists(saveFilePath))
        {
            Directory.CreateDirectory(saveFilePath);
        }
        else
        {
            Directory.Delete(saveFilePath);
        }
        CopyDirectory(applyPath, saveFilePath);
        FileInfo? fileInfo = null;
        foreach (var item in dic.Where(x => x.Value is not null))
        {
            PatchBundleInfo info = item.Value!;
            if (info.PatchData is not null)
            {
                fileInfo = new FileInfo(Path.Combine(saveFilePath, item.Key));
                if (!(fileInfo.Directory?.Exists ?? false))
                    fileInfo.Directory?.Create();
                if (info.IsFullOrPatch)
                {
                    File.WriteAllBytes(Path.Combine(saveFilePath, item.Key), info.PatchData);
                }
                else
                {
                    using var oldFile = File.OpenRead(Path.Combine(applyPath, item.Key));
                    var newFile = Path.Combine(saveFilePath, item.Key);
                    using var saveFile = File.Exists(newFile) ? File.Create(newFile) : File.OpenWrite(newFile);
                    BinaryPatch.Apply(oldFile, () => new MemoryStream(info.PatchData), saveFile);
                }
            }
        }
        Console.WriteLine("ApplyPatch OK");
    }

    /// <summary>
    /// 复制目录下的文件
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="targetDir"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void CopyDirectory(string sourceDir, string targetDir)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDir}");
        }

        // If the destination directory does not exist, create it.
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(targetDir, file.Name);
            file.CopyTo(tempPath, false);
        }

        // If copying subdirectories, copy them and their contents to the new location.
        foreach (DirectoryInfo subdir in dirs)
        {
            string tempPath = Path.Combine(targetDir, subdir.Name);
            CopyDirectory(subdir.FullName, tempPath);
        }
    }

    /// <summary>
    /// 根据 2个 文件生成补丁
    /// </summary>
    /// <param name="oldByte"></param>
    /// <param name="newByte"></param>
    /// <param name="output"></param>
    internal static void GenerateBsPatch(byte[] oldByte, byte[] newByte, Stream output)
    {
        BinaryPatch.Create(oldByte, newByte, output);
    }

    /// <summary>
    /// 上传版本信息 加 FileList
    /// </summary>
    /// <param name="appVer"></param>
    /// <param name="jsonPath">发布工具生成的 Json</param>
    /// <param name="platform"></param>
    /// <returns></returns>
    internal static async ThreadTask UploadAppVer(string appVer, string jsonPath, ClientPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(appVer))
        {
            Console.WriteLine("AppVer not exists");
            return;
        }
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            Console.WriteLine("jsonPath not exists");
            return;
        }
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("jsonPath not Find");
            return;
        }

        var fileListRaw = File.ReadAllBytes(jsonPath);
        var appver = await GetAppVer(appVer);
        if (appver is not null)
        {
            var thisFileList = appver.FileList?.FirstOrDefault(x => x.Platform == platform);
            if (thisFileList is not null)
            {
                if (Hashs.String.SHA384(thisFileList.FileListRaw) != Hashs.String.SHA384(fileListRaw))
                {
                    thisFileList.FileListRaw = fileListRaw;
                }
            }
            else
            {
                if (appver.FileList is null)
                {
                    appver.FileList = new List<AppVerFileList>
                {
                    new AppVerFileList
                    {
                        Platform = platform,
                        FileListRaw = fileListRaw
                    }
                };
                }
                else
                {
                    appver.FileList.Add(new AppVerFileList
                    {
                        Platform = platform,
                        FileListRaw = fileListRaw
                    });
                }
            }
            await UploadAppVer(appver);
        }
        else
        {
            appver = new AppVer
            {
                Platform = platform,
                Ver = appVer,
                Published = DateTimeOffset.Now,
                FileList = new List<AppVerFileList>
                {
                    new AppVerFileList
                    {
                        Platform = platform,
                        FileListRaw = fileListRaw
                    }
                }
            };
            await UploadAppVer(appver);
        }
    }

    /// <summary>
    /// 上传版本信息
    /// </summary>
    /// <param name="appVer"></param>
    /// <returns></returns>
    internal static async ThreadTask UploadAppVer(AppVer appVer)
    {
        using var httpClient = new HttpClient();
        var content = JsonContent.Create(appVer);
        var response = await httpClient.PostAsync($"{BaseUrl}/appver/", content);
    }

    /// <summary>
    /// 传入 2 个版本号生成补丁
    /// </summary>
    /// <param name="oldVer"></param>
    /// <param name="newVer"></param>
    /// <param name="platform"></param>
    /// <param name="patchFilePath"></param>
    /// <returns></returns>
    internal static async ThreadTask GenerateAppPublishPatchBundleInfo(string oldVer, string newVer, ClientPlatform platform, string patchFilePath)
    {
        var oldAppVer = await GetAppVer(oldVer);
        var newAppVer = await GetAppVer(newVer);
        if (oldAppVer is null || newAppVer is null)
        {
            Console.WriteLine("AppVer not exists");
            return;
        }
        var oldFileList = await GetAppPublishInfo(oldAppVer.Id, platform);
        var newFileList = await GetAppPublishInfo(newAppVer.Id, platform);
        if (oldFileList is null || newFileList is null)
        {
            Console.WriteLine("AppPublishInfo not exists");
            return;
        }
        var oldAppVerFileList = JsonSerializer.Deserialize<AppPublishInfo>(oldFileList.FileListRaw);
        var newAppVerFileList = JsonSerializer.Deserialize<AppPublishInfo>(newFileList.FileListRaw);
        if (oldAppVerFileList is null || newAppVerFileList is null)
        {
            Console.WriteLine("AppPublishInfo not exists");
            return;
        }
        var pathSavePath = Path.Combine(patchFilePath, oldVer, $"{newVer}.dat");
        var pathFileInfo = new FileInfo(pathSavePath);
        if (!(pathFileInfo.Directory?.Exists ?? false))
        {
            pathFileInfo.Directory?.Create();
        }
        Console.WriteLine($"开始生成: {oldVer} -> {newVer} platform:{platform}");
        await ContrastVersion(oldAppVerFileList, newAppVerFileList, pathSavePath);

    }

    /// <summary>
    /// 获取版本号信息
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static async Task<AppVer?> GetAppVer(string ver)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{BaseUrl}/appver/{ver}");
        var data = JsonSerializer.Deserialize<ApiRsp<AppVer?>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return data?.Content;
    }

    /// <summary>
    /// 更具 APP 版本 ID 获取发布信息
    /// </summary>
    /// <param name="appVerId">版本号</param>
    /// <param name="platform">平台</param>
    /// <returns></returns>
    public static async Task<AppVerFileList?> GetAppPublishInfo(Guid appVerId, ClientPlatform platform)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{BaseUrl}/appver/file/{appVerId}/{platform}/list");
        var data = JsonSerializer.Deserialize<ApiRsp<AppVerFileList?>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return data?.Content;
    }

    /// <summary>
    /// 根据 2 个版本的文件列表生成补丁
    /// </summary>
    /// <param name="oldFileList"></param>
    /// <param name="newFileList"></param>
    /// <param name="patchFilePath"></param>
    /// <returns></returns>
    public static async ThreadTask ContrastVersion(AppPublishInfo oldFileList, AppPublishInfo newFileList, string patchFilePath)
    {
        var oldFiles = oldFileList.Files.Where(x => !x.RelativePath.StartsWith("dotnet\\"));
        var newFiles = newFileList.Files.Where(x => !x.RelativePath.StartsWith("dotnet\\"));
        Console.WriteLine($"旧版本文件数：{oldFiles.Count()}");
        Console.WriteLine($"新版本文件数：{newFiles.Count()}");
        var changeList = new Dictionary<string, PatchBundleInfo?>();

        var removeFiles = oldFiles.Select(x => x.RelativePath).Except(newFiles.Select(x => x.RelativePath));
        //文件删除
        foreach (var item in removeFiles)
        {
            changeList.Add(item, null);
        }
        Console.WriteLine($"需要删除的文件数：{changeList.Count}");
        foreach (var item in newFiles)
        {
            var oldFile = oldFiles.FirstOrDefault(x => x.RelativePath.Equals(item.RelativePath));
            //文件新增
            if (oldFile == null)
            {
                var newFileRaw = string.IsNullOrWhiteSpace(item.SignatureSHA384) ? await GetFileRaw(item.SHA384) : await GetFileRaw(item.SignatureSHA384);
                if (newFileRaw is null)
                {
                    Console.WriteLine("new file not exists");
                    return;
                }
                changeList.Add(item.RelativePath, new PatchBundleInfo
                {
                    SHA384 = item.SHA384,
                    IsFullOrPatch = true,
                    PatchData = newFileRaw.DataRaw
                });
            }
            else
            {
                //文件修改
                if (oldFile.SHA384 != item.SHA384)
                {
                    var oldFileRaw = string.IsNullOrWhiteSpace(oldFile.SignatureSHA384) ? await GetFileRaw(oldFile.SHA384) : await GetFileRaw(oldFile.SignatureSHA384);
                    if (oldFileRaw is null)
                    {
                        Console.WriteLine("old file not exists");
                        return;
                    }
                    var newFileRaw = string.IsNullOrWhiteSpace(item.SignatureSHA384) ? await GetFileRaw(item.SHA384) : await GetFileRaw(item.SignatureSHA384); // await GetFileRaw(item.SignatureSHA384);
                    if (newFileRaw is null)
                    {
                        Console.WriteLine("new file not exists");
                        return;
                    }
                    using (var output = new MemoryStream())
                    {
                        //BinaryPatch.Create(oldFileRaw.DataRaw, File.ReadAllBytes(item.RelativePath), output);
                        BinaryPatch.Create(oldFileRaw.DataRaw, newFileRaw.DataRaw, output);
                        changeList.Add(item.RelativePath, new PatchBundleInfo
                        {
                            SHA384 = item.SHA384,
                            IsFullOrPatch = false,
                            PatchData = output.ToArray()
                        });
                    }
                }
            }
        }
        if (File.Exists(patchFilePath))
            File.Delete(patchFilePath);

        Console.WriteLine($"变动文件数量：{changeList.Count}");
        using var fileStream = File.OpenWrite(patchFilePath);
        using var brSave = new BrotliStream(fileStream, CompressionLevel.SmallestSize);
        Console.WriteLine($"开始压缩");
        await MemoryPackSerializer.SerializeAsync(typeof(Dictionary<string, PatchBundleInfo?>), brSave, changeList);
        //fileStream.Flush();
        Console.WriteLine($"结束");
    }

    /// <summary>
    /// Hash 384 获取源文件数据
    /// </summary>
    /// <param name="sha384"></param>
    /// <returns></returns>
    public static async Task<AppVerFile?> GetFileRaw(string sha384)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{BaseUrl}/appver/file/{sha384}");
        var data = JsonSerializer.Deserialize<ApiRsp<AppVerFile?>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return data?.Content;
    }

    /// <summary>
    /// 上传指定文件到服务器
    /// </summary>
    /// <param name="directoryPath">需要上传的目录</param> 
    internal static async ThreadTask UploadFiles(string directoryPath)
    {
        DirectoryInfo directoryInfo = new(directoryPath);
        if (directoryInfo.Exists)
        {
            using var httpClient = new HttpClient();
            var fileList = new List<AppPublishFileInfo>();
            IScanPublicDirectoryCommand.ScanPathCore(directoryPath, fileList, null, null);
            using var form = new MultipartFormDataContent();
            fileList.ForEach(file =>
            {
                var fileStream = file.FileInfo!.OpenRead();
                form.Add(new StreamContent(fileStream), file.FileInfo!.Name, file.FileInfo!.Name);
            });
            if (form.Count() > 0)
            {
                var response = await httpClient.PostAsync($"{BaseUrl}/appver/file/upload", form);
                Console.WriteLine(response);
            }
        }
        else
        {
            Console.WriteLine("文件夹不存在");
        }
    }
}
