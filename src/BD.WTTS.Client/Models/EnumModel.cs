using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Models;

public abstract class EnumModel : ReactiveObject
{
    public static IEnumerable<EnumModel<TEnum>> GetEnums<TEnum>() where TEnum : struct, Enum
        => Enum2.GetAll<TEnum>().Select(x => new EnumModel<TEnum>(x.ToString(), x));
}

public class EnumModel<TEnum> : EnumModel where TEnum : struct, Enum
{
    public EnumModel() { }

    public EnumModel(string name, TEnum value)
    {
        Name = name;
        Value = value;
    }

    string _Name = "";

    public string Name
    {
        get => _Name;
        set => this.RaiseAndSetIfChanged(ref _Name, value);
    }

    ///// <summary>
    ///// 获取该枚举值的本地化显示字符串，兼容之前的 Xaml，已弃用
    ///// </summary>
    //[Obsolete("use LocalizationName", true)]
    //public string? Name_Localiza => LocalizationName;

    ///// <summary>
    ///// 获取该枚举值的本地化显示字符串，使用 <see cref="Description"/> 或 <see cref="Name"/> 查找本地化资源
    ///// </summary>
    //[Obsolete("use LocalizationName2")]
    //public string? LocalizationName
    //{
    //    get => string.IsNullOrEmpty(Description)
    //        ? AppResources.ResourceManager.GetString(Name, AppResources.Culture)
    //        : AppResources.ResourceManager.GetString(Description, AppResources.Culture);
    //}

    /// <summary>
    /// 获取该枚举值的本地化显示字符串，使用 枚举类型_枚举名 查找本地化资源
    /// </summary>
    public string? LocalizationName =>
        AppResources.ResourceManager.GetString(
            $"{typeof(TEnum).Name}_{Name}",
            AppResources.Culture);

    string? _Description;

    public string? Description
    {
        get
        {
            _Description ??= Enum2.GetDescription(Value) ?? "";
            return _Description;
        }
        set => _Description = value;
    }

    TEnum _Value;

    public TEnum Value
    {
        get => _Value;
        set => this.RaiseAndSetIfChanged(ref _Value, value);
    }

    bool _Enable;

    public bool Enable
    {
        get => _Enable;
        set => this.RaiseAndSetIfChanged(ref _Enable, value);
    }

    int _Count;

    /// <summary>
    /// 枚举作为 type 时统计集合数量
    /// </summary>
    public int Count
    {
        get => _Count;
        set => this.RaiseAndSetIfChanged(ref _Count, value);
    }
}