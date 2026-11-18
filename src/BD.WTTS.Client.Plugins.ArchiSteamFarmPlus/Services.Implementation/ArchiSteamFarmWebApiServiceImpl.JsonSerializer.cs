namespace BD.WTTS.Services.Implementation;

partial class ArchiSteamFarmWebApiServiceImpl
{
    static HttpContent Serialize(object? value, Type objectType)
    {
        var stream = new MemoryStream();
        try
        {
            System.Text.Json.JsonSerializer.Serialize(stream, value, ArchiSteamFarmJson.TypeInfo(objectType));
            stream.Position = 0;
            var content = new StreamContent(stream);
            content.Headers.TryAddWithoutValidation("Content-Type", "application/json;charset=utf-8");
            return content;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    static async Task<T?> DeserializeAsync<T>(
        HttpContent content,
        CancellationToken cancellationToken = default) where T : class
    {
        using var stream = await content.ReadAsStreamAsync(cancellationToken);
        return await ArchiSteamFarmJson.DeserializeResponseAsync<T>(stream, cancellationToken);
    }
}
