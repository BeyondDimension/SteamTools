using ReactiveUI;
using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public class EnumClass : ReactiveObject
    {
        public static List<EnumClass<E>> GetEnums<E>() where E : struct, Enum
        {
            var list = new List<EnumClass<E>>();
            foreach (var key in Enum.GetNames(typeof(E)))
            {
                list.Add(new EnumClass<E>(key, Enum.Parse<E>(key)));
            }
            return list;
        }
    }

    public class EnumClass<T> : ReactiveObject where T : struct, Enum
    {
        public EnumClass() { }

        public EnumClass(string name, T value)
        {
            Name = name;
            Value = value;
        }

        private string? _Name;
        public string? Name
        {
            get => _Name;
            set => this.RaiseAndSetIfChanged(ref _Name, value);
        }

        public string? Description
        {
            get => Enum2.GetDescription<T>(Value);
        }

        public string? Name_Localiza
        {
            get => string.IsNullOrEmpty(Description)
                ? AppResources.ResourceManager.GetString(Name, AppResources.Culture)
                : AppResources.ResourceManager.GetString(Description, AppResources.Culture);
        }

        T _Value;
        public T Value
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
}
