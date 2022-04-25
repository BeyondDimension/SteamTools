namespace System.Application.Models
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
            get => FloatValue;
            set
            {
                var b = float.TryParse((string)value, out float f);
                if (b)
                {
                    if ((Permission & 2) != 0 && FloatValue != f)
                    {
                        //this.FloatValue = this.FloatValue;
                    }
                    else
                    {
                        FloatValue = f;
                    }
                }
            }
        }

        public override bool IsModified => !FloatValue.Equals(OriginalValue);
    }
}