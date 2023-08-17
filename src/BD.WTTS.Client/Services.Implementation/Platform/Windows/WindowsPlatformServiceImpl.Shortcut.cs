#if WINDOWS
using System.Drawing;
using IWshRuntimeLibrary;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 创建快捷键方式
    /// </summary>
    /// <param name="pathLink"></param>
    /// <param name="targetPath">快捷方式的所在的位置</param>
    /// <param name="iconLocation">快捷键方式图标(传入头像地址)</param>
    /// <param name="arguments">参数</param>
    /// <param name="description">快捷键描述</param>
    /// <param name="hotkey">设置快捷键</param>
    /// <param name="workingDirectory">应用程序工作目录</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateShortcut(
        string pathLink,
        string targetPath,
        string iconLocation,
        string? arguments = null,
        string? description = null,
        string? hotkey = null,
        string? workingDirectory = null)
    {
        HttpClient http = new HttpClient();
        Stream stream = http.GetStreamAsync(iconLocation).Result;
        Bitmap imgBitMap = new Bitmap(stream);
        List<Bitmap> icoBitMap = new List<Bitmap>();
        Icon[] icon = GetAppIcon();    //获取app.ico分辨率集合
        foreach (Icon iconSource in icon)
        {
            Bitmap bgbitmap = ResizeImage(imgBitMap, iconSource.Width, iconSource.Height); //调整头像大小
            Bitmap rmap = SuperpositionIcon(bgbitmap, iconSource); //叠加app.ico
            icoBitMap.Add(rmap);
        }
        iconLocation = CreateIcon(icoBitMap, iconLocation);
        Shortcut(pathLink, targetPath, arguments, description, hotkey, iconLocation, workingDirectory);
    }

    void IPlatformService.CreateShortcut(
        string pathLink,
        string targetPath,
        string iconLocation,
        string? arguments,
        string? description,
        string? hotkey,
        string? workingDirectory)
    {
        CreateShortcut(pathLink, targetPath, iconLocation, arguments, description, hotkey, workingDirectory);
    }

    private static Bitmap ResizeImage(Bitmap image, int width, int height)
    {
        Bitmap scaledImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(scaledImage))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;

            graphics.DrawImage(image, 0, 0, width, height);
        }
        return scaledImage;
    }

    private static Bitmap SuperpositionIcon(Bitmap image, Icon icoIcon)
    {
        int iconWidth = icoIcon.Width / 2;
        int iconHeight = icoIcon.Height / 2;

        int watermarkLeft = icoIcon.Width - iconWidth;
        int watermarkTop = icoIcon.Height - iconHeight;
        icoIcon.ToBitmap().MakeTransparent();

        using (Graphics graphics = Graphics.FromImage(image))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.DrawIcon(icoIcon, new Rectangle(watermarkLeft, watermarkTop, iconWidth, iconHeight));
        }
        return image;
    }

    private static string CreateIcon(List<Bitmap> bitmapList, string iconName)
    {
        iconName = Path.GetFileNameWithoutExtension(iconName);
        string savepath = Path.Combine(ProjectUtils.GetProjectPath(), "cache");
        if (!Directory.Exists(savepath))
        {
            Directory.CreateDirectory(savepath);
        }
        var icopath = Path.Combine(savepath, string.Concat(iconName, ".ico"));
        using (var stream = new FileStream(icopath, FileMode.Create))
        {
            IconFactory.SavePngsAsIcon(bitmapList.ToArray(), stream);
        }
        return icopath;
    }

    private static void Shortcut(string pathLink, string targetPath, string? arguments, string? description, string? hotkey, string? iconLocation, string? workingDirectory)
    {
        WshShell shell = new();
        var shortcut = (IWshShortcut)shell.CreateShortcut(pathLink);
        shortcut.TargetPath = targetPath;

        if (!string.IsNullOrEmpty(description))
            shortcut.Description = description;

        if (!string.IsNullOrEmpty(arguments))
            shortcut.Arguments = arguments;

        if (!string.IsNullOrEmpty(hotkey))
            shortcut.Hotkey = hotkey;

        if (!string.IsNullOrEmpty(iconLocation))
            shortcut.IconLocation = iconLocation;

        if (!string.IsNullOrEmpty(workingDirectory))
            shortcut.WorkingDirectory = workingDirectory;

        shortcut.Save();
    }

    private static Icon[] GetAppIcon()
    {
        string iconPath = Path.Combine(ProjectUtils.GetProjectPath(), "res\\icons\\app\\v3\\Icon.ico");
        var iconSize = Enum.GetValues(typeof(IconFactory.IMAGELIST_SIZE_FLAG));
        Icon[] icons = new Icon[iconSize.Length];
        for (int i = 0; i < iconSize.Length; i++)
        {
            icons[i] = IconFactory.GetIconFromFile(iconPath, (IconFactory.IMAGELIST_SIZE_FLAG)iconSize.GetValue(i));
        }
        return icons;
    }
}
#endif