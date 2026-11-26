#if WINDOWS
using static BD.WTTS.Services.IJumpListService;
using WinUI = Windows.UI.StartScreen;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IJumpListService"/>
internal sealed class JumpListServiceImpl : IJumpListService
{
    async ValueTask IJumpListService.AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    {
        if (DesktopBridge.IsRunningAsUwp && await AddPackagedJumpItemsAsync(items))
            return;

        try
        {
            DesktopJumpList.Apply(items);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Could not update the desktop Jump List.");
        }
    }

    static async ValueTask<bool> AddPackagedJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    {
        try
        {
            if (!WinUI.JumpList.IsSupported())
                return false;

            var jumpList1 = await WinUI.JumpList.LoadCurrentAsync();

            foreach (var (title, _, _, arguments, description, customCategory) in items)
            {
                var isAdd = false;
                var item = jumpList1.Items.FirstOrDefault(x =>
                    x.Kind == WinUI.JumpListItemKind.Arguments && x.Arguments == arguments);
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
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Could not update the packaged Jump List.");
            return false;
        }
    }
}
#endif
