using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xamarin.Essentials;
using BaseFilePickerFileType = Xamarin.Essentials.FilePickerFileType;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    partial class FilePicker2
    {
        public class SaveOptions : PickOptions
        {
            public string? InitialFileName { get; set; }
        }

        public class FilePickerFileType : BaseFilePickerFileType
        {
            sealed class FixedFilePickerFileType : FilePickerFileType
            {
                readonly IEnumerable<string> fileTypes;

                public FixedFilePickerFileType(IEnumerable<string> fileTypes)
                {
                    this.fileTypes = fileTypes;
                }

                protected override IEnumerable<string> GetPlatformFileType(DevicePlatform _) => fileTypes;
            }

            [return: NotNullIfNotNull("values")]
            public static FilePickerFileType? Parse(IEnumerable<string>? values) => values == null ? null : new FixedFilePickerFileType(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(HashSet<string>? values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(string[]? values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(Collection<string>? values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(List<string>? values) => Parse(values);

            sealed class PlatformFilePickerFileType<TValue> : FilePickerFileType where TValue : IEnumerable<string>
            {
                readonly IEnumerable<KeyValuePair<Platform, TValue?>> fileTypes;

                public PlatformFilePickerFileType(IEnumerable<KeyValuePair<Platform, TValue?>> fileTypes)
                {
                    this.fileTypes = fileTypes;
                }

                protected override IEnumerable<string>? GetPlatformFileType(DevicePlatform _)
                {
                    var platform = DeviceInfo2.Platform;
                    foreach (var fileType in fileTypes)
                    {
                        if (fileType.Key.HasFlag(platform))
                            return fileType.Value;
                    }
                    return null;
                }
            }

            [return: NotNullIfNotNull("values")]
            public static FilePickerFileType? Parse<TValue>(IEnumerable<KeyValuePair<Platform, TValue?>> values) where TValue : IEnumerable<string> => values == null ? null : new PlatformFilePickerFileType<TValue>(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(Dictionary<Platform, IEnumerable<string>?> values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(Dictionary<Platform, string[]?> values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(Dictionary<Platform, List<string>?> values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(Dictionary<Platform, HashSet<string>?> values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?(Dictionary<Platform, Collection<string>?> values) => Parse(values);

            public interface IFilePickerFileTypeWithName
            {
                IEnumerable<(string, IEnumerable<string>)> GetFileTypes();
            }

            sealed class FixedFilePickerFileType<TValue> : FilePickerFileType, IFilePickerFileTypeWithName where TValue : IEnumerable<string>
            {
                readonly IEnumerable<(string name, TValue extensions)> fileTypes;

                public FixedFilePickerFileType(IEnumerable<(string name, TValue extensions)> fileTypes)
                {
                    this.fileTypes = fileTypes;
                }

                protected override IEnumerable<string> GetPlatformFileType(DevicePlatform _) => fileTypes.SelectMany(x => x.extensions);

                IEnumerable<(string, IEnumerable<string>)> IFilePickerFileTypeWithName.GetFileTypes()
                {
                    foreach (var item in fileTypes)
                    {
                        yield return item;
                    }
                }
            }

            [return: NotNullIfNotNull("values")]
            public static FilePickerFileType? Parse<TValue>(IEnumerable<(string name, TValue extensions)> values) where TValue : IEnumerable<string> => values == null ? null : new FixedFilePickerFileType<TValue>(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?((string name, IEnumerable<string> extensions)[] values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?((string name, List<string> extensions)[] values) => Parse(values);

            [return: NotNullIfNotNull("values")]
            public static implicit operator FilePickerFileType?((string name, string[] extensions)[] values) => Parse(values);
        }
    }
}
