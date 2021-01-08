using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Model
{
    public class IntStatInfo : StatInfo
    {
        public int OriginalValue { get; set; }
        public int IntValue { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int MaxChange { get; set; }
        public bool IncrementOnly { get; set; }
        public int DefaultValue { get; set; }
        public override object Value
        {
            get { return this.IntValue; }
            set
            {
                var b = int.TryParse((string)value, out int i);
                if (b)
                {
                    if ((this.Permission & 2) != 0 && this.IntValue != i)
                    {
                        //this.IntValue = this.IntValue;
                    }
                    else
                    {
                        this.IntValue = i;
                    }
                }
            }
        }

        public override bool IsModified
        {
            get { return this.IntValue != this.OriginalValue; }
        }
    }
}
