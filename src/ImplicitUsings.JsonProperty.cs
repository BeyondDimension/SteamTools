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
#endif