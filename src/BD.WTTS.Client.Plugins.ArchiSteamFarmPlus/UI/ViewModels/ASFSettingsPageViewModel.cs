namespace BD.WTTS.UI.ViewModels;

public partial class ASFSettingsPageViewModel : ViewModelBase
{
    private readonly IArchiSteamFarmService asfService = IArchiSteamFarmService.Instance;

    public ASFSettingsPageViewModel()
    {
        SelectGlobalFiles = ReactiveCommand.CreateFromTask(SelectGlobalFiles_Click);

        SetEncryptionKey = ReactiveCommand.Create(SetEncryptionKey_Click);

        OpenASFFolder = ReactiveCommand.Create<string>(ASFService.Current.OpenFolder);

        RefreshGlobalConfig = ReactiveCommand.Create(ASFService.Current.RefreshConfig);
    }

    /// <summary>
    /// 选择 <see cref="GlobalConfig"/> 全局配置文件
    /// </summary>
    /// <returns></returns>
    public async Task SelectGlobalFiles_Click()
    {
        if (!ASFService.Current.IsASFRuning)
        {
            Toast.Show(ToastIcon.Info, BDStrings.ASF_RequirRunASF, ToastLength.Short);
            return;
        }

        FilePickerFileType? fileTypes;
        if (IApplication.IsDesktop())
        {
            fileTypes = new ValueTuple<string, string[]>[]
            {
                    ("Json Files", new[] { FileEx.JSON, }),
                //("All Files", new[] { "*", }),
            };
        }
        else if (OperatingSystem2.IsAndroid())
        {
            fileTypes = new[] { MediaTypeNames.JSON };
        }
        else
        {
            fileTypes = null;
        }
        await FilePicker2.PickAsync(ASFService.Current.ImportGlobalFiles, fileTypes);
    }

    public async void SetEncryptionKey_Click() => await asfService.SetEncryptionKeyAsync();
}
