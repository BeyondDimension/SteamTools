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

using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Responses;

public sealed class GitHubReleaseResponse
{
    /// <summary>
    ///     Changelog of the release rendered in HTML.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public string ChangelogHTML { get; private set; } = "";

    /// <summary>
    ///     Date of the release.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public DateTime ReleasedAt { get; private set; }

    /// <summary>
    ///     Boolean value that specifies whether the build is stable or not (pre-release).
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public bool Stable { get; private set; }

    /// <summary>
    ///     Version of the release.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public string Version { get; private set; } = "";

    [JsonConstructor]
    public GitHubReleaseResponse()
    {

    }
}
