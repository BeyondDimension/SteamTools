// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Microsoft.Net.Http.Headers;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Hosting;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Cryptography.KeyDerivation;
global using Microsoft.AspNetCore.Connections.Features;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.AspNetCore.Http.Extensions;
global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Server.Kestrel.Https;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using Microsoft.AspNetCore.Mvc.ApplicationParts;
global using Microsoft.AspNetCore.Mvc.Controllers;
global using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
global using Microsoft.AspNetCore.Mvc.ViewComponents;
global using Microsoft.AspNetCore.Mvc.Formatters;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.AspNetCore.Authorization.Policy;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.AspNetCore.StaticFiles;
global using Microsoft.Extensions.WebEncoders;
//global using IOFile = System.IO.File;

//#if !PROJ_TYPE_MODELS
//global using StackExchange.Redis;
//#endif

#if NET7_0_OR_GREATER && _IMPORT_MS_OPENAPI_MODELS_ && DEBUG
global using Microsoft.OpenApi.Models;
#endif

global using Microsoft.AspNetCore.Connections;
global using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
global using AspNetCoreHttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;