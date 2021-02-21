namespace System
{
    /// <summary>
    /// Android ROM 种类。
    /// </summary>
    public enum AndroidROMType : byte
    {
        /// <summary>
        /// Unknown or Other ROM
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// MIUI (MI/小米)
        /// </summary>
        MIUI,

        /// <summary>
        /// Emotion UI (HUAWEI/华为)
        /// </summary>
        EMUI,

        /// <summary>
        /// ColorOS (OPPO)
        /// </summary>
        ColorOS,

        /// <summary>
        /// Funtouch OS (vivo)
        /// </summary>
        FuntouchOS,

        /// <summary>
        /// Smartisan OS (Smartisan/锤子)
        /// </summary>
        SmartisanOS,

        /// <summary>
        /// Flyme (Meizu/魅族)
        /// </summary>
        Flyme,

        /// <summary>
        /// amigoOS (GIONEE/金立)
        /// </summary>
        AmigoOS,

        /// <summary>
        /// EUI (LeEco/乐视)
        /// </summary>
        EUI,

        /// <summary>
        /// HTC Sense® (宏达电子公司)
        /// </summary>
        HTCSense,

        /// <summary>
        /// nubia UI (努比亚)
        /// </summary>
        NubiaUI,
    }
}