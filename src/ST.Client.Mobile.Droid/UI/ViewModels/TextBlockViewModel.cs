using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

namespace System.Application.UI.ViewModels
{
    [MPObj]
    public class TextBlockViewModel
    {
        [MPKey(0)]
        public string? Title { get; set; }

        [MPKey(1)]
        public string? Content { get; set; }

        [MPKey(2)]
        public ContentSourceEnum ContentSource { get; set; }

        public enum ContentSourceEnum
        {
            None,
            OpenSourceLibrary,
        }
    }
}