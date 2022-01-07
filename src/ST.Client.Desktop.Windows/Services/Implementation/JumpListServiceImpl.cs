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
                var jumpList1 = JumpList.GetJumpList(AvaloniaApplication.Current);
                if (jumpList1 == null)
                {
                    jumpList1 = new JumpList
                    {
                        ShowRecentCategory = true,
                        ShowFrequentCategory = false,
                    };
                    JumpList.SetJumpList(AvaloniaApplication.Current, jumpList1);
                }

                foreach (var (title, applicationPath, iconResourcePath, arguments, description, customCategory) in items)
                {
                    var task = (from s in jumpList1.JumpItems
                                let v = s is JumpTask t ? t : null
                                where v != null &&
                                  v.ApplicationPath == applicationPath &&
                                  v.Arguments == arguments
                                select v).FirstOrDefault();
                    if (task != null)
                    {
                        task.Title = title;
                        task.IconResourcePath = iconResourcePath;
                        task.Description = description;
                        task.CustomCategory = customCategory;
                    }
                    else
                    {
                        task = new JumpTask
                        {
                            // Get the path to Calculator and set the JumpTask properties.
                            ApplicationPath = applicationPath,
                            IconResourcePath = iconResourcePath,
                            Arguments = arguments,
                            Title = title,
                            Description = description,
                            CustomCategory = customCategory,
                        };
                        jumpList1.JumpItems.Add(task);
                    }
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

    /// <inheritdoc cref="IJumpListService"/>
    [SupportedOSPlatform("Windows10.0.10586.0")]
    internal sealed class Windows10JumpListServiceImpl : IJumpListService
    {
        public static bool IsSupported
        {
            get
            {
                if (OperatingSystem2.IsWindowsVersionAtLeast(10, 0, 10586))
                {
                    return WinUI.JumpList.IsSupported();
                }
                return false;
            }
        }

        public async Task AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
        {
            try
            {
                var jumpList1 = await WinUI.JumpList.LoadCurrentAsync();

                foreach (var (title, _, _, arguments, description, customCategory) in items)
                {
                    var isAdd = false;
                    var item = jumpList1.Items.FirstOrDefault(x => x.Kind == WinUI.JumpListItemKind.Arguments && x.Arguments == arguments);
                    if (item == null)
                    {
                        item = WinUI.JumpListItem.CreateWithArguments(arguments, title);
                        isAdd = true;
                    }
                    else
                    {
                        item.DisplayName = title;
                    }
                    item.Description = description;
                    item.GroupName = customCategory;

                    if (isAdd) jumpList1.Items.Add(item);
                }

                await jumpList1.SaveAsync();
            }
            catch (Exception ex)
            {
                const string log_msg = $"{nameof(AddJumpItemsAsync)} catch.";
                Log.Error(TAG, ex, log_msg);
            }
        }
    }
}