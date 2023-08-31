// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Microsoft.Win32;
global using System;
global using System.Collections;
#if !NET35
global using System.Collections.Concurrent;
#endif
global using System.Collections.ObjectModel;
global using System.ComponentModel;
#if !NETFRAMEWORK && !APP_HOST
global using System.Collections.Immutable;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
#endif
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Web;
global using System.IO;
#if !NETFRAMEWORK && !APP_HOST
global using System.Buffers;
global using System.IO.Pipes;
global using PipeOptions = System.IO.Pipes.PipeOptions;
#if !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using System.IO.Pipelines;
global using PipelineOptions = System.IO.Pipelines.PipeOptions;
#endif
#endif
global using System.IO.Compression;
#if !NETFRAMEWORK && !APP_HOST && !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using System.IO.FileFormats;
#endif
global using System.Linq;
global using System.Linq.Expressions;
global using System.Net;
global using System.Net.Security;
#if !NETFRAMEWORK && !APP_HOST && !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Net.Http.Client;
#endif
global using System.Net.Sockets;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.Serialization;
global using System.Security;
#if !NETFRAMEWORK
global using System.Security.Claims;
#endif
global using System.Security.Cryptography;
global using System.Security.Principal;
global using System.Text;
#if !NETFRAMEWORK && !APP_HOST && !__NOT_HAVE_S_JSON__
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization.Metadata;
global using System.Text.Unicode;
#endif
global using System.Text.RegularExpressions;
global using System.Runtime;
#if !NETFRAMEWORK && !APP_HOST && !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using DeploymentMode = System.Runtime.DeploymentMode;
global using System.Runtime.Devices;
#endif
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;
global using System.Runtime.Serialization.Formatters;

#if !NETFRAMEWORK && !APP_HOST && !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

#if WINDOWS && !APP_HOST && !APP_REVERSE_PROXY && !LIB_CLIENT_IPC && !__NOT_IMPORT_COMMON__
global using System.Management;
#endif

#if !NONE_UI && (WINDOWS || APP_HOST) && !__NOT_IMPORT_COMMON__
global using WPFMessageBox = MS.Win32.MessageBox;
global using WPFMessageBoxButton = MS.Win32.MessageBoxButton;
global using WPFMessageBoxImage = MS.Win32.MessageBoxImage;
global using WPFMessageBoxResult = MS.Win32.MessageBoxResult;
#endif

global using System.Xml;
global using System.Xml.Serialization;
global using System.Security.Cryptography.X509Certificates;
global using IPAddress = System.Net.IPAddress;
#if !NETFRAMEWORK && !APP_HOST && !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using Ioc = System.Ioc;
global using DateTimeFormat = System.DateTimeFormat;
global using SerializationDateTimeFormat = System.Runtime.Serialization.DateTimeFormat;
global using HttpMethod = System.Net.Http.HttpMethod;
#endif

#if !NETFRAMEWORK && !APP_HOST && !NETFRAMEWORK_SINGLE_FILE && !RES_PROJ && !__NOT_IMPORT_COMMON__
global using HttpHandlerCategory = System.Net.Http.Client.HttpHandlerCategory;
global using IHttpClientFactory = System.Net.Http.Client.IHttpClientFactory;
#endif

#if !APP_HOST
#if ANDROID
global using HttpHandlerType = Xamarin.Android.Net.AndroidMessageHandler;
#elif IOS || MACCATALYST
global using HttpHandlerType = System.Net.Http.NSUrlSessionHandler;
#elif NETFRAMEWORK
global using HttpHandlerType = System.Net.Http.HttpClientHandler;
#else
global using HttpHandlerType = System.Net.Http.SocketsHttpHandler;
#endif
#endif