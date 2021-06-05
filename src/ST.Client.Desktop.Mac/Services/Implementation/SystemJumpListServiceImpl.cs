 
using System.Application.Models; 
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace System.Application.Services.Implementation
{ 
    internal sealed class SystemJumpListServiceImpl : ISystemJumpListService
    {
        public void InitJumpList()
        { 
        }

        public void AddJumpTask(string title, string applicationPath, string iconResourcePath, string arguments = "", string description = "", string customCategory = "")
        {
           
        }
    }
}
