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
using System.Linq;
using System.Application.Properties;

namespace System.Application.Services.Implementation
{
	public class ScriptManagerServiceImpl : IScriptManagerService
	{
		public const string TAG = "Script";

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
		/// <summary>
		/// 添加脚本
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public async Task<(bool state, ScriptDTO? model, string msg)> AddScriptAsync(string filePath, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null)
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
							{
								return (false, null, SR.Script_FileRepeat);
							}
							var url = Path.Combine(TAG, $"{md5}.js");
							var savePath = Path.Combine(IOPath.AppDataDirectory, url);
							var saveInfo = new FileInfo(savePath);
							if (!saveInfo.Directory.Exists)
							{
								saveInfo.Directory.Create();
							}
							if (saveInfo.Exists)
								saveInfo.Delete();
							fileInfo.CopyTo(savePath);
							if (oldInfo != null)
							{
								info.LocalId = oldInfo.LocalId;
								info.Id = oldInfo.Id;
								var state = await DeleteScriptAsync(oldInfo, false);
								if (!state.state)
									return (false, null, string.Format(SR.Script_FileDeleteError, oldInfo.FilePath));
							}
							if (pid.HasValue)
								info.Id = pid.Value;
							var cachePath = Path.Combine(IOPath.CacheDirectory, url);
							info.FilePath = url;
							info.CachePath = url;
							if (await BuildScriptAsync(info, build))
							{
								var db = mapper.Map<Script>(info);
								db.MD5 = md5;
								db.SHA512 = sha512;
								if (order.HasValue)
									db.Order = order.Value;
								try
								{
									if (deleteFile)
										fileInfo.Delete();
								}
								catch (Exception e) { logger.LogError(e.ToString()); }
								var state = (await scriptRepository.InsertOrUpdateAsync(db)).rowCount > 0;
								return (state, info, state ? SR.Script_SaveDbSuccess : SR.Script_SaveDBError);
							}
							else
							{
								var msg = string.Format(SR.Script_BuildError, filePath);
								logger.LogError(msg);
								toast.Show(msg);
								return (false, null, msg);
							}
						}
						else
						{
							var msg = string.Format(SR.Script_ReadFileError, filePath);
							logger.LogError(msg);
							toast.Show(msg);
							return (false, null, msg);
						}
					}
					catch (Exception e)
					{
						var msg = string.Format(SR.Script_ReadFileError, e.ToString());
						logger.LogError(e, msg);
						return (false, null, msg);
					}
				}
				else
				{
					var msg = string.Format(SR.Script_ReadFileError, filePath); //$"文件解析失败，请检查格式:{filePath}";
					logger.LogError(msg);
					return (false, null, msg);
				}
			}
			else
			{
				var msg = string.Format(SR.Script_NoFile, filePath);// $"文件不存在:{filePath}";
				logger.LogError(msg);
				return (false, null, msg);
			}
		}
		public async Task<bool> BuildScriptAsync(ScriptDTO model, bool build = true)
		{
			try
			{
				if (model.RequiredJsArray != null)
				{
					var scriptContent = new StringBuilder();
					if (build)
					{
						scriptContent.AppendLine("(function() {");
						foreach (var item in model.RequiredJsArray)
						{
							try
							{
								var scriptInfo = await httpService.GetAsync<string>(item);
								scriptContent.AppendLine(scriptInfo);
							}
							catch (Exception e)
							{
								var errorMsg = string.Format(SR.Script_BuildDownloadError, model.Name, item);
								logger.LogError(e, errorMsg);
								toast.Show(errorMsg);
							}
						}
						scriptContent.AppendLine(model.Content);
						scriptContent.AppendLine("})( )");
					}
					else
					{
						scriptContent.Append(model.Content);
					}
					var cachePath = Path.Combine(IOPath.CacheDirectory, model.CachePath);
					var fileInfo = new FileInfo(cachePath);
					if (!fileInfo.Directory.Exists)
					{
						fileInfo.Directory.Create();
					}
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
				var msg = string.Format(SR.Script_BuildError, e.ToString());
				logger.LogError(e, msg);
				toast.Show(msg);
			}
			return false;
		}
		public async Task<(bool state, string msg)> DeleteScriptAsync(ScriptDTO item, bool rmDb = true)
		{
			if (item.LocalId > 0)
			{
				var info = await scriptRepository.FirstOrDefaultAsync(x => x.Id == item.LocalId);
				if (info != null)
				{
					var url = Path.Combine(TAG, $"{info.MD5}.js");
					try
					{
						var cachePath = Path.Combine(IOPath.CacheDirectory, url);
						var cacheInfo = new FileInfo(cachePath);
						if (cacheInfo.Exists)
							cacheInfo.Delete();
					}
					catch (Exception e)
					{
						var msg = string.Format(SR.Script_CacheDeleteError, e.ToString());
						logger.LogError(e, msg);
						return (false, msg);
					}
					try
					{
						var savePath = Path.Combine(IOPath.AppDataDirectory, url);
						var fileInfo = new FileInfo(savePath);
						if (fileInfo.Exists)
							fileInfo.Delete();
					}
					catch (Exception e)
					{
						var msg = string.Format(SR.Script_FileDeleteError, e.ToString());
						logger.LogError(e, msg);
						return (false, msg);
					}
					if (rmDb)
					{
						var state = (await scriptRepository.DeleteAsync(item.LocalId)) > 0;
						return (state, state ? SR.Script_DeleteSuccess : SR.Script_DeleteError);
					}
					return (true, SR.Script_DeleteSuccess);
				}
				else
				{
					return (false, SR.Script_DeleteError);
				}
			}
			return (false, SR.Script_NoKey);
		}
		public async Task<ScriptDTO> TryReadFile(ScriptDTO item)
		{
			var cachePath = Path.Combine(IOPath.CacheDirectory, item.CachePath);
			var fileInfo = new FileInfo(cachePath);
			if (fileInfo.Exists)
				item.Content = File.ReadAllText(cachePath);
			else
			{
				var infoPath = Path.Combine(IOPath.AppDataDirectory, item.FilePath);
				var infoFile = new FileInfo(infoPath);
				if (infoFile.Exists)
				{
					if (await BuildScriptAsync(item,item.IsBuild))
					{
						cachePath = Path.Combine(IOPath.CacheDirectory, item.CachePath);
						fileInfo = new FileInfo(cachePath);
						if (fileInfo.Exists)
							item.Content = File.ReadAllText(cachePath);
					}
					else
					{
						toast.Show(string.Format(SR.Script_ReadFileError, item.Name));
					}

				}
				else
				{
					var temp = await DeleteScriptAsync(item);
					if (temp.state)//$"脚本:{item.Name}_文件丢失已删除"
						toast.Show(string.Format(SR.Script_NoFile, item.Name));
					else
						toast.Show(string.Format(SR.Script_NoFileDeleteError, item.Name));
					//toast.Show($"脚本:{item.Name}_文件丢失，删除失败去尝试手动删除");
				}
			}
			return item;
		}
		public async Task<IEnumerable<ScriptDTO>> GetAllScript()
		{
			var scriptList = mapper.Map<List<ScriptDTO>>(await scriptRepository.GetAllAsync());
			try
			{
				foreach (var item in scriptList)
				{
					await TryReadFile(item);
					//if (item.Content!= null) { 

					//}
				}
			}
			catch (Exception e)
			{
				var errorMsg = string.Format(SR.Script_ReadFileError, e.ToString());//$"文件读取出错:[{e}]";
				logger.LogError(e, errorMsg);
				toast.Show(errorMsg);
			}
			return scriptList.Where(x => !string.IsNullOrWhiteSpace(x.Content));
		}
		//public async Task<ScriptDTO> BasicsTry(ScriptDTO script) { 

		//}
		public async Task<(bool state, string path)> DownloadScript(string url)
		{
			var scriptInfo = await httpService.GetAsync<string>(url);
			if (scriptInfo != null && scriptInfo.Length > 0)
			{
				try
				{
					var md5 = Hashs.String.MD5(scriptInfo);
					var cachePath = Path.Combine(IOPath.CacheDirectory, TAG, md5);
					var fileInfo = new FileInfo(cachePath);
					if (!fileInfo.Directory.Exists)
					{
						fileInfo.Directory.Create();
					}
					if (fileInfo.Exists)
						fileInfo.Delete();
					using (var stream = fileInfo.CreateText())
					{
						stream.Write(scriptInfo);
						await stream.FlushAsync();
						await stream.DisposeAsync();
					}
					return (true, cachePath);
				}
				catch (Exception e)
				{
					return (false, e.ToString());
				}
			}
			return (false, string.Empty);
		}
	}
}
