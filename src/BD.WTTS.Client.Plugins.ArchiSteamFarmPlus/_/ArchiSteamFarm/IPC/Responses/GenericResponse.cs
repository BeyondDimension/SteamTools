//     _                _      _  ____   _                           _____
//    / \    _ __  ___ | |__  (_)/ ___| | |_  ___   __ _  _ __ ___  |  ___|__ _  _ __  _ __ ___
//   / _ \  | '__|/ __|| '_ \ | |\___ \ | __|/ _ \ / _` || '_ ` _ \ | |_  / _` || '__|| '_ ` _ \
//  / ___ \ | |  | (__ | | | || | ___) || |_|  __/| (_| || | | | | ||  _|| (_| || |   | | | | | |
// /_/   \_\|_|   \___||_| |_||_||____/  \__|\___| \__,_||_| |_| |_||_|   \__,_||_|   |_| |_| |_|
// |
// Copyright 2015-2023 Łukasz "JustArchi" Domeradzki
// Contact: JustArchi@JustArchi.net
// |
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// |
// http://www.apache.org/licenses/LICENSE-2.0
// |
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.ComponentModel.DataAnnotations;
using ArchiSteamFarm.Localization;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Responses;

public sealed class GenericResponse<T> : GenericResponse
{
    /// <summary>
    ///     The actual result of the request, if available.
    /// </summary>
    /// <remarks>
    ///     The type of the result depends on the API endpoint that you've called.
    /// </remarks>
    [JsonProperty]
    public T? Result { get; private set; }

    public GenericResponse(T? result) : base(result is not null) => Result = result;

    public GenericResponse(bool success, string? message) : base(success, message) { }

    public GenericResponse(bool success, T? result) : base(success) => Result = result;

    public GenericResponse(bool success, string? message, T? result) : base(success, message) => Result = result;

    [JsonConstructor]
    private GenericResponse() { }
}

public class GenericResponse
{
    /// <summary>
    ///     A message that describes what happened with the request, if available.
    /// </summary>
    /// <remarks>
    ///     This property will provide exact reason for majority of expected failures.
    /// </remarks>
    [JsonProperty]
    public string? Message { get; private set; }

    /// <summary>
    ///     Boolean type that specifies if the request has succeeded.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public bool Success { get; private set; }

    public GenericResponse(bool success, string? message = null)
    {
        Success = success;

        // ReSharper disable once RedundantSuppressNullableWarningExpression - required for .NET Framework
        Message = !string.IsNullOrEmpty(message) ? message! : success ? "OK" : Strings.WarningFailed;
    }

    [JsonConstructor]
    protected GenericResponse() { }
}
