using System.Linq;
using Avalonia.Win32.JumpLists;
using WinUI = Windows.UI.StartScreen;
using AvaloniaApplication = Avalonia.Application;
using static System.Application.Services.IJumpListService;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IJumpListService"/>
    internal sealed class JumpListServiceImpl : IJumpListService
    {
        public Task AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
        {
            try
            {
                JumpList jumpList1 = JumpList.GetJumpList(AvaloniaApplication.Current);
                if (jumpList1 == null)
                {
                    jumpList1 = new JumpList
                    {
                        ShowRecentCategory = true,
                        ShowFrequentCategory = false,
                    };
                    JumpList.SetJumpList(AvaloniaApplication.Current, jumpList1);
                }

                //bool isRecent = false; // 将指定的项路径添加到跳转列表的“最近”类别中。
                foreach (var (title, applicationPath, iconResourcePath, arguments, description, customCategory) in items)
                {
                    var task = new JumpTask
                    {
                        // Get the path to Calculator and set the JumpTask properties.
                        ApplicationPath = applicationPath,
                        IconResourcePath = iconResourcePath,
                        Arguments = arguments,
                        Title = title,
                        Description = description,
                        CustomCategory = customCategory,
                    };
                    var tk = jumpList1.JumpItems.FirstOrDefault(s
                             => s is JumpTask t &&
                                 t.ApplicationPath == task.ApplicationPath &&
                                 t.Arguments == task.Arguments);
                    if (tk != null)
                    {
                        jumpList1.JumpItems.Remove(tk);
                    }
                    jumpList1.JumpItems.Add(task);
                    //if (isRecent)
                    //    JumpList.AddToRecentCategory(task);
                }

                jumpList1.Apply();
            }
            catch (Exception ex)
            {
                const string log_msg = $"{nameof(AddJumpItemsAsync)} catch.";
                Log.Error(TAG, ex, log_msg);
            }

            return Task.CompletedTask;
        }
    }

    ///// <inheritdoc cref="IJumpListService"/>
    //[SupportedOSPlatform("Windows10.0.10586.0")]
    //internal sealed class Windows10JumpListServiceImpl : IJumpListService
    //{
    //    public static bool IsSupported
    //    {
    //        get
    //        {
    //            if (OperatingSystem2.IsWindowsVersionAtLeast(10, 0, 10586))
    //            {
    //                return WinUI.JumpList.IsSupported();
    //            }
    //            return false;
    //        }
    //    }

    //    public async Task AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    //    {
    //        var jumpList1 = await WinUI.JumpList.LoadCurrentAsync();

    //        foreach (var (title, _, iconResourcePath, arguments, description, customCategory) in items)
    //        {
    //            var item = WinUI.JumpListItem.CreateWithArguments(arguments, title);
    //            item.Description = description;
    //            item.GroupName = customCategory;
    //            item.Logo = new Uri(iconResourcePath);

    //            jumpList1.Items.Add(item);
    //        }

    //        await jumpList1.SaveAsync();
    //    }
    //}
}