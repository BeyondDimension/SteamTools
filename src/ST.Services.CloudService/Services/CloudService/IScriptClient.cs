using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
	public interface IScriptClient
	{
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="id">当前应用版本Id</param>
        /// <param name="platform">(单选)当前设备所属平台</param>
        /// <param name="deviceIdiom">(单选)当前设备所属类型</param>
        /// <param name="supportedAbis">(多选)当前设备支持的CPU架构</param>
        /// <param name="osVersion">当前设备运行的操作系统版本号</param>
        /// <returns></returns>
        Task<IApiResponse<ScriptResponse>> Basics();
    }
}
