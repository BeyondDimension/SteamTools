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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Responses;

public sealed class TypeResponse
{
    /// <summary>
    ///     A string-string map representing a decomposition of given type.
    /// </summary>
    /// <remarks>
    ///     The actual structure of this field depends on the type that was requested. You can determine that type based on <see cref="Properties" /> metadata.
    ///     For enums, keys are friendly names while values are underlying values of those names.
    ///     For objects, keys are non-private fields and properties, while values are underlying types of those.
    /// </remarks>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public Dictionary<string, string>? Body { get; private set; }

    /// <summary>
    ///     Metadata of given type.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public TypeProperties? Properties { get; private set; }

    [JsonConstructor]
    public TypeResponse()
    {

    }
}
