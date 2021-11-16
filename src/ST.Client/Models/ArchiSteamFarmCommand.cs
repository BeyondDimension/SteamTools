using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace System.Application.Models
{
    public class ArchiSteamFarmCommand
    {
        public static readonly Lazy<IImmutableList<ArchiSteamFarmCommand>> Commands = new(ImmutableList.Create(new ArchiSteamFarmCommand[]
        {
            new("2fa [b]","为指定机器人生成临时的​两步验证​令牌。"),
            new("2fano [b]","为指定机器人拒绝所有等待操作的​两步验证​交易确认。"),
            new("2faok [b]","为指定机器人拒绝所有等待操作的​两步验证​交易确认。"),
            new("level [b]","显示指定机器人的 Steam 帐户等级。"),
            new("addlicense [Bots] <Licenses>","为指定机器人激活给定的 Licenses （许可）。"),
        }));

        public ArchiSteamFarmCommand(string command, string? description = null)
        {
            Command = command;
            Description = description;
        }

        public string Command { get; }

        public string CommandText => "!" + Command;

        //public int Access { get; }

        public string? Description { get; }
    }
}
