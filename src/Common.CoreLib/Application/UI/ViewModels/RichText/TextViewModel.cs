//using System.Collections.Generic;
//using System.Text;
//using MPKey = MessagePack.KeyAttribute;
//using MPObj = MessagePack.MessagePackObjectAttribute;
//using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
//using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

//namespace System.Application.UI.ViewModels.RichText;

//[MPObj]
//[Serializable]
//public class TextViewModel : Serializable
//{
//    [MPKey(0)]
//    [N_JsonProperty("q")]
//    [S_JsonProperty("q")]
//    public List<SpanViewModel> Spans { get; set; } = new List<SpanViewModel>();

//    /// <summary>
//    /// android:textColor
//    /// <para>Text color.</para>
//    /// <para>May be a reference to another resource, in the form "@[+][package:]type/name" or a theme attribute in the form "?[package:]type/name".</para>
//    /// <para>May be a color value, in the form of "#rgb", "#argb", "#rrggbb", or "#aarrggbb".</para>
//    /// </summary>
//    [MPKey(1)]
//    [N_JsonProperty("w")]
//    [S_JsonProperty("w")]
//    public string? FontColor { get; set; }

//    /// <summary>
//    /// android:textSize
//    /// <para>Size of the text. Recommended dimension type for text is "sp" for scaled-pixels (example: 15sp).</para>
//    /// <para>May be a dimension value, which is a floating point number appended with a unit such as "14.5sp". Available units are: px(pixels), dp(density-independent pixels), sp(scaled pixels based on preferred font size), in (inches), and mm(millimeters).</para>
//    /// </summary>
//    [MPKey(2)]
//    [N_JsonProperty("e")]
//    [S_JsonProperty("e")]
//    public float? FontSize { get; set; }

//    /// <summary>
//    /// android:lineSpacingMultiplier
//    /// <para>Extra spacing between lines of text, as a multiplier.The value will not be applied for the last line of text.</para>
//    /// <para>May be a floating point value, such as "1.2".</para>
//    /// </summary>
//    [MPKey(3)]
//    [N_JsonProperty("r")]
//    [S_JsonProperty("r")]
//    public float? LineSpacingMultiplier { get; set; }

//    /// <summary>
//    /// android:lineSpacingExtra
//    /// <para>Extra spacing between lines of text.The value will not be applied for the last line of text.</para>
//    /// <para>May be a dimension value, which is a floating point number appended with a unit such as "14.5sp". Available units are: px (pixels), dp (density-independent pixels), sp(scaled pixels based on preferred font size), in (inches), and mm(millimeters).</para>
//    /// </summary>
//    [MPKey(4)]
//    [N_JsonProperty("t")]
//    [S_JsonProperty("t")]
//    public float? LineSpacingExtra { get; set; }

//    public override string ToString()
//    {
//        if (Spans.Any_Nullable())
//        {
//            var @string = new StringBuilder();
//            foreach (var item in Spans)
//            {
//                if (item.NewLine)
//                {
//                    @string.AppendLine(item.Text);
//                }
//                else
//                {
//                    @string.Append(item.Text);
//                }
//            }
//            return @string.ToString();
//        }
//        return string.Empty;
//    }
//}