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
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Responses;

public sealed class TypeProperties
{
    /// <summary>
    ///     Base type of given type, if available.
    /// </summary>
    /// <remarks>
    ///     This can be used for determining how the body of the response should be interpreted.
    /// </remarks>
    [JsonProperty]
    public string? BaseType { get; private set; }

    /// <summary>
    ///     Custom attributes of given type, if available.
    /// </summary>
    /// <remarks>
    ///     This can be used for determining main enum type if <see cref="BaseType" /> is <see cref="Enum" />.
    /// </remarks>
    [JsonProperty]
    public HashSet<string>? CustomAttributes { get; private set; }

    /// <summary>
    ///     Underlying type of given type, if available.
    /// </summary>
    /// <remarks>
    ///     This can be used for determining underlying enum type if <see cref="BaseType" /> is <see cref="Enum" />.
    /// </remarks>
    [JsonProperty]
    public string? UnderlyingType { get; private set; }

    [JsonConstructor]
    public TypeProperties()
    {

    }
}
