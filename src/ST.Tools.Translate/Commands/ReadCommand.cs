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
    /// 读取 xlsx 文件
    /// </summary>
    static class ReadCommand
    {
        public static void Add(RootCommand command)
        {
            var read_xlsx = new Command("read-xlsx", "读取 xlsx 写入 resx")
            {
                Handler = CommandHandler.Create(() =>
                {

                }),
            };
            command.AddCommand(read_xlsx);
        }
    }
}
