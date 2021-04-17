using Microsoft.Extensions.Logging;
using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
	public  class ScriptManager
    {
        protected const string TAG = "ScriptManager";

        protected readonly ILogger logger;
        public ScriptManager(
          ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger(TAG); 
        }
        public async ValueTask<bool> AddScript() {
            return false;
        }

        public async ValueTask<IList<ScriptDTO>> GetScript()
        {



            return null;
        }

    }
}
