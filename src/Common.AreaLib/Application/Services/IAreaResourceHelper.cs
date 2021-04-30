using System.Application.Entities;
using System.Linq;

namespace System.Application.Services
{
    internal interface IAreaResourceHelper<TArea> : IAreaResource<TArea> where TArea : class, IArea
    {
        /// <summary>
        /// 获取二级地区，如果获取不到则返回自身
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public TArea GetSecondaryOrThis(TArea area)
        {
            if (area.Level == AreaLevel.区县_县级市 && area.Up.HasValue)
            {
                var all = GetAll();
                var area_up_lever = all.FirstOrDefault(x => x.Id == area.Up.Value);
                if (area_up_lever == null)
                {
                    var up_lever_string = area.Up.Value.ToString();
                    if (up_lever_string.Length > 2)
                    {
                        var up_lever_int32 = (up_lever_string.Substring(0, 2) + "0000").TryParseInt32();
                        if (up_lever_int32.HasValue)
                        {
                            area_up_lever = all.FirstOrDefault(x => x.Id == up_lever_int32.Value);
                        }
                    }
                }

                if (area_up_lever != null)
                {
                    return area_up_lever;
                }
            }
            return area;
        }

        /// <summary>
        /// 获取完整名称，例如[湖南 长沙]
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public string GetFullName(TArea area)
        {
            if (area.Up.HasValue)
            {
                var entity = GetAll().FirstOrDefault(x => x.Id == area.Up.Value);
                if (entity != default)
                {
                    return $"{entity} {area}";
                }
            }
            return AreaExtensions.ToString(area);
        }
    }
}