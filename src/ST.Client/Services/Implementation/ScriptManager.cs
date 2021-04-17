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
        protected readonly IToast toast;
        public ScriptManager(
          ILoggerFactory loggerFactory,IToast toast)
        {
            this.toast = toast;
            logger = loggerFactory.CreateLogger(TAG); 
        }
        public async ValueTask<bool> AddScript() {
            return false;
        }

        public async ValueTask<IList<ScriptDTO>> GetScript()
        {


            //toast.Show();
            return null;
        } 

    }
}
