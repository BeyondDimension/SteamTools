using ReactiveUI;
using System.Application.Entities;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.ViewModels
{
    public static class AreaUIHelper
    {
        public interface IViewModel : IViewModelBase
        {
            IReadOnlyList<IArea>? AreaItems2 { get; set; }
            IArea? AreaSelectItem2 { get; set; }
            IReadOnlyList<IArea>? AreaItems3 { get; set; }
            IArea? AreaSelectItem3 { get; set; }
            IReadOnlyList<IArea>? AreaItems4 { get; set; }
            IArea? AreaSelectItem4 { get; set; }
            bool AreaNotVisible3 { get; set; }
            bool AreaNotVisible4 { get; set; }
        }

        public interface IViewModelField : IViewModel
        {
            IReadOnlyList<IArea>? MAreaItems3 { set; }
            IReadOnlyList<IArea>? MAreaItems4 { set; }
            IArea? MAreaSelectItem2 { set; }
            IArea? MAreaSelectItem3 { set; }
            IArea? MAreaSelectItem4 { set; }
            bool MAreaNotVisible3 { set; }
            bool MAreaNotVisible4 { set; }
        }

        static void SetAreaSelectItem2(this IViewModelField v, IArea value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaSelectItem2 = value;
            }
            else
            {
                v.MAreaSelectItem2 = value;
            }
        }

        static void SetAreaItems3(this IViewModelField v, IReadOnlyList<IArea> value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaItems3 = value;
            }
            else
            {
                v.MAreaItems3 = value;
            }
        }

        static void SetAreaSelectItem3(this IViewModelField v, IArea value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaSelectItem3 = value;
            }
            else
            {
                v.MAreaSelectItem3 = value;
            }
        }

        static void SetAreaNotVisible3(this IViewModelField v, bool value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaNotVisible3 = value;
            }
            else
            {
                v.MAreaNotVisible3 = value;
            }
        }

        static void SetAreaItems4(this IViewModelField v, IReadOnlyList<IArea> value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaItems4 = value;
            }
            else
            {
                v.MAreaItems4 = value;
            }
        }

        static void SetAreaNotVisible4(this IViewModelField v, bool value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaNotVisible4 = value;
            }
            else
            {
                v.MAreaNotVisible4 = value;
            }
        }

        static void SetAreaSelectItem4(this IViewModelField v, IArea value, bool isSetProperty)
        {
            if (isSetProperty)
            {
                v.AreaSelectItem4 = value;
            }
            else
            {
                v.MAreaSelectItem4 = value;
            }
        }

        public static void SetAreaId(this IViewModelField v, int areaId, IArea? area, bool isSetProperty)
        {
            if (areaId == PleaseSelect.Id) return;
            area ??= Res.GetAll().FirstOrDefault(x => x.Id == areaId);
            if (area == null) return;
            if (!isSetProperty && area.Up.HasValue) v.SetAreaId(area.Up.Value, null, isSetProperty);
            switch (area.Level)
            {
                case AreaLevel.省或直辖市或特别行政区:
                    v.SetAreaItems3(GetAreasByLevel(AreaLevel.市_不包括直辖市, areaId), isSetProperty);
                    v.SetAreaNotVisible3(v.AreaItems3 == GroupEmpty, isSetProperty);
                    v.SetAreaItems4(v.AreaNotVisible3 ? GetAreasByLevel(AreaLevel.区县_县级市, areaId) : GroupEmpty, isSetProperty);
                    v.SetAreaNotVisible4(v.AreaNotVisible3 && v.AreaItems4 == GroupEmpty, isSetProperty);
                    if (!isSetProperty) v.SetAreaSelectItem2(area, isSetProperty);
                    v.SetAreaSelectItem3(v.AreaSelectItem4 = PleaseSelect, isSetProperty);
                    break;
                case AreaLevel.市_不包括直辖市:
                    v.SetAreaItems4(GetAreasByLevel(AreaLevel.区县_县级市, areaId), isSetProperty);
                    v.SetAreaNotVisible4(v.AreaItems4 == GroupEmpty, isSetProperty);
                    if (!isSetProperty) v.SetAreaSelectItem3(area, isSetProperty);
                    v.SetAreaSelectItem4(PleaseSelect, isSetProperty);
                    break;
                case AreaLevel.区县_县级市:
                    if (!isSetProperty) v.SetAreaSelectItem4(area, isSetProperty);
                    break;
            }
        }

        /// <summary>
        /// 获取当前选中的地区Id
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int? GetSelectAreaId(this IViewModel v)
        {
            if (v.AreaSelectItem4 != null && v.AreaSelectItem4.Id != PleaseSelect.Id) return v.AreaSelectItem4.Id;
            else if (v.AreaSelectItem3 != null && v.AreaSelectItem3.Id != PleaseSelect.Id) return v.AreaSelectItem3.Id;
            else if (v.AreaSelectItem2 != null && v.AreaSelectItem2.Id != PleaseSelect.Id) return v.AreaSelectItem2.Id;
            return null;
        }

        /// <summary>
        /// 初始化视图模型中的地区数据
        /// </summary>
        /// <param name="v"></param>
        public static void InitAreas(this IViewModelField v)
        {
            v.AreaItems2 = Group2;
            v.AreaItems3 = v.AreaItems4 = GroupEmpty;
            v.SetAreaSelectItem4(PleaseSelect, false);
            v.SetAreaSelectItem3(PleaseSelect, false);
            v.SetAreaSelectItem2(PleaseSelect, false);

            void SubscribeAreaSelectItem(IArea? area)
            {
                if (area != null)
                {
                    v.SelectArea(area);
                }
            }
            v.WhenAnyValue(x => x.AreaSelectItem2).Subscribe(SubscribeAreaSelectItem).AddTo(v);
            v.WhenAnyValue(x => x.AreaSelectItem3).Subscribe(SubscribeAreaSelectItem).AddTo(v);
        }

        /// <summary>
        /// 后端设置一个地区
        /// </summary>
        /// <param name="v"></param>
        /// <param name="areaId"></param>
        public static void SetAreaId(this IViewModelField v, int areaId) => v.SetAreaId(areaId, null, false);

        /// <inheritdoc cref="SetAreaId(IViewModelField, int)"/>
        public static void SetAreaId(this IViewModelField v, IArea area) => v.SetAreaId(area.Id);

        /// <summary>
        /// 前端选中一个地区
        /// </summary>
        /// <param name="v"></param>
        /// <param name="areaId"></param>
        /// <param name="area"></param>
        /// <param name="setArea"></param>
        /// <param name="isRecursionUp"></param>
        static void SelectAreaId(this IViewModelField v, int areaId, IArea? area) => v.SetAreaId(areaId, area, true);

        /// <inheritdoc cref="SelectAreaId(IViewModelField, int, IArea?)"/>
        public static void SelectAreaId(this IViewModelField v, int areaId) => v.SelectAreaId(areaId, null);

        /// <inheritdoc cref="SelectAreaId(IViewModelField, int, IArea?)"/>
        public static void SelectArea(this IViewModelField v, IArea area) => v.SelectAreaId(area.Id);

        /// <summary>
        /// 根据地区等级获取地区数据
        /// </summary>
        /// <param name="level"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        static IReadOnlyList<IArea> GetAreasByLevel(AreaLevel level, int? up = null)
        {
            var list = Res.GetAll().Where(x => x.Level == level && x.Up == up).OrderBy(x => x.Id).ToList();
            if (!list.Any()) return GroupEmpty;
            list.Insert(0, mPleaseSelect);
            return list;
        }

        public static IAreaResource<Area> Res => DI.Get<IAreaResource<Area>>();

        static readonly Area mPleaseSelect = new()
        {
            ShortName = AppResources.PleaseSelect,
        };

        /// <summary>
        /// 请选择
        /// </summary>
        public static IArea PleaseSelect => mPleaseSelect;

        static readonly Lazy<IReadOnlyList<IArea>> mGroupEmpty = new(() => new[] { PleaseSelect });

        /// <summary>
        /// 空选项组
        /// </summary>
        public static IReadOnlyList<IArea> GroupEmpty => mGroupEmpty.Value;

        static readonly Lazy<IReadOnlyList<IArea>> mGroup2 = new(() => GetAreasByLevel(AreaLevel.省或直辖市或特别行政区));

        /// <summary>
        /// 第一级地区选项组
        /// </summary>
        public static IReadOnlyList<IArea> Group2 => mGroup2.Value;
    }

    partial class
