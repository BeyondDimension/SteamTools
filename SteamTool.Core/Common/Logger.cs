using log4net;
using log4net.Config;
using log4net.Repository;
using SteamTool.Core.Properties;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SteamTool.Core.Common
{
    /// <summary>
    /// 日志操作管理类
    /// </summary>
    public sealed class Logger
    {
        private static ILog _textLog;
        public static bool EnableTextLog = false;

        static Logger()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Assembly.GetCallingAssembly().GetName().Name);
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, @"log\Log4net.config")))
            {
                XmlConfigurator.Configure(repository, new FileInfo(@"log\Log4net.config"));
            }
            else
            {
                var doc = new XmlDocument();
                doc.LoadXml(Resources.Log4netXml);
                XmlConfigurator.Configure(repository, doc.DocumentElement);
            }
            _textLog = LogManager.GetLogger(repository.Name, "TextLog");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void InitLogger(string name, string configFile)
        {
            var loggerRepository = LogManager.CreateRepository(name);
            var path = Path.Combine(AppContext.BaseDirectory, configFile);
            log4net.Config.XmlConfigurator.Configure(loggerRepository, new FileInfo(path));
            _textLog = LogManager.GetLogger(loggerRepository.Name, "TextLog");
        }

        /// <summary>
        /// 毁灭性的错误
        /// </summary>
        /// <param name="msg"></param>
        public static void Fatal(object msg)
        {
            if (EnableTextLog)
            {
                _textLog.Fatal(msg);
            }
        }

        /// <summary>
        /// 毁灭性的错误
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void Fatal(object msg, Exception ex)
        {
            if (EnableTextLog)
            {
                _textLog.Fatal(msg, ex);
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="msg"></param>
        public static void Error(object msg)
        {
            if (EnableTextLog)
            {
                _textLog.Error(msg);
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void Error(object msg, Exception ex)
        {
            if (EnableTextLog)
            {
                _textLog.Error(msg, ex);
            }
        }

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="msg"></param>
        public static void Warn(object msg)
        {
            if (EnableTextLog)
            {
                _textLog.Warn(msg);
            }
        }

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void Warn(object msg, Exception ex)
        {
            if (EnableTextLog)
            {
                _textLog.Warn(msg, ex);
            }
        }

        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(object msg)
        {
            if (EnableTextLog)
            {
                _textLog.Info(msg);
            }
        }

        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        /// <param name="isText"></param>
        public static void Info(object msg, Exception ex)
        {
            if (EnableTextLog)
            {
                _textLog.Info(msg, ex);
            }
        }

        /// <summary>
        /// 调试信息
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(object msg)
        {
            if (EnableTextLog)
            {
                _textLog.Debug(msg);
            }
        }

        /// <summary>
        /// 调试信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void Debug(object msg, Exception ex)
        {
            if (EnableTextLog)
            {
                _textLog.Debug(msg, ex);
            }
        }
    }
}
