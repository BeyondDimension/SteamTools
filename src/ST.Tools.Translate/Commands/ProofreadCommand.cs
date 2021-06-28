using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Commands
{
    /// <summary>
    /// 校对 xlsx 文件
    /// </summary>
    static class ProofreadCommand
    {
        public static void Add(RootCommand command)
        {
            var proofread_xlsx = new Command("proofread-xlsx", "校对 xlsx")
            {
                Handler = CommandHandler.Create(() =>
                {

                }),
            };
            command.AddCommand(proofread_xlsx);
        }
    }
}
