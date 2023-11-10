using Newtonsoft.Json;

namespace BD.WTTS.Services.Implementation;

partial class ArchiSteamFarmWebApiServiceImpl // JsonSerializer(Newtonsoft.Json)
{
    readonly JsonSerializer serializer = GetJsonSerializer();

    static JsonSerializer GetJsonSerializer()
    {
        // https://github.com/BeyondDimension/ArchiSteamFarm/blob/v5.4.9.3/ArchiSteamFarm/IPC/Startup.cs#L350-L364
        // https://github.com/dotnet/aspnetcore/blob/v7.0.11/src/Mvc/Mvc.NewtonsoftJson/src/JsonSerializerSettingsProvider.cs

        var settings = new JsonSerializerSettings();
        // Fix default contract resolver to use original names and not a camel case
        settings.ContractResolver = new DefaultContractResolver();

        var serializer = JsonSerializer.Create(settings);
        return serializer;
    }

    HttpContent Serialize(object? value, Type? objectType = null)
    {
        var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
        using var jsonWriter = new JsonTextWriter(streamWriter);
        serializer.Serialize(jsonWriter, value, objectType);
        jsonWriter.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        var content = new StreamContent(stream);
        content.Headers.TryAddWithoutValidation("Content-Type", "application/json;charset=utf-8");
        return content;
    }

    async Task<object?> DeserializeAsync(
        HttpContent content,
        Type? objectType,
        CancellationToken cancellationToken = default)
    {
        using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        using var jsonReader = new JsonTextReader(streamReader);
        var value = serializer.Deserialize(jsonReader, objectType);
        return value;
    }
}
