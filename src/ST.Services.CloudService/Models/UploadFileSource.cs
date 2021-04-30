namespace System.Application.Models
{
    public class UploadFileSource : IUploadFileSource
    {
        public string FilePath { get; set; } = string.Empty;

        public string MIME { get; set; } = string.Empty;

        public bool IsCompressed { get; set; }

        public bool IsCache { get; set; }

        public UploadFileType UploadFileType { get; set; }

        bool IExplicitHasValue.ExplicitHasValue() => !string.IsNullOrWhiteSpace(FilePath);
    }
}