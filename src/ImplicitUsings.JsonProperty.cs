// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

//#if __HAVE_N_JSON__
global using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
global using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
//#endif
#if !__NOT_HAVE_S_JSON__
global using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
global using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Text.Json.Serialization.Metadata;
global using System.Text.Encodings.Web;
global using System.Text.Unicode;

global using SystemTextJsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
global using SystemTextJsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
global using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;
global using SystemTextJsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
global using SystemTextJsonIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition;
global using SystemTextJsonConstructor = System.Text.Json.Serialization.JsonConstructorAttribute;
global using SystemTextJsonObject = System.Text.Json.Nodes.JsonObject;
global using SystemTextJsonPropertyStruct = System.Text.Json.JsonProperty;
global using SystemTextJsonExtensionData = System.Text.Json.Serialization.JsonExtensionDataAttribute;
global using SystemTextJsonSerializable = System.Text.Json.Serialization.JsonSerializableAttribute;
global using SystemTextJsonSerializerContext = System.Text.Json.Serialization.JsonSerializerContext;
global using SystemTextJsonConverter = System.Text.Json.Serialization.JsonConverterAttribute;
global using SystemTextJsonConverterBase = System.Text.Json.Serialization.JsonConverter;
#endif

global using Newtonsoft.Json;
global using Newtonsoft.Json.Converters;
global using Newtonsoft.Json.Serialization;
global using Newtonsoft.Json.Linq;
global using NewtonsoftJsonConverter = Newtonsoft.Json.JsonConverter;
global using NewtonsoftJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
global using NewtonsoftJsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
global using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;
global using NewtonsoftJsonFormatting = Newtonsoft.Json.Formatting;
global using NewtonsoftJsonConvert = Newtonsoft.Json.JsonConvert;
global using NewtonsoftJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;
global using NewtonsoftJsonConstructor = Newtonsoft.Json.JsonConstructorAttribute;
global using NewtonsoftJsonObject = Newtonsoft.Json.Linq.JObject;
global using NewtonsoftJsonPropertyClass = Newtonsoft.Json.Serialization.JsonProperty;
global using NewtonsoftJsonExtensionData = Newtonsoft.Json.JsonExtensionDataAttribute;