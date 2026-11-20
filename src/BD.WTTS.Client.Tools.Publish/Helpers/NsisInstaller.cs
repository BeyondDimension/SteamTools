namespace BD.WTTS.Client.Tools.Publish.Helpers;

sealed class NsisInstaller
{
    readonly string appFiles;
    readonly string compiler;
    readonly string template;

    internal NsisInstaller(string buildDirectory, string? makensis = null)
    {
        buildDirectory = Path.GetFullPath(buildDirectory);
        appFiles = Path.Combine(buildDirectory, "AppCode", "Steampp");
        var project = Path.Combine(appFiles, "app", "SteamPP_setup.nsi");
        if (!File.Exists(project))
            throw new FileNotFoundException(string.Format(ToolStrings.Get("MissingNsisBuild"), project), project);

        compiler = makensis is null ? Path.Combine(buildDirectory, "NSIS", "makensis.exe") : Path.GetFullPath(makensis);
        if (!File.Exists(compiler))
            throw new FileNotFoundException("The NSIS-Build compiler was not found.", compiler);
        template = File.ReadAllText(project);
    }

    internal void Build(string archive, string output, string version)
    {
        var script = template
            .Replace("${{ Steam++_Company }}", AssemblyInfo.Company)
            .Replace("${{ Steam++_Copyright }}", AssemblyInfo.Copyright)
            .Replace("${{ Steam++_ProductName }}", AssemblyInfo.Trademark)
            .Replace("${{ Steam++_ExeName }}", "Steam++.exe")
            .Replace("${{ Steam++_Version }}", version)
            .Replace("${{ Steam++_OutPutFileName }}", Path.GetFileName(output))
            .Replace("${{ Steam++_AppFileDir }}", appFiles)
            .Replace("${{ Steam++_7zFilePath }}", archive)
            .Replace("${{ Steam++_7zFileName }}", Path.GetFileName(archive))
            .Replace("${{ Steam++_OutPutFilePath }}", output)
            .Replace("${{ Steam++_UninstFileName }}", Path.Combine(appFiles, "app", "uninst.exe"));

        var temporary = Directory.CreateTempSubdirectory("wtts-nsis-");
        try
        {
            // Keep the checkout read-only and resolve its original skin, icons and includes in place.
            var scriptPath = Path.Combine(temporary.FullName, "SteamPP_setup.nsi");
            File.WriteAllText(scriptPath, script);
            var start = new ProcessStartInfo(compiler)
            {
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(appFiles, "app"),
            };
            start.ArgumentList.Add("/NOCD");
            start.ArgumentList.Add("/DINSTALL_WITH_NO_NSIS7Z=1");
            start.ArgumentList.Add(scriptPath);
            ProcessHelper.StartAndWaitForExit(start);
        }
        finally
        {
            temporary.Delete(recursive: true);
        }
    }
}
