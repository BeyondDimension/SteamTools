using ReactiveUI;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class AuthorizedDevice : ReactiveObject
    {
        public const long UndefinedId = 76561197960265728;
        public AuthorizedDevice(){}
        public AuthorizedDevice(string vdfString) :this(){

            OriginVdfString = vdfString;
        }
        public string ProfileUrl => string.Format(STEAM_PROFILES_URL, SteamId64_Int);
        public bool First { get; set; }
        public bool End { get; set; }
        public int Index { get; set; }
        public long SteamId3_Int { get; set; }
        public long SteamId64_Int => UndefinedId + SteamId3_Int;
        private string? _OnlineState;
        public string? OnlineState
        {
            get => _OnlineState;
            set => this.RaiseAndSetIfChanged(ref _OnlineState, value);
        }
        private string? _Remark;
        /// <summary>
        /// 备注
        /// </summary> 
        public string? Remark
        {
            get => _Remark;
            set => this.RaiseAndSetIfChanged(ref _Remark, value);
        }
        public string? SteamID { get; set; }
        public string? ShowName { get; set; }
        public SteamMiniProfile? MiniProfile { get; set; }
        
        public string? SteamNickName { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string? AccountName { get; set; }

        public long Timeused { get; set; }
        public DateTime TimeusedTime => Timeused.ToDateTimeS();



        public string? Description { get; set; }

        public string? Tokenid { get; set; }

        public string AvatarIcon { get; set; }
        public string AvatarMedium { get; set; }

        private Task<string?>? _AvatarStream; 
        public Task<string?>? AvatarStream
        {
            get => _AvatarStream;
            set => this.RaiseAndSetIfChanged(ref _AvatarStream, value);
        }

        public string? OriginVdfString { get; set; }
        public string CurrentVdfString =>
           "\t\t\"" + SteamId3_Int + "\"\n\t\t{\n" +
           "\t\t\t\"timeused\"\t\t\"" + Timeused + "\"\n" +
           "\t\t\t\"description\"\t\t\"" + Description + "\"\n" +
           "\t\t\t\"tokenid\"\t\t\"" + Tokenid + "\"\n\t\t}";
    }
}