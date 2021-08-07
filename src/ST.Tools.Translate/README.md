# 客户端文本翻译命令行工具(Command Line Tools/CLT)

## 文案指南/中西文混排
- [B 类产品文案指南](https://zhuanlan.zhihu.com/p/351739115)  
- [文案 - Ant Design](https://ant.design/docs/spec/copywriting-cn)  
- [通用文案](https://design.teambition.com/doc/copywriting)  
- [中西文混排](https://design.teambition.com/doc/mixed)  

## 命令示例
- 将 resx 文件导出为 xlsx
    <pre>t write-xlsx -resx all -lang all // 导出所有的 resx, 与所有支持的语言</pre>
    <pre>t write-xlsx -resx all -lang en // 导出所有的 resx，语言仅 English</pre>

## Language Id
- [Windows Language Code Identifier (LCID) Reference](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c)
- [R.cs](https://github.com/SteamTools-Team/SteamTools/blob/develop/src/ST.Client.Desktop/UI/Resx/R.cs#L37-L44)

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