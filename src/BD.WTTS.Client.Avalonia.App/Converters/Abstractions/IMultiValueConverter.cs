global using IMultiValueConverter = BD.WTTS.Converters.Abstractions.IMultiValueConverter;
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name
using CommonCA = BD.Common.Converters.Abstractions;
#pragma warning restore SA1211 // Using alias directives should be ordered alphabetically by alias name
#pragma warning restore IDE0079 // 请删除不必要的忽略

namespace BD.WTTS.Converters.Abstractions;

public partial interface IMultiValueConverter : CommonCA.IMultiValueConverter, IBinding
{
}

partial interface IMultiValueConverter :
#if AVALONIA
    Avalonia.Data.Converters.IMultiValueConverter
#endif
{

}