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

using ArchiSteamFarm.Storage;
using STJ = System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Responses;

public sealed class ASFResponse
{
    /// <summary>
    ///     ASF's build variant.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public string BuildVariant { get; private set; } = "";

    /// <summary>
    ///     A value specifying whether this variant of ASF is capable of auto-update.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public bool CanUpdate { get; private set; }

    /// <summary>
    ///     Currently loaded ASF's global config.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public GlobalConfig? GlobalConfig { get; private set; }

    /// <summary>
    ///     Current amount of managed memory being used by the process, in kilobytes.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public uint MemoryUsage { get; private set; }

    /// <summary>
    ///     Start date of the process.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public DateTime ProcessStartTime { get; private set; }

    /// <summary>
    ///     Boolean value specifying whether ASF has been started with a --service parameter.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public bool Service { get; private set; }

    /// <summary>
    ///     ASF version of currently running binary.
    /// </summary>
    [STJ.JsonInclude]
    [STJ.JsonRequired]
    [Required]
    public Version? Version { get; private set; }

    [STJ.JsonConstructor]
    public ASFResponse()
    {

    }
}
