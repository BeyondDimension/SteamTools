using System.Application.Entities;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class AreaExtensions
    {
        internal static string ToString(IArea area)
        {
            if (!string.IsNullOrWhiteSpace(area.ShortName)) return area.ShortName;
            if (!string.IsNullOrWhiteSpace(area.Name)) return area.Name;
            return area.Id.ToString();
        }

        /// <inheritdoc cref="IAreaResource{TArea}.GetSecondaryOrThis(TArea)"/>
        public static TArea GetSecondaryOrThis<TArea>(this TArea area) where TArea : class, IArea
        {
            var s = DI.Get<IAreaResourceHelper<TArea>>();
            return s.GetSecondaryOrThis(area);
        }

        /// <inheritdoc cref="IAreaResource{TArea}.GetFullName(TArea)"/>
        public static string GetFullName<TArea>(this TArea area) where TArea : class, IArea
        {
            var s = DI.Get<IAreaResourceHelper<TArea>>();
            return s.GetFullName(area);
        }
    }
}