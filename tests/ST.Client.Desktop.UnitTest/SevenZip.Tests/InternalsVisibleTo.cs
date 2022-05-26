#if WINDOWS_NT
using System.Reflection;

namespace SevenZip;

static class SevenZipLibraryManager
{
    static readonly Type Type = Type.GetType("SevenZip.SevenZipLibraryManager, SevenZipSharp")!;

    public static void SetLibraryPath(string libraryPath)
    {
        try
        {
            Type.GetMethod(nameof(SetLibraryPath),
                BindingFlags.Static | BindingFlags.Public)
                    !.Invoke(null, new[] { libraryPath });
        }
        catch (TargetInvocationException e)
        {
            if (e.InnerException != null)
            {
                throw e.InnerException;
            }
            else
            {
                throw;
            }
        }
    }

    public static LibraryFeature CurrentLibraryFeatures
    {
        get
        {
            return (LibraryFeature)Type.GetProperty(
                nameof(CurrentLibraryFeatures),
                BindingFlags.Static | BindingFlags.Public)
                    !.GetValue(null)!;
        }
    }
}

static class FileChecker
{
    static readonly Type Type = Type.GetType("SevenZip.FileChecker, SevenZipSharp")!;

    public static InArchiveFormat CheckSignature(Stream stream, out int offset, out bool isExecutable)
    {
        var args = new object?[] { stream, null, null };
        var result = (InArchiveFormat)Type.GetMethod(nameof(CheckSignature),
             BindingFlags.Static | BindingFlags.Public,
             new[] { typeof(Stream), typeof(int).MakeByRefType(), typeof(bool).MakeByRefType() })
                 !.Invoke(null, args)!;
        offset = (int)args[1]!;
        isExecutable = (bool)args[2]!;
        return result;
    }

    public static InArchiveFormat CheckSignature(string fileName, out int offset, out bool isExecutable)
    {
        var args = new object?[] { fileName, null, null };
        var result = (InArchiveFormat)Type.GetMethod(nameof(CheckSignature),
             BindingFlags.Static | BindingFlags.Public,
             new[] { typeof(string), typeof(int).MakeByRefType(), typeof(bool).MakeByRefType() })
                 !.Invoke(null, args)!;
        offset = (int)args[1]!;
        isExecutable = (bool)args[2]!;
        return result;
    }
}
#endif