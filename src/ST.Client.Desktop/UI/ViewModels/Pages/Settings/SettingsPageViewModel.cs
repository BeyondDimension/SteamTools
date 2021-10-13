using DynamicData.Binding;
using ReactiveUI;
using System.Application.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.IO.FileFormats;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using static System.Application.FilePicker2;

namespace System.Application.UI.ViewModels
{
    partial class SettingsPageViewModel
    {
        KeyValuePair<string, string> _SelectFont;
        public KeyValuePair<string, string> SelectFont
        {
            get => _SelectFont;
            set => this.RaiseAndSetIfChanged(ref _SelectFont, value);
        }

        public ICommand SelectImage_Click { get; }

        public ICommand ResetImage_Click { get; }

        public SettingsPageViewModel()
        {
            SelectFont = R.Fonts.FirstOrDefault(x => x.Value == UISettings.FontName.Value);
            this.WhenValueChanged(x => x.SelectFont, false)
                  .Subscribe(x => UISettings.FontName.Value = x.Value);

            SelectImage_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = !IsSupportedFileExtensionFilter ? (FilePickerFileType?)null : new FilePickerFilter(new (string, IEnumerable<string>)[] {
                    ("Image Files", new[] {
                            FileEx.BMP,
                            FileEx.JPG,
                            FileEx.JPEG,
                            FileEx.PNG,
                            FileEx.GIF,
                            FileEx.WEBP,
                        }),
                    ("All Files", new[] { "*" }),
                });
                await PickAsync(SetBackgroundImagePath, fileTypes);
            });

            ResetImage_Click = ReactiveCommand.Create(() => SetBackgroundImagePath(null));
        }

        const double clickInterval = 3d;
        readonly Dictionary<string, DateTime> clickTimeRecord = new();
        public void OpenFolder(string tag)
        {
            var path = tag switch
            {
                IOPath.DirName_AppData => IOPath.AppDataDirectory,
                IOPath.DirName_Cache => IOPath.CacheDirectory,
                IApplication.LogDirName => IApplication.LogDirPath,
                _ => null,
            };
            if (path != null)
            {
                var hasKey = clickTimeRecord.TryGetValue(path, out var dt);
                var now = DateTime.Now;
                if (hasKey && (now - dt).TotalSeconds <= clickInterval) return;
                IDesktopPlatformService.Instance.OpenFolder(path);
                if (!clickTimeRecord.TryAdd(path, now)) clickTimeRecord[path] = now;
            }
        }

        public void SetBackgroundImagePath(string? imagePath)
        {
            if (imagePath == null)
            {
                UISettings.BackgroundImagePath.Reset();
                return;
            }
            if (File.Exists(imagePath))
            {
                if (IOPath.TryOpenRead(imagePath, out var stream, out var _))
                {
                    var image = FileFormat.IsImage(stream);
                    if (image.isImage)
                    {
                        UISettings.BackgroundImagePath.Value = imagePath;
                        return;
                    }
                }
            }
            Toast.Show("选择的图片格式不正确");
        }
    }
}