#if !__MOBILE__
        UserProfileWindowViewModel
#else
        UserProfilePageViewModel
#endif
    : AreaUIHelper.IViewModelField
    {
        IReadOnlyList<IArea>? _AreaItems2;
        public IReadOnlyList<IArea>? AreaItems2
        {
            get => _AreaItems2;
            set => this.RaiseAndSetIfChanged(ref _AreaItems2, value);
        }

        IReadOnlyList<IArea>? _AreaItems3;
        public IReadOnlyList<IArea>? AreaItems3
        {
            get => _AreaItems3;
            set => this.RaiseAndSetIfChanged(ref _AreaItems3, value);
        }

        IReadOnlyList<IArea>? _AreaItems4;
        public IReadOnlyList<IArea>? AreaItems4
        {
            get => _AreaItems4;
            set => this.RaiseAndSetIfChanged(ref _AreaItems4, value);
        }

        IArea? _AreaSelectItem2;
        public IArea? AreaSelectItem2
        {
            get => _AreaSelectItem2;
            set => this.RaiseAndSetIfChanged(ref _AreaSelectItem2, value);
        }

        IArea? _AreaSelectItem3;
        public IArea? AreaSelectItem3
        {
            get => _AreaSelectItem3;
            set => this.RaiseAndSetIfChanged(ref _AreaSelectItem3, value);
        }

        IArea? _AreaSelectItem4;
        public IArea? AreaSelectItem4
        {
            get => _AreaSelectItem4;
            set => this.RaiseAndSetIfChanged(ref _AreaSelectItem4, value);
        }

        bool _AreaNotVisible3;
        public bool AreaNotVisible3
        {
            get => _AreaNotVisible3;
            set => this.RaiseAndSetIfChanged(ref _AreaNotVisible3, value);
        }

        bool _AreaNotVisible4;
        public bool AreaNotVisible4
        {
            get => _AreaNotVisible4;
            set => this.RaiseAndSetIfChanged(ref _AreaNotVisible4, value);
        }

        IReadOnlyList<IArea>? AreaUIHelper.IViewModelField.MAreaItems3
        {
            set => _AreaItems3 = value;
        }
        IReadOnlyList<IArea>? AreaUIHelper.IViewModelField.MAreaItems4
        {
            set => _AreaItems4 = value;
        }
        IArea? AreaUIHelper.IViewModelField.MAreaSelectItem2
        {
            set => _AreaSelectItem2 = value;
        }
        IArea? AreaUIHelper.IViewModelField.MAreaSelectItem3
        {
            set => _AreaSelectItem3 = value;
        }
        IArea? AreaUIHelper.IViewModelField.MAreaSelectItem4
        {
            set => _AreaSelectItem4 = value;
        }
        bool AreaUIHelper.IViewModelField.MAreaNotVisible3
        {
            set => _AreaNotVisible3 = value;
        }
        bool AreaUIHelper.IViewModelField.MAreaNotVisible4
        {
            set => _AreaNotVisible4 = value;
        }
    }
}