using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Model
{
    public class AchievementInfo
    {
        public int AppId { get; set; }
        public string Id { get; set; }
        public float Percent { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconNormal { get; set; }
        public string IconLocked { get; set; }
        public bool IsHidden { get; set; }
        public bool IsAchieved { get; set; }
        public bool IsChecked { get; set; }
        public int UnlockTimeUnix { get; set; }
        public DateTime UnlockTime { get; set; }
        public string IconUrl => IsAchieved ? string.Format(Const.STEAMAPP_ICON_URL, AppId, IconNormal) : string.Format(Const.STEAMAPP_ICON_URL, AppId, IconLocked);
        public int Permission { get; set; }
        public bool IsProtection => (Permission & 3) != 0;
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{0}: {1}",
                this.Name ?? this.Id ?? base.ToString(),
                this.Permission);
        }
    }
}
