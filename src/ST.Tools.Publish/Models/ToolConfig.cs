using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;

namespace System.Application.Models
{
    internal sealed class ToolConfig : IExplicitHasValue
    {
        const string fileName = "Config.json";
        static readonly string filePath = Path.Combine(IOPath.AppDataDirectory, fileName);
        static readonly Lazy<ToolConfig> value = new(GetConfig);

        /// <summary>
        /// 当前工具配置文件，位于程序目录下
        /// </summary>
        public static ToolConfig Value => value.Value;

        static ToolConfig GetConfig()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var jsonStr = File.ReadAllText(filePath);
                    return Serializable.DJSON<ToolConfig>(jsonStr) ?? new();
                }
                catch
                {
                }
            }

            return new ToolConfig();
        }

        public void Save()
        {
            var jsonStr = Serializable.SJSON(this, writeIndented: true);
            IOPath.FileIfExistsItDelete(filePath);
            File.WriteAllText(filePath, jsonStr);
        }

        static void Init()
        {
            if (!File.Exists(filePath))
            {
                Value.Save();
            }
        }

        public static void AddInitCommand(RootCommand rootCommand)
        {
            var init = new Command("init", "初始化")
            {
                Handler = CommandHandler.Create(() =>
                {
                    Init();
                })
            };
            rootCommand.Add(init);
        }

        /// <summary>
        /// 云端数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Web目录，CDN静态站点目录
        /// </summary>
        public string WwwRootPath { get; set; } = string.Empty;

        bool IExplicitHasValue.ExplicitHasValue()
        {
            return !string.IsNullOrWhiteSpace(ConnectionString) &&
                !string.IsNullOrWhiteSpace(WwwRootPath);
        }
    }
}