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

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning disable SA1600 // Elements should be documented

using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ArchiSteamFarm.Steam.Data;

[PublicAPI]
[SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
public sealed class Confirmation
{
    [JsonProperty(PropertyName = "nonce", Required = Required.Always)]
    internal readonly ulong Nonce;

    [JsonProperty(PropertyName = "type", Required = Required.Always)]
    public EConfirmationType ConfirmationType { get; private set; }

    [JsonProperty(PropertyName = "creator_id", Required = Required.Always)]
    public ulong CreatorID { get; private set; }

    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public ulong ID { get; private set; }

    [JsonConstructor]
    private Confirmation() { }

    [UsedImplicitly]
    public static bool ShouldSerializeNonce() => false;

    [PublicAPI]
    public enum EConfirmationType : byte
    {
        Unknown,
        Generic,
        Trade,
        Market,
        PhoneNumberChange = 5,
        AccountRecovery = 6,
        ApiKeyRegistration = 9
    }
}
