using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using System.Collections;

namespace System.Application.UI.Views.Controls
{
    public class CustomFilePicker : ContentControl
    {


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
                //if (e.Data.Contains(DataFormats.Text))
                //    file = e.Data.GetText();
                //else if (e.Data.Contains(DataFormats.FileNames))
                //    file = string.Join(Environment.NewLine, e.Data.GetFileNames());
            }

            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
            #endregion
        }

    }
}
