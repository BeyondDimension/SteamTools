# Resx 翻译命令行工具(Resx Translation Command Line Tools/RTCLT)

## 文案指南/中西文混排
- [B 类产品文案指南](https://zhuanlan.zhihu.com/p/351739115)  
- [文案 - Ant Design](https://ant.design/docs/spec/copywriting-cn)  
- [通用文案](https://design.teambition.com/doc/copywriting)  
- [中西文混排](https://design.teambition.com/doc/mixed)  

## 命令示例
- 将 resx 文件导出为 xlsx 文件
    <pre>t write-xlsx -resx all -lang all // 导出所有的 resx, 与所有支持的语言</pre>
    <pre>t write-xlsx -resx all -lang en // 导出所有的 resx，语言仅 English</pre>
    <pre>t write-xlsx -resx all -lang all -only_machine // 导出所有的 resx(仅机翻或未翻译的值), 与所有支持的语言</pre>
- 使用机翻校对 xlsx 文件(将译文机翻回原文进行审阅)
    <pre>t proofread-xlsx -resx all -lang all // 校对所有的 xlsx, 与所有支持的语言</pre>
    <pre>t proofread-xlsx -resx all -lang en // 校对所有的 xlsx，语言仅 English</pre>
- 将 xlsx 文件导入到 resx 文件
    <pre>t read-xlsx -resx all -lang all // 导入所有的 xlsx, 与所有支持的语言</pre>
    <pre>t read-xlsx -resx all -lang en // 导入所有的 xlsx，语言仅 English</pre>

## Language Id
- [Windows Language Code Identifier (LCID) Reference](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c)
- [Steamworks 文献库 > 商店状态 > 本地化和语言](https://partner.steamgames.com/doc/store/localization#supported_languages)
- [R.cs](https://github.com/BeyondDimension/SteamTools/blob/develop/src/ST.Client.Desktop/UI/Resx/R.cs#L37-L44)

|  Language  |  Id  |
|  ----  |  ----  |
| Chinese (Simplified)  | zh-Hans |
| Chinese (Traditional)  | zh-Hant |
| English  | en |
| Korean  | ko |
| Japanese  | ja |
| Russian  | ru |
| Spanish  | es |
| Italian  | it |
