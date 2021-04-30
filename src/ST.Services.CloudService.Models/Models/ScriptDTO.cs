using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using static System.Application.Services.CloudService.Constants;
#if MVVM_VM
using ReactiveUI;
#endif

namespace System.Application.Models
{
    /// <summary>
    /// 脚本
    /// </summary>
    [MPObj]
    public class ScriptDTO
#if MVVM_VM
        : ReactiveObject
#endif
    {
#if MVVM_VM
        const string HomepageURL = "HomepageURL";
        const string DownloadURL = "DownloadURL";
        const string UpdateURL = "UpdateURL";
        const string Exclude = "Exclude";
        const string Grant = "Grant";
        const string Require = "Require";
        const string Include = "Include";
        const string DescRegex = @"(?<={0})[\s\S]*?(?=\n)";

        public ScriptDTO()
        {
            this.WhenAnyValue(x => x.IsUpdate, c => c.IsLoading, v => v.IsExist)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(DownloadLoading));
                    this.RaisePropertyChanged(nameof(DownloadButtonLoading));
                });
        }

        public static bool TryParse(string path, [NotNullWhen(true)] out ScriptDTO? proxyScript)
        {
            var content = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(content))
            {
                var userScript = content.Substring("==UserScript==", "==/UserScript==");
                if (!string.IsNullOrEmpty(userScript))
                {
                    var script = new ScriptDTO
                    {
                        FilePath = path,
                        Content = content.Replace("</script>", "<\\/script>"),
                        //Content = content.Replace("</script>", "<\\/script>").Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", ""),
                        Name = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Name)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        //NameSpace = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(NameSpace)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray(),
                        Version = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Version)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        Description = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Description)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        Author = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Author)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        SourceLink = Regex.Match(userScript, string.Format(DescRegex, $"@{HomepageURL}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        //SupportURL = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(SupportURL)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        DownloadLink = Regex.Match(userScript, string.Format(DescRegex, $"@{DownloadURL}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        UpdateLink = Regex.Match(userScript, string.Format(DescRegex, $"@{UpdateURL}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        ExcludeDomainNames = string.Join(GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{Exclude}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true)),
                        DependentGreasyForkFunction = Regex.Matches(userScript, string.Format(DescRegex, $"@{Grant}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).Any_Nullable(),
                        RequiredJs = string.Join(GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{Require}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true)),
                    };
                    var matchs = string.Join(GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Match)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true));
                    var Includes = string.Join(GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{Include}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true));
                    script.MatchDomainNames = string.IsNullOrEmpty(matchs) ? Includes : matchs;
                    var enable = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Enable)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true);

                    script.Enable = bool.TryParse(enable, out var e) && e;
                    proxyScript = script;
                    return true;
                }
            }
            proxyScript = null;
            return false;
        }

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool IsBasics { get; set; } = false;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public int LocalId { get; set; }

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string FilePath { get; set; } = string.Empty;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string CachePath { get; set; } = string.Empty;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string FileName => FilePath ?? Path.GetFileName(FilePath);

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string Content { get; set; } = string.Empty;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string? JsPathUrl { get; set; }

        private bool _IsUpdate;
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool IsUpdate
        {
            get => _IsUpdate;
            set => this.RaiseAndSetIfChanged(ref _IsUpdate, value);
        }

        private bool _IsExist;
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool IsExist
        {
            get => _IsExist;
            set => this.RaiseAndSetIfChanged(ref _IsExist, value);
        }

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool DownloadLoading => !IsUpdate && IsLoading;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool DownloadButtonLoading => !IsExist && !DownloadLoading;

        private bool _IsLoading;
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string? NewVersion { get; set; }

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public DateTimeOffset? UpdateTime { get; set; }

#endif
        /// <summary>
        /// 显示名称
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public string Name
#if MVVM_VM
        {
            get => _Name;
            set => this.RaiseAndSetIfChanged(ref _Name, value);
        }
        string _Name
#else
        { get; set; }
