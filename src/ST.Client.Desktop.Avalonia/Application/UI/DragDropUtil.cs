using Avalonia.Input;
using System.Collections.Generic;

namespace System.Application.UI
{
    /// <summary>
    /// 拖拽功能工具类
    /// https://github.com/AvaloniaUI/Avalonia/blob/0.10.0-preview6/samples/ControlCatalog/Pages/DragAndDropPage.xaml.cs
    /// </summary>
    public static class DragDropUtil
    {
        public static void InitDragDrop(this IOnDragDropListener listener,
           InputElement element)
        {
            element.PointerPressed += async (_, e)
                => await DragDrop.DoDragDrop(e, new DataObject(), DragDropEffects.Copy);
            element.AddHandler(DragDrop.DropEvent, Drop);
            element.AddHandler(DragDrop.DragOverEvent, DragOver);

            void DragOver(object? s, DragEventArgs? e)
            {
                if (e?.Data == null) return;
                listener.DragOver(s is InputElement el ? el : element, e);
            }

            void Drop(object? s, DragEventArgs? e)
            {
                if (e?.Data == null) return;
                listener.Drop(s is InputElement el ? el : element, e);
            }
        }

        public static void InitDragDrop(this IOnDragDropListener listener,
            IEnumerable<InputElement> elements)
        {
            foreach (var element in elements)
            {
                listener.InitDragDrop(element);
            }
        }

        public static void InitDragDrop(this IOnDragDropListener listener,
            params InputElement[] elements)
        {
            IEnumerable<InputElement> elements_ = elements;
            listener.InitDragDrop(elements_);
        }

        public interface IOnDragDropListener
        {
            void DragOver(InputElement sender, DragEventArgs e)
            {
                // Only allow Copy or Link as Drop Operations.
                e.DragEffects &= (DragDropEffects.Copy | DragDropEffects.Link);

                // Only allow if the dragged data contains text or filenames.
                if (!e.Data.Contains(DataFormats.Text)
                    && !e.Data.Contains(DataFormats.FileNames))
                    e.DragEffects = DragDropEffects.None;
            }

            void Drop(InputElement sender, DragEventArgs e)
            {
                if (e.Data.Contains(DataFormats.Text))
                {
                    var text = e.Data.GetText();
                    if (text != null)
                    {
                        DropText(sender, text);
                    }
                }
                else if (e.Data.Contains(DataFormats.FileNames))
                {
                    var fileNames = e.Data.GetFileNames();
                    if (fileNames != null)
                    {
                        DropFileNames(sender, fileNames);
                    }
                }
            }

            /// <summary>
            /// 拖拽文本
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="text"></param>
            void DropText(InputElement sender, string text);

            /// <summary>
            /// 拖拽路径，路径可能是文件也可能是文件夹
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="fileNames"></param>
            void DropFileNames(InputElement sender, IEnumerable<string> fileNames);
        }
    }
}