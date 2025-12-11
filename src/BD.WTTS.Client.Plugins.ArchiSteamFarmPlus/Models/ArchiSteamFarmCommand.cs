namespace BD.WTTS.Models;

public class ArchiSteamFarmCommand
{
    private const string ExecuteCommandChar = "!";

    public static readonly Lazy<IReadOnlyCollection<ArchiSteamFarmCommand>> Commands = new(new ArchiSteamFarmCommand[]
    {
new("2fa","2fa [Bots]","为指定机器人生成临时的​两步验证​令牌。"),
new("2fano","2fano [Bots]","为指定机器人拒绝所有等待操作的​两步验证​交易确认。"),
new("2faok","2faok [Bots]","为指定机器人接受所有等待操作的​两步验证​交易确认。"),
new("addlicense","addlicense [Bots] <Licenses>","为指定机器人激活给定的 Licenses （许可），该参数解释详见​下文。"),
new("balance","balance [Bots]","显示指定机器人的 Steam 钱包余额。"),
new("bgr","bgr [Bots]","显示指定机器人的 BGR（后台游戏激活器）队列信息。"),
new("encrypt","encrypt <encryptionMethod> <stringToEncrypt>","以给定的加密方式加密字符串——详见​下文的解释。"),
new("farm","farm [Bots]","重新启动指定机器人的挂卡模块。"),
new("fb","fb [Bots]","列出指定机器人的自动挂卡黑名单。"),
new("fbadd","fbadd [Bots] <AppIDs>","将给定的 AppIDs 加入指定机器人的自动挂卡黑名单。"),
new("fbrm","fbrm [Bots] <AppIDs>","将给定的 AppIDs 从指定机器人的自动挂卡黑名单中移除。"),
new("fq","fq [Bots]","列出指定机器人的优先挂卡队列。"),
new("fqadd","fqadd [Bots] <AppIDs>","将给定的 AppIDs 加入指定机器人的优先挂卡队列。"),
new("fqrm","fqrm [Bots] <AppIDs>","将给定的 AppIDs 从指定机器人的优先挂卡队列中移除。"),
new("hash","hash <hashingMethod> <stringToHash>","以指定的加密方式生成给定字符串的哈希值——详见​下文的解释。"),
new("help","help","显示帮助（指向此页面的链接）。"),
new("input","input [Bots] <Type> <Value>","为指定机器人填写给定的输入值，仅在 Headless 模式中可用——详见​下文的解释。"),
new("level","level [Bots]","显示指定机器人的 Steam 帐户等级。"),
new("loot","loot [Bots]","将指定机器人的所有 LootableTypes 社区物品拾取到其 SteamUserPermissions 属性中设置的 Master 用户（如果有多个则取 steamID 最小的）。"),
new("loot@","loot@ [Bots] <AppIDs>","将指定机器人的所有符合给定 AppIDs 的 LootableTypes 社区物品拾取到其 SteamUserPermissions 属性中设置的 Master 用户（如果有多个则取 steamID 最小的）。 此命令与 loot% 相反。"),
new("loot%","loot% [Bots] <AppIDs>","将指定机器人的所有不符合给定 AppIDs 的 LootableTypes 社区物品拾取到其 SteamUserPermissions 属性中设置的 Master 用户（如果有多个则取 steamID 最小的）。 此命令与 loot@ 相反。"),
new("loot^","loot^ [Bots] <AppID> <ContextID>","将指定机器人的 ContextID 库存分类中符合给定 AppID 的物品拾取到其 SteamUserPermissions 属性中设置的 Master 用户（如果有多个则取 steamID 最小的）。"),
new("mab","mab [Bots]","列出 MatchActively 自动交易的 App 黑名单。"),
new("mabadd","mabadd [Bots] <AppIDs>","将给定的 AppIDs 加入到 MatchActively 自动交易的 App 黑名单。"),
new("mabrm","mabrm [Bots] <AppIDs>","将给定的 AppIDs 从 MatchActively 自动交易 App 黑名单中移除。"),
new("nickname","nickname [Bots] <Nickname>","将指定机器人的昵称更改为 Nickname。"),
new("owns","owns [Bots] <Games>","检查指定机器人是否已拥有 Games，该参数解释详见​下文。"),
new("pause","pause [Bots]","永久暂停指定机器人的自动挂卡模块。 ASF 在本次会话中将不会再尝试对此帐户进行挂卡，除非您手动 resume 或者重启 ASF。"),
new("pause~","pause~ [Bots]","临时暂停指定机器人的自动挂卡模块。 挂卡进程将会在下次游戏事件或者机器人断开连接时自动恢复。 您可以 resume 以恢复挂卡。"),
new("pause&","pause& [Bots] <Seconds>","临时暂停指定机器人的自动挂卡模块 Seconds 秒。 之后，挂卡模块会自动恢复。"),
new("play","play [Bots] <AppIDs,GameName>","切换到手动挂卡——使指定机器人运行给定的 AppIDs，并且可选自定义 GameName 为游戏名称。 若要此功能正常工作，您的 Steam 帐户必须拥有所有您指定的 AppIDs 的有效许可，包括免费游戏。 使用 reset 或 resume 命令恢复。"),
new("points","points [Bots]","显示指定机器人的 Steam 商店​点数余额。"),
new("privacy","privacy [Bots] <Settings>","更改指定机器人的 Steam 隐私设置，可用选项见​下文。"),
new("redeem","redeem [Bots] <Keys>","为指定机器人激活给定的游戏序列号或钱包充值码。"),
new("redeem^","redeem^ [Bots] <Modes> <Keys>","以 Modes 模式为指定机器人激活给定的游戏序列号或钱包充值码，模式详见下文的​解释。"),
new("reset","reset [Bots]","重置为原始（之前的）游玩状态，用来配合 play 命令的手动挂卡模式使用。"),
new("resume","resume [Bots]","恢复指定机器人的自动挂卡进程。"),
new("start","start [Bots]","启动指定机器人。"),
new("stats","stats","显示进程统计信息，例如托管内存用量。"),
new("status","status [Bots]","显示指定机器人的状态。"),
new("stop","stop [Bots]","停止指定机器人。"),
new("tb","tb [Bots]","列出指定机器人的交易黑名单用户。"),
new("tbadd","tbadd [Bots] <SteamIDs64>","将给定的 SteamIDs 加入指定机器人的交易黑名单。"),
new("tbrm","tbrm [Bots] <SteamIDs64>","将给定的 SteamIDs 从指定机器人的交易黑名单中移除。"),
new("transfer","transfer [Bots] <TargetBot>","将指定机器人的所有 TransferableTypes 社区物品转移到一个目标机器人。"),
new("transfer@","transfer@ [Bots] <AppIDs> <TargetBot>","将指定机器人的所有符合给定 AppIDs 的 TransferableTypes 社区物品转移到一个目标机器人。 此命令与 transfer% 相反。"),
new("transfer%","transfer% [Bots] <AppIDs> <TargetBot>","将指定机器人的所有不符合给定 AppIDs 的 TransferableTypes 社区物品转移到一个目标机器人。 此命令与 transfer@ 相反。"),
new("transfer^","transfer^ [Bots] <AppID> <ContextID> <TargetBot>","将指定机器人的 ContextID 库存分类中符合给定 AppID 的物品转移到一个目标机器人。"),
new("unpack","unpack [Bots]","拆开指定机器人库存中的所有补充包。"),
new("version","version","显示 ASF 的版本号。"),
    });

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
