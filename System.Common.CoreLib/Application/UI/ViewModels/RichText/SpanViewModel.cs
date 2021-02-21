//using MPKey = MessagePack.KeyAttribute;
//using MPObj = MessagePack.MessagePackObjectAttribute;
//using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
//using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

//namespace System.Application.UI.ViewModels.RichText
//{
//    [MPObj]
//    [Serializable]
//    public class SpanViewModel : Serializable
//    {
//        /// <summary>
//        /// 文本内容
//        /// </summary>
//        [MPKey(0)]
//        [N_JsonProperty("q")]
//        [S_JsonProperty("q")]
//        public string? Text { get; set; }

//        /// <summary>
//        /// 字体样式
//        /// </summary>
//        [MPKey(1)]
//        [N_JsonProperty("w")]
//        [S_JsonProperty("w")]
//        public FontStyle FontStyle { get; set; }

//        /// <summary>
//        /// 字体大小
//        /// </summary>
//        [MPKey(2)]
//        [N_JsonProperty("e")]
//        [S_JsonProperty("e")]
//        public float? FontSize { get; set; }

//        /// <summary>
//        /// 底部外边距
//        /// </summary>
//        [MPKey(3)]
//        [N_JsonProperty("r")]
//        [S_JsonProperty("r")]
//        public float? BottomMargin { get; set; }

//        /// <summary>
//        /// 是否换行
//        /// </summary>
//        [MPKey(4)]
//        [N_JsonProperty("t")]
//        [S_JsonProperty("t")]
//        public bool NewLine { get; set; }

//        /// <summary>
//        /// 文本对齐方式
//        /// </summary>
//        [MPKey(5)]
//        [N_JsonProperty("y")]
//        [S_JsonProperty("y")]
//        public Alignment TextAlignment { get; set; }

//        /// <summary>
//        /// 字体颜色
//        /// </summary>
//        [MPKey(6)]
//        [N_JsonProperty("u")]
//        [S_JsonProperty("u")]
//        public string? FontColor { get; set; }
//    }
//}