using System;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    public interface IScriptRepository : IRepository<Script, int>
    {
        /// <summary>
        /// 脚本可导入数量最大值
        /// </summary>
        public const int MaxValue = 100;

        Task<bool> ExistsScript(string md5, string sha512);

        Task<Script> ExistsScriptName(string name);

        Task<IList<Script>> GetAllAsync();
        Task SaveScriptEnable(ScriptDTO item);
    }
}
