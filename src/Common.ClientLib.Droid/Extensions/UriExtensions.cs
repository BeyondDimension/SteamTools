using Android;
using Android.OS;
using Android.Provider;
using Android.Webkit;
using System.IO;
using Xamarin.Essentials;
using AndroidUri = Android.Net.Uri;
using XEPlatform = Xamarin.Essentials.Platform;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class UriExtensions
    {
        // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FileSystem/FileSystem.android.cs

        const string storageTypePrimary = "primary";
        const string storageTypeRaw = "raw";
        const string storageTypeImage = "image";
        const string storageTypeVideo = "video";
        const string storageTypeAudio = "audio";
        static readonly string[] contentUriPrefixes =
        {
            "content://downloads/public_downloads",
            "content://downloads/my_downloads",
            "content://downloads/all_downloads",
        };

        internal const string UriSchemeFile = "file";
        internal const string UriSchemeContent = "content";

        internal const string UriAuthorityExternalStorage = "com.android.externalstorage.documents";
        internal const string UriAuthorityDownloads = "com.android.providers.downloads.documents";
        internal const string UriAuthorityMedia = "com.android.providers.media.documents";

        public static string? EnsurePhysicalPath(this AndroidUri? uri, bool requireExtendedAccess = true)
        {
            if (uri != null)
            {
                // if this is a file, use that
                if (uri.Scheme!.Equals(UriSchemeFile, StringComparison.OrdinalIgnoreCase))
                    return uri.Path;

                // try resolve using the content provider
                var absolute = ResolvePhysicalPath(uri, requireExtendedAccess);
                if (!string.IsNullOrWhiteSpace(absolute) && Path.IsPathRooted(absolute))
                    return absolute;

                // fall back to just copying it
                var cached = CacheContentFile(uri);
                if (!string.IsNullOrWhiteSpace(cached) && Path.IsPathRooted(cached))
                    return cached;
            }

            return null;
        }

        static string? ResolvePhysicalPath(AndroidUri uri, bool requireExtendedAccess = true)
        {
            if (uri.Scheme!.Equals(UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                // if it is a file, then return directly

                var resolved = uri.Path;
                if (File.Exists(resolved))
                    return resolved;
            }
            else if (!requireExtendedAccess || !((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.Q))
            {
                // if this is on an older OS version, or we just need it now

                if (DocumentsContract.IsDocumentUri(XEPlatform.AppContext, uri))
                {
                    var resolved = ResolveDocumentPath(uri);
                    if (File.Exists(resolved))
                        return resolved;
                }
                else if (uri.Scheme!.Equals(UriSchemeContent, StringComparison.OrdinalIgnoreCase))
                {
                    var resolved = ResolveContentPath(uri);
                    if (File.Exists(resolved))
                        return resolved;
                }
            }

            return null;
        }

        static string? ResolveDocumentPath(AndroidUri uri)
        {
            var docId = DocumentsContract.GetDocumentId(uri);

            var docIdParts = docId?.Split(':');
            if (docIdParts == null || docIdParts.Length == 0)
                return null;

            if (uri.Authority!.Equals(UriAuthorityExternalStorage, StringComparison.OrdinalIgnoreCase))
            {
                if (docIdParts.Length == 2)
                {
                    var storageType = docIdParts[0];
                    var uriPath = docIdParts[1];

                    // This is the internal "external" memory, NOT the SD Card
                    if (storageType.Equals(storageTypePrimary, StringComparison.OrdinalIgnoreCase))
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        var root = Android.OS.Environment.ExternalStorageDirectory!.Path;
#pragma warning restore CS0618 // Type or member is obsolete

                        return Path.Combine(root, uriPath);
                    }

                    // TODO: support other types, such as actual SD Cards
                }
            }
            else if (uri.Authority.Equals(UriAuthorityDownloads, StringComparison.OrdinalIgnoreCase))
            {
                // NOTE: This only really applies to older Android vesions since the privacy changes

                if (docIdParts.Length == 2)
                {
                    var storageType = docIdParts[0];
                    var uriPath = docIdParts[1];

                    if (storageType.Equals(storageTypeRaw, StringComparison.OrdinalIgnoreCase))
                        return uriPath;
                }

                // ID could be "###" or "msf:###"
                var fileId = docIdParts.Length == 2
                    ? docIdParts[1]
                    : docIdParts[0];

                foreach (var prefix in contentUriPrefixes)
                {
                    var uriString = prefix + "/" + fileId;
                    var contentUri = AndroidUri.Parse(uriString);

                    if (GetDataFilePath(contentUri!) is string filePath)
                        return filePath;
                }
            }
            else if (uri.Authority.Equals(UriAuthorityMedia, StringComparison.OrdinalIgnoreCase))
            {
                if (docIdParts.Length == 2)
                {
                    var storageType = docIdParts[0];
                    var uriPath = docIdParts[1];

                    AndroidUri? contentUri = null;
                    if (storageType.Equals(storageTypeImage, StringComparison.OrdinalIgnoreCase))
                        contentUri = MediaStore.Images.Media.ExternalContentUri!;
                    else if (storageType.Equals(storageTypeVideo, StringComparison.OrdinalIgnoreCase))
                        contentUri = MediaStore.Video.Media.ExternalContentUri!;
                    else if (storageType.Equals(storageTypeAudio, StringComparison.OrdinalIgnoreCase))
                        contentUri = MediaStore.Audio.Media.ExternalContentUri!;

#pragma warning disable CS0618 // 类型或成员已过时
                    if (contentUri != null && GetDataFilePath(contentUri, $"{MediaStore.MediaColumns.Id!}=?", new[] { uriPath }) is string filePath)
#pragma warning restore CS0618 // 类型或成员已过时
                        return filePath;
                }
            }

            return null;
        }

        static string? ResolveContentPath(AndroidUri uri)
        {
            if (GetDataFilePath(uri) is string filePath)
                return filePath;

            // TODO: support some additional things, like Google Photos if that is possible

            return null;
        }

        static string? GetDataFilePath(AndroidUri contentUri, string? selection = null, string[]? selectionArgs = null)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            const string column = MediaStore.Files.FileColumns.Data;
#pragma warning restore CS0618 // Type or member is obsolete

            // ask the content provider for the data column, which may contain the actual file path
            var path = GetColumnValue(contentUri, column, selection, selectionArgs);
            if (!string.IsNullOrEmpty(path) && Path.IsPathRooted(path))
                return path;

            return null;
        }

        static string? GetColumnValue(AndroidUri contentUri, string column, string? selection = null, string[]? selectionArgs = null)
        {
            try
            {
                var value = QueryContentResolverColumn(contentUri, column, selection, selectionArgs);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            catch
            {
                // Ignore all exceptions and use null for the error indicator
            }

            return null;
        }

        static string? QueryContentResolverColumn(AndroidUri contentUri, string columnName, string? selection = null, string[]? selectionArgs = null)
        {
            string? text = null;

            var projection = new[] { columnName };
            using var cursor = XEPlatform.AppContext.ContentResolver!.Query(contentUri, projection, selection, selectionArgs, null);
            if (cursor?.MoveToFirst() == true)
            {
                var columnIndex = cursor.GetColumnIndex(columnName);
                if (columnIndex != -1)
                    text = cursor.GetString(columnIndex);
            }

            return text;
        }

        static string? CacheContentFile(AndroidUri uri)
        {
            if (!uri.Scheme!.Equals(UriSchemeContent, StringComparison.OrdinalIgnoreCase))
                return null;

            // open the source stream
            using var srcStream = OpenContentStream(uri, out var extension);
            if (srcStream == null)
                return null;

            // resolve or generate a valid destination path
#pragma warning disable CS0618 // 类型或成员已过时
            var filename = GetColumnValue(uri, MediaStore.Files.FileColumns.DisplayName) ?? Guid.NewGuid().ToString("N");
#pragma warning restore CS0618 // 类型或成员已过时
            if (!Path.HasExtension(filename) && !string.IsNullOrEmpty(extension))
                filename = Path.ChangeExtension(filename, extension);

            // create a temporary file
            var hasPermission = Permissions.IsDeclaredInManifest(Manifest.Permission.WriteExternalStorage);
            var root = hasPermission
                ? XEPlatform.AppContext.ExternalCacheDir
                : XEPlatform.AppContext.CacheDir;
            var tmpFile = GetEssentialsTemporaryFile(root!, filename);

            // copy to the destination
            using var dstStream = File.Create(tmpFile.CanonicalPath);
            srcStream.CopyTo(dstStream);

            return tmpFile.CanonicalPath;
        }

        static Stream? OpenContentStream(AndroidUri uri, out string? extension)
        {
            var isVirtual = IsVirtualFile(uri);
            if (isVirtual)
            {
                return GetVirtualFileStream(uri, out extension);
            }

            extension = GetFileExtension(uri);
            return XEPlatform.AppContext.ContentResolver!.OpenInputStream(uri);
        }

        static string? GetFileExtension(AndroidUri uri)
        {
            var mimeType = XEPlatform.AppContext.ContentResolver!.GetType(uri);

            return mimeType != null
                ? MimeTypeMap.Singleton!.GetExtensionFromMimeType(mimeType)
                : null;
        }

        static bool IsVirtualFile(AndroidUri uri)
        {
            if (!DocumentsContract.IsDocumentUri(XEPlatform.AppContext, uri))
                return false;

            var value = GetColumnValue(uri, DocumentsContract.Document.ColumnFlags);
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var flagsInt))
            {
                var flags = (DocumentContractFlags)flagsInt;
                return flags.HasFlag(DocumentContractFlags.VirtualDocument);
            }

            return false;
        }

        static Stream? GetVirtualFileStream(AndroidUri uri, out string? extension)
        {
            var mimeTypes = XEPlatform.AppContext.ContentResolver!.GetStreamTypes(uri, MediaTypeNames.All);
            if (mimeTypes?.Length >= 1)
            {
                var mimeType = mimeTypes[0];

                var stream = XEPlatform.AppContext.ContentResolver!
                    .OpenTypedAssetFileDescriptor(uri, mimeType, null)!
                    .CreateInputStream();

                extension = MimeTypeMap.Singleton!.GetExtensionFromMimeType(mimeType);

                return stream;
            }

            extension = null;
            return null;
        }

        internal static Java.IO.File GetEssentialsTemporaryFile(Java.IO.File root, string fileName)
        {
            // create the directory for all Essentials files
            var rootDir = new Java.IO.File(root, EssentialsFolderHash);
            rootDir.Mkdirs();
            rootDir.DeleteOnExit();

            // create a unique directory just in case there are multiple file with the same name
            var tmpDir = new Java.IO.File(rootDir, Guid.NewGuid().ToString("N"));
            tmpDir.Mkdirs();
            tmpDir.DeleteOnExit();

            // create the new temporary file
            var tmpFile = new Java.IO.File(tmpDir, fileName);
            tmpFile.DeleteOnExit();

            return tmpFile;
        }

        internal const string EssentialsFolderHash = "2203693cc04e0be7f4f024d5f9499e13";
    }
}