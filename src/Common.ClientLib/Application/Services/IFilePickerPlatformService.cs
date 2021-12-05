using System.Collections.Generic;
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
            protected static void FormatExtensions(IEnumerable<string>? extensions, Action<string> add, bool trimLeadingPeriod = false)
            {
                var hasAtLeastOneType = false;

                if (extensions != null)
                {
                    foreach (var extension in extensions)
                    {
                        var ext = FileEx.Clean(extension, trimLeadingPeriod);
                        if (!string.IsNullOrWhiteSpace(ext))
                        {
                            add(ext);
                            hasAtLeastOneType = true;
                        }
                    }
                }

                if (!hasAtLeastOneType)
                    add("*");
            }

            protected static List<string> FormatExtensions(IEnumerable<string>? extensions, bool trimLeadingPeriod = false)
            {
                var list = new List<string>();
                FormatExtensions(extensions, list.Add, trimLeadingPeriod);
                return list;
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

            Task<FileResult?> PlatformSaveAsync(SaveOptions? options);
        }
    }
}