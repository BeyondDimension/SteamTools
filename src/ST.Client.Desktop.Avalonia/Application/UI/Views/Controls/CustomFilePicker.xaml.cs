using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using System.Collections;
using System.IO;
using static System.Application.FilePicker2;

namespace System.Application.UI.Views.Controls
{
    public class CustomFilePicker : ContentControl
    {
        public static readonly StyledProperty<string?> FileNameProperty = AvaloniaProperty.Register<CustomFilePicker, string?>(nameof(FileName), null);

        public static readonly StyledProperty<Stream?> FileStreamProperty = AvaloniaProperty.Register<CustomFilePicker, Stream?>(nameof(FileStream), null);

        public static readonly StyledProperty<string?> FileExtensionsProperty = AvaloniaProperty.Register<CustomFilePicker, string?>(nameof(FileExtensions), "*");

        public Stream? FileStream
        {
            get => GetValue(FileStreamProperty);
            set => SetValue(FileStreamProperty, value);
        }

        public string? FileName
        {
            get => GetValue(FileNameProperty);
            set
            {
                FileStream = IOPath.OpenRead(FileName);
                SetValue(FileNameProperty, value);
            }
        }

        public string? FileExtensions
        {
            get => GetValue(FileExtensionsProperty);
            set => SetValue(FileExtensionsProperty, value);
        }

        public CustomFilePicker()
        {
            #region 启用拖拽文件的效果

            //Windows管理员权限运行无法在低权限的资源管理中拖拽文件到高权限的程序上
            //https://docs.microsoft.com/zh-cn/archive/blogs/patricka/q-why-doesnt-drag-and-drop-work-when-my-application-is-running-elevated-a-mandatory-integrity-control-and-uipi?tdsourcetag=s_pctim_aiomsg

            void DragOver(object? sender, DragEventArgs e)
            {
                // Only allow Copy or Link as Drop Operations.
                e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

                // Only allow if the dragged data contains text or filenames.
                if (!e.Data.Contains(DataFormats.Text) &&
                    !e.Data.Contains(DataFormats.FileNames))
                    e.DragEffects = DragDropEffects.None;
            }

            void Drop(object? sender, DragEventArgs e)
            {
                if (e.Data.Contains(DataFormats.Text))
                    FileName = e.Data.GetText();
                else if (e.Data.Contains(DataFormats.FileNames))
                    FileName = string.Join(Environment.NewLine, e.Data.GetFileNames());
            }

            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
            #endregion

            this.Tapped += CustomFilePicker_Tapped;
        }

        private async void CustomFilePicker_Tapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FilePickerFileType fileTypes = new ValueTuple<string, string[]>[] { ("Image Files", FileEx.ImageFileExtensions) };

            //if (string.IsNullOrEmpty(FileExtensions) || FileExtensions == "*")
            //{
            //    fileTypes = new ValueTuple<string, string[]>[] { ("All Files", new[] { "*" }) };
            //}
            //else
            //{
            //    fileTypes = new ValueTuple<string, string[]>[] {
            //            ("Required Files", FileExtensions.Split(",")),
            //            ("All Files", new[] { "*" }),
            //           };
            //}

            await PickAsync((file) =>
            {
                FileName = file;
            }, fileTypes);
        }
    }
}
