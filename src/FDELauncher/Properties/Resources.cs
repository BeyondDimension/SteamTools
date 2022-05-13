using System;
using System.Globalization;

namespace FDELauncher.Properties;

static class Resources
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

    public static string GetString(Func<Language, string> getString, CultureInfo? resourceCulture = null)
    {
        resourceCulture ??= Culture;
        if (CultureInfo2.IsMatch(resourceCulture, "zh-Hans"))
            return getString(Language.ChineseSimplified);
        else if (CultureInfo2.IsMatch(resourceCulture, "zh-Hant"))
            return getString(Language.ChineseTraditional);
        else if (CultureInfo2.IsMatch(resourceCulture, "es"))
            return getString(Language.Spanish);
        else if (CultureInfo2.IsMatch(resourceCulture, "it"))
            return getString(Language.Italian);
        else if (CultureInfo2.IsMatch(resourceCulture, "ja"))
            return getString(Language.Japanese);
        else if (CultureInfo2.IsMatch(resourceCulture, "ko"))
            return getString(Language.Korean);
        else if (CultureInfo2.IsMatch(resourceCulture, "ru"))
            return getString(Language.Russian);
        else
            return getString(Language.English);
    }

    public static string AspNetCoreRuntimeFormat2 => GetString(l => l switch
    {
        Language.ChineseSimplified => "ASP.NET Core 运行时 {0} ({1})",
        Language.ChineseTraditional => "ASP.NET Core 運行時 {0} ({1})",
        Language.Japanese => "ASP.NET Core ランタイム {0} ({1})",
        _ => "ASP.NET Core Runtime {0} ({1})",
    });

    public static string ExecutiveNotExistsFailure => GetString(l => l switch
    {
        Language.ChineseSimplified => "主程序文件不存在，请重新下载或安装此应用。",
        Language.ChineseTraditional => "主程序檔案不存在，請重新下載或安裝此應用。",
        Language.Spanish => "El archivo principal del programa no existe, por favor descargue o instale esta aplicación de nuevo.",
        Language.Italian => "Il file del programma principale non esiste, scaricare o installare nuovamente questa applicazione.",
        Language.Japanese => "メインプログラムのファイルが存在しないので、このアプリケーションを再度ダウンロードまたはインストールしてください。",
        Language.Korean => "주 프로그램 파일이 존재하지 않습니다. 이 프로그램을 다시 다운로드하거나 설치하십시오.",
        Language.Russian => "Основной файл программы не существует, пожалуйста, загрузите или установите это приложение снова.",
        _ => "The main program file does not exist, please download or install this application again.",
    });

    public static string VerificationExecutiveInfoFailure => GetString(l => l switch
    {
        Language.ChineseSimplified => "主程序校验失败，请重新下载或安装此应用。",
        Language.ChineseTraditional => "主程序校驗失敗，請重新下載或安裝此應用。",
        Language.Spanish => "La verificación del programa principal ha fallado, por favor descargue o instale esta aplicación de nuevo.",
        Language.Italian => "La verifica del programma principale non è riuscita, si prega di scaricare o installare nuovamente questa applicazione.",
        Language.Japanese => "メインプログラムの検証に失敗しました。このアプリケーションを再度ダウンロードまたはインストールしてください。",
        Language.Korean => "주 프로그램 검사에 실패했습니다. 이 프로그램을 다시 다운로드하거나 설치하십시오.",
        Language.Russian => "Проверка основной программы не удалась, пожалуйста, загрузите или установите это приложение снова.",
        _ => "The main program verification failed, please download or install this application again.",
    });

    public static string OpenCoreByProcessOnExceptionFormat1 => GetString(l => l switch
    {
        Language.ChineseSimplified => "打开 Web 链接失败{0}，请设置系统默认浏览器后再尝试。",
        Language.ChineseTraditional => "打開 Web 連結失敗{0}，請設定系統默認瀏覽器後再嘗試。",
        Language.Spanish => "No se ha podido abrir el enlace web {0}, por favor, configure el navegador por defecto del sistema e inténtelo de nuevo.",
        Language.Italian => "Impossibile aprire il collegamento Web {0}, impostare il browser predefinito del sistema e riprovare.",
        Language.Japanese => "Webリンク{0} を開くのに失敗しました。システムのデフォルトブラウザを設定して、もう一度やり直してください。",
        Language.Korean => "웹 링크를 열 수 없습니다{0}. 시스템 기본 브라우저를 설정한 다음 시도하십시오.",
        Language.Russian => "Не удалось открыть веб-ссылку{0}, пожалуйста, установите системный браузер по умолчанию и повторите попытку.",
        _ => "Failed to open Web link{0}, please set the system default browser and try again.",
    });

    public static string RuntimeMissingFailureFormat2 => GetString(l => l switch
    {
        Language.ChineseSimplified => "{0} 框架依赖版(framework-dependent executable) 必须安装 {1} 才能运行，你应下载文件名中不包含 fde 的版本或安装运行时。\r\n\r\n你想现在就下载并安装运行时吗？",
        Language.ChineseTraditional => "{0} 框架依賴版(framework-dependent executable) 必須安裝 {1} 才能運行，你應下載檔名中不包含 fde 的版本或安裝運行時。\r\n\r\n你想現在就下載並安裝運行時嗎？",
        Language.Spanish => "{0} el ejecutable dependiente del framework debe ser instalado {1} para ejecutarse, debe descargar la versión sin fde en el nombre del archivo o instalar el runtime.\r\n\r\n¿Quieres descargar e instalar el tiempo de ejecución ahora?",
        Language.Italian => "{0} l'eseguibile dipendente dal framework deve essere installato {1} per essere eseguito, si deve scaricare la versione senza fde nel nome del file o installare il runtime.\r\n\r\nVolete scaricare e installare il runtime ora?",
        Language.Japanese => "{0} フレームワーク依存の実行ファイルをインストールする必要があります {1} 実行するには、ファイル名に fde が含まれていないバージョンをダウンロードするか、ランタイムをインストールする必要があります。\r\n\r\n今すぐランタイムをダウンロードし、インストールしますか？",
        Language.Korean => "{0} framework-dependent executable 는 {1} 을 설치해야 실행할 수 있습니다. 파일 이름에 fde가 포함되지 않은 버전을 다운로드하거나 설치할 때 실행해야 합니다.\r\n\r\n런타임을 다운로드하여 지금 설치하시겠습니까?",
        Language.Russian => "{0} исполняемый файл, зависящий от фреймворка, должен быть установлен {1} для запуска необходимо загрузить версию без fde в имени файла или установить среду выполнения. \r\n\r\nНе хотите ли вы загрузить и установить программу выполнения сейчас?",
        _ => "{0} framework-dependent executable must be installed {1} to run, you should download the version without fde in the filename or install the runtime. \r\n\r\nDo you want to download and install the runtime now?",
    });

    public static string NotSupportedWin8PlatformError => GetString(l => l switch
    {
        Language.ChineseSimplified => "由于微软官方对 Windows 8 的支持已结束，故本程序无法在此操作系统上运行，建议升级到 Windows 8.1 或 Windows 10/11",
        Language.ChineseTraditional => "由於微軟官方對 Windows 8 的支持已結束，故本程式無法在此作業系統上運行，建議陞級到 Windows 8.1 或 Windows 10/11",
        Language.Spanish => "Debido a que el soporte oficial de Microsoft para Windows 8 ha finalizado, este programa no se puede ejecutar en este sistema operativo y se recomienda actualizar a Windows 8.1 o Windows 10/11",
        Language.Italian => "Poiché il supporto ufficiale di Microsoft per Windows 8 è terminato, questo programma non può essere eseguito su questo sistema operativo ed è consigliabile eseguire l&apos;aggiornamento a Windows 8.1 o Windows 10/11",
        Language.Japanese => "Windows 8 はマイクロソフトの公式サポートが終了しているため、このプログラムは動作しませんので、Windows 8.1 または Windows 10/11 へのアップグレードをお勧めします。",
        Language.Korean => "Windows 8 에 대한 Microsoft 의 공식 지원이 종료되었으므로이 프로그램은이 운영 체제에서 실행할 수 없습니다. Windows 8.1 또는 Windows 10/11 으로 업그레이드하는 것이 좋습니다.",
        Language.Russian => "Эта программа не будет работать на Windows 8, так как официальная поддержка этой операционной системы компанией Microsoft закончилась, поэтому рекомендуется перейти на Windows 8.1 или Windows 10/11",
        _ => "Since Microsoft's official support for Windows 8 has ended, this program will not work on this operating system, so it is recommended to upgrade to Windows 8.1 or Windows 10/11",
    });

    public static string NotSupportedPlatformError => GetString(l => l switch
    {
        Language.ChineseSimplified => "操作系统最低需要 Windows 7 SP1",
        Language.ChineseTraditional => "作業系統最低需要 Windows 7 SP1",
        Language.Spanish => "El sistema operativo requiere un mínimo de Windows 7 SP1",
        Language.Italian => "Il sistema operativo richiede un minimo di Windows 7 SP1",
        Language.Japanese => "オペレーティングシステムは Windows 7 SP1 以上",
        Language.Korean => "운영 체제에는 Windows 7 SP1 이상이 필요합니다.",
        Language.Russian => "Операционная система требует минимум Windows 7 SP1",
        _ => "The minimum requirement of operating system is Windows 7 SP1",
    });

    public static string ThisAppOnlySupport64BitOS => GetString(l => l switch
    {
        Language.ChineseSimplified => "此应用仅支持 64 位操作系统",
        Language.ChineseTraditional => "此應用僅支持 64 位操作系統",
        Language.Spanish => "Esta aplicación solo admite sistemas operativos de 64 bits",
        Language.Italian => "Questa app supporta solo sistemi operativi a 64 bit",
        Language.Japanese => "このアプリケーションは、64 ビットのオペレーティングシステムにのみ対応しています。",
        Language.Korean => "앱은 64 비트 운영 체제 만 지원합니다.",
        Language.Russian => "Это приложение поддерживает только 64-битные операционные системы",
        _ => "This application only supports 64-bit operating systems",
    });
}

enum Language
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
