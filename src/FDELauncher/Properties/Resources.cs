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
