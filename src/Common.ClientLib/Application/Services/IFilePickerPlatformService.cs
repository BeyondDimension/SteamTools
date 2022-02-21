using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static System.Application.FilePicker2;

namespace System.Application.Services
{
    public interface IFilePickerPlatformService
    {
        public IOpenFileDialogService OpenFileDialogService { get; }

        public ISaveFileDialogService SaveFileDialogService { get; }

        public interface IServiceBase
        {
            protected static IEnumerable<string> FormatExtensions(IEnumerable<string>? extensions, bool trimLeadingPeriod = false)
            {
                var hasAtLeastOneType = false;

                if (extensions != null)
                {
                    foreach (var extension in extensions)
                    {
                        var ext = Clean(extension, trimLeadingPeriod);
                        static string Clean(string extension, bool trimLeadingPeriod = false)
                        {
                            if (string.IsNullOrWhiteSpace(extension))
                                return string.Empty;

                            if (extension.StartsWith('.'))
                            {
                                if (!trimLeadingPeriod) return extension;
                                return extension.TrimStart('.');
                            }
                            else
                            {
                                if (!trimLeadingPeriod) return "." + extension;
                                return extension;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(ext))
                        {
                            yield return ext;
                            hasAtLeastOneType = true;
                        }
                    }
                }

                if (!hasAtLeastOneType)
                    yield return "*";
            }
        }

        public interface IOpenFileDialogService : IServiceBase
        {
            static IOpenFileDialogService? Instance => DI.Get_Nullable<IOpenFileDialogService>();

            Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions? options, bool allowMultiple = false);

            bool IsSupportedFileExtensionFilter { get; }
        }

        public interface ISaveFileDialogService : IServiceBase
        {
            static ISaveFileDialogService? Instance => DI.Get_Nullable<ISaveFileDialogService>();

            Task<SaveFileResult?> PlatformSaveAsync(SaveOptions? options);
        }
    }
}