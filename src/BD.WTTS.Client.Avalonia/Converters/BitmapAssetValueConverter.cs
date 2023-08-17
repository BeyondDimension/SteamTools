namespace BD.WTTS.Converters;

public sealed class BitmapAssetValueConverter : ImageValueConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;
        int width = 0;
        if (parameter is int w)
        {
            width = w;
        }
        if (value is string rawUri)
        {
            if (rawUri == string.Empty) return null;
            Uri uri;
            // Allow for assembly overrides
            if (File.Exists(rawUri))
            {
                return GetDecodeBitmap(rawUri, width);
            }
            // 在列表中使用此方法性能极差!!!
            else if (String2.IsHttpUrl(rawUri))
            {
                return DownloadImage(rawUri, width);
            }
#if AVALONIA
            else if (rawUri.StartsWith("avares://"))
            {
                uri = new Uri(rawUri);
            }
#endif
            else
            {
                uri = GetResUri(rawUri);
            }
            using var asset = OpenAssets(uri);
            return GetDecodeBitmap(asset, width);
        }
        else if (value is byte[] b)
        {
            using var s = new MemoryStream(b);
            return GetDecodeBitmap(s, width);
        }
        else if (value is Stream s)
        {
            TryReset(s);
            return GetDecodeBitmap(s, width);
        }
        else if (value is ImageSource.ClipStream clipStream)
        {
            return GetDecodeBitmap(clipStream.Stream, width);
        }
        else if (value is Guid imageid)
        {
            if (imageid == default)
            {
                return null;
            }
            return DownloadImage(ImageUrlHelper.GetImageApiUrlById(imageid), width);
        }
        return this.DoNothing();
    }
}