namespace BD.WTTS.Client.Resources;

static class Strings
{
    static CultureInfo? resourceCulture;

    public static CultureInfo Culture
    {
        get
        {
            return resourceCulture ?? CultureInfo.CurrentUICulture;
        }
        set
        {
            resourceCulture = value;
        }
    }

    public enum Language : byte
    {
        English,
        Spanish,
        Italian,
        Japanese,
        Korean,
        Russian,
        ChineseSimplified,
        ChineseTraditional,
    }

    public static string GetLang() => GetString(l => l switch
    {
        Language.ChineseSimplified => "zh-cn",
        Language.Japanese => "ja-jp",
        _ => "en-us",
    });

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsMatch(this CultureInfo cultureInfo, string cultureName)
    {
        if (
#if NET35
            Program
#else
            string
#endif
            .IsNullOrWhiteSpace(cultureInfo.Name))
        {
            return false;
        }
        if (string.Equals(cultureInfo.Name, cultureName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else
        {
            return cultureInfo.Parent.IsMatch(cultureName);
        }
    }

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string GetString(Func<Language, string> getString, CultureInfo? resourceCulture = null)
    {
        resourceCulture ??= Culture;
        if (resourceCulture.IsMatch(AssemblyInfo.CultureName_SimplifiedChinese))
            return getString(Language.ChineseSimplified);
        else if (resourceCulture.IsMatch(AssemblyInfo.CultureName_TraditionalChinese))
            return getString(Language.ChineseTraditional);
        else if (resourceCulture.IsMatch(AssemblyInfo.CultureName_Spanish))
            return getString(Language.Spanish);
        else if (resourceCulture.IsMatch(AssemblyInfo.CultureName_Italian))
            return getString(Language.Italian);
        else if (resourceCulture.IsMatch(AssemblyInfo.CultureName_Japanese))
            return getString(Language.Japanese);
        else if (resourceCulture.IsMatch(AssemblyInfo.CultureName_Korean))
            return getString(Language.Korean);
        else if (resourceCulture.IsMatch(AssemblyInfo.CultureName_Russian))
            return getString(Language.Russian);
        else
            return getString(Language.English);
    }

    public static string AspNetCoreRuntimeFormat1 => GetString(l => l switch
    {
        Language.ChineseSimplified => $"ASP.NET Core 运行时 {Program.dotnet_version} ({{0}})",
        Language.ChineseTraditional => $"ASP.NET Core 運行時 {Program.dotnet_version} ({{0}})",
        Language.Japanese => $"ASP.NET Core ランタイム {Program.dotnet_version} ({{0}})",
        _ => $"ASP.NET Core Runtime {Program.dotnet_version} ({{0}})",
    });

    public static string NetRuntimeFormat1 => GetString(l => l switch
    {
        Language.ChineseSimplified => $".NET 运行时 {Program.dotnet_version} ({{0}})",
        Language.ChineseTraditional => $".NET 運行時 {Program.dotnet_version} ({{0}})",
        Language.Japanese => $".NET ランタイム {Program.dotnet_version} ({{0}})",
        _ => $".NET Runtime {Program.dotnet_version} ({{0}})",
    });

    public static string And => GetString(l => l switch
    {
        Language.ChineseSimplified or Language.ChineseTraditional => "和",
        Language.Spanish => "y",
        Language.Italian => "e",
        Language.Japanese => "と",
        Language.Korean => "및",
        Language.Russian => "и",
        _ => "and",
    });

    public static string FrameworkMissingFailureFormat1 => GetString(l => l switch
    {
        Language.ChineseSimplified => "此应用程序必须安装 {0} 才能运行，你想现在就下载并安装运行时吗？",
        Language.ChineseTraditional => "此應用程序必須安裝 {0} 才能運行，你想現在就下載並安裝運行時嗎？",
        Language.Spanish => "Esta aplicación debe tener {0} instalado para ejecutarse, ¿desea descargar e instalar el runtime ahora?",
        Language.Italian => "Questa applicazione deve avere {0} installato per funzionare, si desidera scaricare e installare il runtime ora?",
        Language.Japanese => "このアプリケーションを実行するには {0} がインストールされている必要がありますが、今すぐランタイムをダウンロードしインストールしますか？",
        Language.Korean => "이 응용 프로그램을 실행하려면 {0} 이 설치되어 있어야 합니다. 지금 런타임을 다운로드하여 설치하시겠습니까?",
        Language.Russian => "Для запуска этого приложения необходимо установить {0}, хотите ли вы загрузить и установить среду выполнения сейчас?",
        _ => "This application must have {0} installed to run, would you like to download and install the runtime now?",
    });

    public static string Error => GetString(l => l switch
    {
        Language.ChineseSimplified => "错误",
        Language.ChineseTraditional => "錯誤",
        Language.Italian => "Errore",
        Language.Japanese => "エラー",
        Language.Korean => "오류",
        Language.Russian => "Ошибка",
        _ => "Error",
    });

    public static string Error_BaseDir_StartsWith_Temp => GetString(l => l switch
    {
        Language.ChineseSimplified => "不能在临时文件夹中运行此程序，请将所有文件复制或解压到其他路径后再启动程序",
        Language.ChineseTraditional => "",
        Language.Spanish => "No puede ejecutar este programa en una carpeta temporal, por favor copie o extraiga todos los archivos a otra ruta antes de iniciar el programa",
        Language.Italian => "Non è possibile eseguire il programma in una cartella temporanea; copiare o estrarre tutti i file in un altro percorso prima di avviare il programma",
        Language.Japanese => "このプログラムは一時フォルダでは実行できませんので、すべてのファイルを別のパスにコピーまたは解凍してからプログラムを起動してください",
        Language.Korean => "이 프로그램은 임시 폴더에서 실행할 수 없으므로 프로그램을 시작하기 전에 모든 파일을 다른 경로로 복사하거나 압축을 풀어야 합니다",
        Language.Russian => "Вы не можете запустить эту программу во временной папке, пожалуйста, скопируйте или извлеките все файлы в другой путь перед запуском программы",
        _ => "You cannot run this program in the temporary folder, please copy or extract all files to another path and then start the program",
    });

    public static string Error_IncompatibleOS => GetString(l => l switch
    {
        Language.ChineseSimplified => "此应用程序仅兼容 Windows 11 与 Windows 10 版本 1809（OS 内部版本 17763）或更高版本",
        Language.ChineseTraditional => "此應用程序僅相容 Windows 11 與 Windows 10 版本 1809（OS 內部版本 17763）或更高版本",
        Language.Spanish => "Esta aplicación solo es compatible con Windows 11 y Windows 10 versión 1809 (versión interna del sistema operativo 17763) o superior",
        Language.Italian => "Questa applicazione è compatibile solo con Windows 11 e Windows 10 versione 1809 (versione interna del sistema operativo 17763) o superiore",
        Language.Japanese => "このアプリケーションは、Windows 11およびWindows 10 バージョン1809（OS内部バージョン17763）以降にのみ対応しています",
        Language.Korean => "이 애플리케이션은 Windows 11 및 Windows 10 버전 1809(OS 내부 버전 17763) 이상과만 호환됩니다",
        Language.Russian => "Это приложение совместимо только с Windows 11 и Windows 10 версии 1809 (внутренняя версия ОС 17763) или выше",
        _ => "This application is only compatible with Windows 11 and Windows 10 version 1809 (OS internal version 17763) or higher",
    });

    public static string OpenCoreByProcess_Win32Exception_ => GetString(l => l switch
    {
        Language.ChineseSimplified => "打开 Web 链接失败 0x{0}，请设置系统默认浏览器后再尝试",
        Language.ChineseTraditional => "打開 Web 連結失敗 0x{0}，請設定系統默認瀏覽器後再嘗試",
        Language.Spanish => "No se ha podido abrir el enlace web 0x{0}, por favor, configure el navegador por defecto del sistema e inténtelo de nuevo",
        Language.Italian => "Impossibile aprire il collegamento Web 0x{0}, impostare il browser predefinito del sistema e riprovare",
        Language.Japanese => "Webリンク 0x{0} を開くのに失敗しました。システムのデフォルトブラウザを設定して、もう一度やり直してください",
        Language.Korean => "웹 링크를 열 수 없습니다 0x{0}. 시스템 기본 브라우저를 설정한 다음 시도하십시오",
        Language.Russian => "Невозможно открыть ссылку 0x{0}, повторите снова, после установки браузер по умолчанию.",
        _ => "Failed to open Web link 0x{0}, please set the system default browser and try again",
    });
}