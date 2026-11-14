using AppStrings = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Models;

public class ArchiSteamFarmCommand
{
    private const string ExecuteCommandChar = "!";

    public static IReadOnlyCollection<ArchiSteamFarmCommand> Commands => new ArchiSteamFarmCommand[]
    {
new("2fa","2fa [Bots]",AppStrings.L10nAsf_Text040),
new("2fano","2fano [Bots]",AppStrings.L10nAsf_Text041),
new("2faok","2faok [Bots]",AppStrings.L10nAsf_Text042),
new("addlicense","addlicense [Bots] <Licenses>",AppStrings.L10nAsf_Text043),
new("balance","balance [Bots]",AppStrings.L10nAsf_Text044),
new("bgr","bgr [Bots]",AppStrings.L10nAsf_Text045),
new("encrypt","encrypt <encryptionMethod> <stringToEncrypt>",AppStrings.L10nAsf_Text046),
new("farm","farm [Bots]",AppStrings.L10nAsf_Text047),
new("fb","fb [Bots]",AppStrings.L10nAsf_Text048),
new("fbadd","fbadd [Bots] <AppIDs>",AppStrings.L10nAsf_Text049),
new("fbrm","fbrm [Bots] <AppIDs>",AppStrings.L10nAsf_Text050),
new("fq","fq [Bots]",AppStrings.L10nAsf_Text051),
new("fqadd","fqadd [Bots] <AppIDs>",AppStrings.L10nAsf_Text052),
new("fqrm","fqrm [Bots] <AppIDs>",AppStrings.L10nAsf_Text053),
new("hash","hash <hashingMethod> <stringToHash>",AppStrings.L10nAsf_Text054),
new("help","help",AppStrings.L10nAsf_Text055),
new("input","input [Bots] <Type> <Value>",AppStrings.L10nAsf_Text056),
new("level","level [Bots]",AppStrings.L10nAsf_Text057),
new("loot","loot [Bots]",AppStrings.L10nAsf_Text058),
new("loot@","loot@ [Bots] <AppIDs>",AppStrings.L10nAsf_Text059),
new("loot%","loot% [Bots] <AppIDs>",AppStrings.L10nAsf_Text060),
new("loot^","loot^ [Bots] <AppID> <ContextID>",AppStrings.L10nAsf_Text061),
new("mab","mab [Bots]",AppStrings.L10nAsf_Text062),
new("mabadd","mabadd [Bots] <AppIDs>",AppStrings.L10nAsf_Text063),
new("mabrm","mabrm [Bots] <AppIDs>",AppStrings.L10nAsf_Text064),
new("nickname","nickname [Bots] <Nickname>",AppStrings.L10nAsf_Text065),
new("owns","owns [Bots] <Games>",AppStrings.L10nAsf_Text066),
new("pause","pause [Bots]",AppStrings.L10nAsf_Text067),
new("pause~","pause~ [Bots]",AppStrings.L10nAsf_Text068),
new("pause&","pause& [Bots] <Seconds>",AppStrings.L10nAsf_Text069),
new("play","play [Bots] <AppIDs,GameName>",AppStrings.L10nAsf_Text070),
new("points","points [Bots]",AppStrings.L10nAsf_Text071),
new("privacy","privacy [Bots] <Settings>",AppStrings.L10nAsf_Text072),
new("redeem","redeem [Bots] <Keys>",AppStrings.L10nAsf_Text073),
new("redeem^","redeem^ [Bots] <Modes> <Keys>",AppStrings.L10nAsf_Text074),
new("reset","reset [Bots]",AppStrings.L10nAsf_Text075),
new("resume","resume [Bots]",AppStrings.L10nAsf_Text076),
new("start","start [Bots]",AppStrings.L10nAsf_Text077),
new("stats","stats",AppStrings.L10nAsf_Text078),
new("status","status [Bots]",AppStrings.L10nAsf_Text079),
new("stop","stop [Bots]",AppStrings.L10nAsf_Text080),
new("tb","tb [Bots]",AppStrings.L10nAsf_Text081),
new("tbadd","tbadd [Bots] <SteamIDs64>",AppStrings.L10nAsf_Text082),
new("tbrm","tbrm [Bots] <SteamIDs64>",AppStrings.L10nAsf_Text083),
new("transfer","transfer [Bots] <TargetBot>",AppStrings.L10nAsf_Text084),
new("transfer@","transfer@ [Bots] <AppIDs> <TargetBot>",AppStrings.L10nAsf_Text085),
new("transfer%","transfer% [Bots] <AppIDs> <TargetBot>",AppStrings.L10nAsf_Text086),
new("transfer^","transfer^ [Bots] <AppID> <ContextID> <TargetBot>",AppStrings.L10nAsf_Text087),
new("unpack","unpack [Bots]",AppStrings.L10nAsf_Text088),
new("version","version",AppStrings.L10nAsf_Text089),
    };


    public ArchiSteamFarmCommand(string command, string? text = null, string? description = null)
    {
        Command = ExecuteCommandChar + command;
        CommandText = ExecuteCommandChar + text;
        Description = description;
    }

    public string Command { get; }

    public string? CommandText { get; }

    //public int Access { get; }


    public string? Description { get; }
}