#endif
         = string.Empty;

        /// <summary>
        /// 脚本作者
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string Version
#if MVVM_VM
        {
            get => _Version;
            set => this.RaiseAndSetIfChanged(ref _Version, value);
        }
        string _Version
#else
        { get; set; }
#endif
        = string.Empty;

        /// <summary>
        /// 来源地址
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public string SourceLink { get; set; } = string.Empty;

        /// <summary>
        /// 下载地址
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public string DownloadLink { get; set; } = string.Empty;

        /// <summary>
        /// 更新地址
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public string UpdateLink
#if MVVM_VM
        {
            get => _UpdateLink;
            set => this.RaiseAndSetIfChanged(ref _UpdateLink, value);
        }
        string _UpdateLink
#else
        { get; set; }
#endif
        = string.Empty;

        /// <summary>
        /// 说明
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 脚本匹配域名，分号分割多个
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        public string MatchDomainNames { get; set; } = string.Empty;

        /// <summary>
        /// 是否默认启用
        /// </summary>
        [MPKey(8)]
        [N_JsonProperty("8")]
        [S_JsonProperty("8")]
        public bool Enable
#if MVVM_VM
        {
            get => mEnable;
            set => this.RaiseAndSetIfChanged(ref mEnable, value);
        }
        bool mEnable;
#else
        { get; set; }
#endif
        string? mMatchDomainNames;
        string[]? mMatchDomainNamesArray;
        readonly object mMatchDomainNamesArrayLock = new();

        /// <inheritdoc cref="MatchDomainNames"/>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string[] MatchDomainNamesArray
            => GetSplitValues(mMatchDomainNamesArrayLock, MatchDomainNames, ref mMatchDomainNames, ref mMatchDomainNamesArray);

        /// <summary>
        /// 脚本显示图标
        /// </summary>
        [MPKey(9)]
        [N_JsonProperty("9")]
        [S_JsonProperty("9")]
        public string? Icon { get; set; }

        /// <summary>
        /// 是否依赖油猴函数
        /// </summary>
        [MPKey(10)]
        [N_JsonProperty("10")]
        [S_JsonProperty("10")]
        public bool DependentGreasyForkFunction { get; set; }

        /// <summary>
        /// 脚本排除域名，分号分割多个
        /// </summary>
        [MPKey(11)]
        [N_JsonProperty("11")]
        [S_JsonProperty("11")]
        public string? ExcludeDomainNames { get; set; }

        string? mExcludeDomainNames;
        string[]? mExcludeDomainNamesArray;
        readonly object mExcludeDomainNamesArrayLock = new();

        /// <inheritdoc cref="ExcludeDomainNames"/>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string[]? ExcludeDomainNamesArray
            => ExcludeDomainNames == null ? null : GetSplitValues(mExcludeDomainNamesArrayLock, ExcludeDomainNames, ref mExcludeDomainNames, ref mExcludeDomainNamesArray);

        [MPKey(12)]
        [N_JsonProperty("12")]
        [S_JsonProperty("12")]
        public List<Guid>? AccelerateProjects { get; set; }

        /// <summary>
        /// 依赖外部JS，分号分割多个
        /// </summary>
        [MPKey(13)]
        [N_JsonProperty("13")]
        [S_JsonProperty("13")]
        public string? RequiredJs { get; set; }

        string? mRequiredJs;
        string[]? mRequiredJsArray;
        readonly object mRequiredJsArrayLock = new();

        /// <inheritdoc cref="RequiredJs"/>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string[]? RequiredJsArray
            => RequiredJs == null ? null : GetSplitValues(mRequiredJsArrayLock, RequiredJs, ref mRequiredJs, ref mRequiredJsArray);

        [MPKey(14)]
        [N_JsonProperty("14")]
        [S_JsonProperty("14")]
        public Guid? Id { get; set; }

        [MPKey(15)]
        [N_JsonProperty("15")]
        [S_JsonProperty("15")]
        public int Order { get; set; }

        [MPKey(16)]
        [N_JsonProperty("16")]
        [S_JsonProperty("16")]
        public bool IsBuild { get; set; } = true;
    }
}