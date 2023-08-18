#if WINDOWS
using System.Drawing;
using IWshRuntimeLibrary;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SkiaSharp;
using Res = BD.WTTS.Properties.Resources;

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
        //using var b = SKBitmap.Decode(Res.AppLogo_512);
        HttpClient http = new HttpClient();
        Stream stream = http.GetStreamAsync(iconLocation).Result;
        Bitmap imgBitMap = new Bitmap(stream);
        List<Bitmap> icoBitMap = new List<Bitmap>();
        Icon[] icon = GetAppIcon();    //获取app.ico分辨率集合
        foreach (Icon iconSource in icon)
        {
            Bitmap rmap = ResizeImage(imgBitMap, iconSource.Width, iconSource.Height, iconSource);  //调整头像大小叠加app.ico
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

    private static Bitmap ResizeImage(Bitmap image, int width, int height, Icon icoIcon)
    {
        int iconWidth = width / 2;
        int iconHeight = height / 2;

        int watermarkLeft = width - iconWidth;
        int watermarkTop = height - iconHeight;
        Bitmap scaledImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(scaledImage))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.DrawImage(image, 0, 0, width, height);
            graphics.DrawIcon(icoIcon, new Rectangle(watermarkLeft, watermarkTop, iconWidth, iconHeight));
        }
        return scaledImage;
    }

    static string CreateIcon(List<Bitmap> bitmapList, string iconName)
    {
        iconName = Path.GetFileNameWithoutExtension(iconName);
        string savepath = IOPath.CacheDirectory;
        if (!Directory.Exists(savepath))
        {
            Directory.CreateDirectory(savepath);
        }
        var icopath = Path.Combine(savepath, $"{iconName}.ico");
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
        string iconPath = Path.Combine(IOPath.BaseDirectory, "res\\icons\\app\\v3\\Icon.ico");
        var iconSize = Enum2.GetAll<IconFactory.IMAGELIST_SIZE_FLAG>();

        var icons = new Icon[iconSize.Length];
        for (int i = 0; i < iconSize.Length; i++)
        {
            icons[i] = IconFactory.GetIconFromFile(iconPath, (IconFactory.IMAGELIST_SIZE_FLAG)iconSize.GetValue(i));
        }
        return icons;
    }
}
#endif