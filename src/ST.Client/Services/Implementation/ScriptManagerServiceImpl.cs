using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace System.Application.Services.Implementation
{
	public class ScriptManagerServiceImpl : IScriptManagerService
	{
		protected const string TAG = "Script";

		protected readonly ILogger logger;
		protected readonly IToast toast;
		protected readonly IHttpService httpService;
		protected readonly IMapper mapper;

		protected readonly IScriptRepository scriptRepository;
		public ScriptManagerServiceImpl(
		  IScriptRepository scriptRepository, IMapper mapper, ILoggerFactory loggerFactory, IToast toast, IHttpService httpService)
		{
			this.scriptRepository = scriptRepository;
			this.mapper = mapper;
			this.toast = toast;
			this.httpService = httpService;
			logger = loggerFactory.CreateLogger<ScriptManagerServiceImpl>();
		}

		public async Task<bool> AddScriptAsync(string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			if (fileInfo.Exists)
			{
				ScriptDTO.TryParse(filePath, out ScriptDTO? info);

				if (info != null)
				{
					try
					{
						if (info.Content != null)
						{
							var md5 = Hashs.String.MD5(info.Content);
							var sha512 = Hashs.String.SHA512(info.Content);
							if (await scriptRepository.ExistsScript(md5, sha512))
								return true;
							var cachePath = Path.Combine(IOPath.CacheDirectory, TAG, $"{md5}.js");
							info.FilePath = cachePath;
							if (await BuildScriptAsync(info))
							{
								var db = mapper.Map<Script>(info);
								await scriptRepository.InsertOrUpdateAsync(db);
								return true;
							}
							else {
								var msg = $"JS打包出错:{filePath}";
								logger.LogError(msg);
								toast.Show(msg);
							}
							return false;
						}
						else
						{
							var msg = $"JS读取出错:{filePath}";
							logger.LogError(msg);
							toast.Show(msg);
						}
					}
					catch (Exception e)
					{
						logger.LogError(e, "map转换出现错误");
					}
				}
				else
				{
					logger.LogError($"文件读取失败{filePath}");
				}
			}
			else
			{
				logger.LogError($"文件不存在:{filePath}");
			}
			return false;
		}
		public async Task<bool> BuildScriptAsync(ScriptDTO model)
		{
			try
			{ 

				if (model.RequiredJsArray != null)
				{
					var scriptContent = new StringBuilder();
					foreach (var item in model.RequiredJsArray)
					{
						try
						{
							var scriptInfo = await httpService.GetAsync<string>(item);
							scriptContent.AppendLine(scriptInfo);
						}
						catch (Exception e)
						{
							var errorMsg = $"脚本依赖下载出错:[{model.Name}]_{item}";
							logger.LogError(e, errorMsg);
							toast.Show(errorMsg);
						}
					}
					scriptContent.AppendLine(model.Content);
					var fileInfo = new FileInfo(model.FilePath);
					if (fileInfo.Exists)
						fileInfo.Delete(); 
					using (var stream = fileInfo.CreateText())
					{ 
						stream.Write(scriptContent);
						await stream.FlushAsync();
						await stream.DisposeAsync();
						//stream
					}
					return true;
					//var scriptRequired = new string[model.RequiredJsArray.Length];
					//Parallel.ForEach(model.RequiredJsArray, async (item,index)=>
					//{
					//	var scriptInfo = await httpService.GetAsync<string>(item);  
					//});
				}

			}
			catch (Exception e)
			{
				var errorMsg = $"脚本绑定出错:[{model.Name}]";
				logger.LogError(e, errorMsg);
				toast.Show(errorMsg);
			}
			return false;
		}
		public async Task<IList<ScriptDTO>> GetAllScript()
		{ 
			var scriptList= mapper.Map<List<ScriptDTO>>(await scriptRepository.GetAllAsync()); 
			foreach (var item in scriptList)
			{
				item.Content= File.ReadAllText(item.CachePath);
			} 
			return scriptList;
		}

	}
}
