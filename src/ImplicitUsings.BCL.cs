// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Microsoft.Win32;
global using System.Collections.Concurrent;
#if !NETFRAMEWORK
global using System.Collections.Immutable;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
#endif
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.IO.Compression;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Net;
global using System.Net.Security;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Net.Sockets;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.Serialization;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.Unicode;
global using System.Text.RegularExpressions;
global using System.Runtime.Devices;
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;

#if MVVM_VM
global using DynamicData;
global using DynamicData.Binding;
global using System.Collections.ObjectModel;
global using System.Reactive.Linq;
#endif

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

#if WINDOWS7_0_OR_GREATER
global using MessageBox = MS.Win32.MessageBox;
#endif