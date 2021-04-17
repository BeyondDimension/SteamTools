using Microsoft.Extensions.Logging;
using System;
using System.Application.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
	public class ScriptManager:IScriptManager
	{
		protected const string TAG = "Script";

		protected readonly ILogger logger;
		protected readonly IToast toast;
		protected readonly IHttpService httpService;

		public ScriptManager(
		  ILoggerFactory loggerFactory, IToast toast, IHttpService httpService)
		{
			this.toast = toast;
			this.httpService = httpService;
			logger = loggerFactory.CreateLogger<ScriptManager>();
		}

		public async ValueTask<bool> AddScript()
		{
			var savePath = Path.Combine(IOPath.AppDataDirectory, TAG);


			return false;
		}
		public async Task<bool> BuildScriptAsync(ScriptDTO model)
		{
			var cachePath = Path.Combine(IOPath.CacheDirectory, TAG);
			try
			{
				var cacheFile= Path.Combine(cachePath, $"{model.Name}");

				var scriptContent = new StringBuilder();
				if (model.RequiredJsArray != null)
				{
					foreach (var item in model.RequiredJsArray)
					{
						try { 
						var scriptInfo = await httpService.GetAsync<string>(item);
						scriptContent.Append(scriptInfo);
						}
						catch (Exception e) {
							var errorMsg = $"脚本依赖下载出错:[{model.Name}]_{item}";
							logger.LogError(e, errorMsg);
							toast.Show(errorMsg);
						}
					}
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
		public async ValueTask<IList<ScriptDTO>> GetScript()
		{


			//toast.Show();
			return null;
		}

	}
}
