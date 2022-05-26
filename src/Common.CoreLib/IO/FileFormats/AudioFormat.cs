namespace System.IO.FileFormats;

/// <summary>
/// 音频格式
/// </summary>
public enum AudioFormat : byte
{
    // ReSharper disable once InconsistentNaming
    // 此枚举中禁止出现值为0的

    /// <summary>
    /// 自适应多速率音频压缩（Adaptive multi-Rate compression，简称AMR）是一个使语音编码最优化的专利。AMR被标准语音编码 3GPP在1998年10月选用，现在广泛在GSM和UMTS中使用。它使用1-8个不同的位元率编码。
    /// <para>AMR 也是一个文件格式，存储AMR 语音编码文件. 很多手机允许用户存储短时间的AMR 格式录音，而部分开源（参看外部链接）和商业软件有和其他格式转换的程序。但是AMR是一个语音格式，并未针对其他声音进行最优化。普通文件扩展名是 .amr。</para>
    /// <para>https://en.wikipedia.org/wiki/Adaptive_Multi-Rate_audio_codec</para>
    /// </summary>
    AMR = 9,

    /// <summary>
    /// Waveform Audio File Format（WAVE，又或者是因为扩展名而被大众所知的WAV），是微软与IBM公司所开发在个人计算机存储音频流的编码格式，在Windows平台的应用软件受到广泛的支持，地位上类似于麦金塔计算机里的AIFF。此格式属于资源交换档案格式(RIFF)的应用之一，通常会将采用脉冲编码调制的音频资存储在区块中。也是其音乐发烧友中常用的指定规格之一。由于此音频格式未经过压缩，所以在音质方面不会出现失真的情况，但档案的体积因而在众多音频格式中较为大。
    /// <para>https://en.wikipedia.org/wiki/WAV</para>
    /// </summary>
    WAV,

    /// <summary>
    /// Core Audio Format 是一个用于存储音频的容器，由 Apple Inc. 开发。它与 Mac OS X 10.4 及更高版本兼容; Mac OS X 10.3 需要安装 QuickTime 7。
    /// <para>https://en.wikipedia.org/wiki/Core_Audio_Format</para>
    /// </summary>
    CAF,

    /// <summary>
    /// 动态图像专家组-1或动态图像专家组-2 音频层III（MPEG-1 or MPEG-2 Audio Layer III），常简称为MP3，是当今流行的一种数字音频编码和有损压缩格式，它被设计来大幅降低音频数据量，通过舍弃PCM音频数据中对人类听觉不重要的部分，达成压缩成较小文件的目的。而对于大多数用户的听觉感受来说，MP3的音质与最初的不压缩音频相比没有明显的下降。它是在1991年，由位于德国埃尔朗根的研究组织弗劳恩霍夫协会的一组工程师发明和标准化的。MP3的普及，曾对音乐产业造成冲击与影响。
    /// <para>https://en.wikipedia.org/wiki/MP3</para>
    /// </summary>
    MP3,
}
