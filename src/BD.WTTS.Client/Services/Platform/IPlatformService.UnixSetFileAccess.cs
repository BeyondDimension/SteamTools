// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    const UnixFileMode Combined755 = UnixFileMode.UserRead | UnixFileMode.UserWrite |
        UnixFileMode.UserExecute | UnixFileMode.GroupRead |
        UnixFileMode.GroupExecute | UnixFileMode.OtherRead |
        UnixFileMode.OtherExecute;

    const UnixFileMode Combined777 = UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.UserExecute | UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherWrite |
            UnixFileMode.OtherExecute;

#if DEBUG
    [Obsolete("use System.IO.UnixFileMode")]
    [Flags]
    enum UnixPermission : ushort
    {
        OtherExecute = 0x1,
        OtherWrite = 0x2,
        OtherRead = 0x4,
        GroupExecute = 0x8,
        GroupWrite = 0x10,
        GroupRead = 0x20,
        UserExecute = 0x40,
        UserWrite = 0x80,
        UserRead = 0x100,
        [Obsolete("use IPlatformService.Combined755")]
        Combined755 = UserRead | UserWrite |
            UserExecute | GroupRead |
            GroupExecute | OtherRead |
            OtherExecute,
        [Obsolete("use IPlatformService.Combined777")]
        Combined777 = UserRead | UserWrite |
            UserExecute | GroupRead |
            GroupWrite | GroupExecute |
            OtherRead | OtherWrite |
            OtherExecute,
    }

    [Obsolete("use UnauthorizedAccessException/DirectoryNotFoundException/FileNotFoundException", true)]
    enum UnixSetFileAccessResult
    {
        PathIsNullOrEmpty = -3,
        PathInvalid,
        Failure,
        Success,
    }

    [Obsolete("use System.IO.File.SetUnixFileMode", true)]
    UnixSetFileAccessResult UnixSetFileAccess(string? path, UnixPermission permission)
    {
        throw new NotImplementedException();
    }
#endif
}