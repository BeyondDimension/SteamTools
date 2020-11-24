using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Model
{
    public class FloatStatInfo : StatInfo
    {
        public float OriginalValue { get; set; }
        public float FloatValue { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float MaxChange { get; set; }
        public bool IncrementOnly { get; set; }
        public float DefaultValue { get; set; }

        public override object Value
        {
            get { return this.FloatValue; }
            set
            {
                var b = float.TryParse((string)value, out float f);
                if (b)
                {
                    if ((this.Permission & 2) != 0 && this.FloatValue != f)
                    {
                        //this.FloatValue = this.FloatValue;
                    }
                    else
                    {
                        this.FloatValue = f;
                    }
                }
            }
        }

        public override bool IsModified
        {
            get { return this.FloatValue.Equals(this.OriginalValue) == false; }
        }
    }
}
