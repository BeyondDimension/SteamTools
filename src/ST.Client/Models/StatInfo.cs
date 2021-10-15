using System.ComponentModel;

namespace System.Application.Models
{
    public abstract class StatInfo
    {
        public abstract bool IsModified { get; }

        public string? Id { get; set; }

        public string? DisplayName { get; set; }

        public abstract object Value { get; set; }

        public bool IsIncrementOnly { get; set; }

        public int Permission { get; set; }

        public string Extra
        {
            get
            {
                var flags = StatFlags.None;
                flags |= IsIncrementOnly == false ? 0 : StatFlags.IncrementOnly;
                flags |= (Permission & 2) != 0 == false ? 0 : StatFlags.Protected;
                flags |= (Permission & ~2) != 0 == false ? 0 : StatFlags.UnknownPermission;
                return flags.ToString();
            }
        }
    }

    [Flags]
    public enum StatFlags
    {
        [Description("默认")]
        None = 0,

        [Description("仅增加")]
        IncrementOnly = 1 << 0,

        [Description("受保护")]
        Protected = 1 << 1,

        [Description("未知权限")]
        UnknownPermission = 1 << 2,
    }
}