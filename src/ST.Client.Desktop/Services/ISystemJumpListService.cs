using System.Application.Models;
using System.Runtime.Versioning;

namespace System.Application.Services
{
    [SupportedOSPlatform("Windows")]
    public interface ISystemJumpListService
    {
        void InitJumpList();

        void AddJumpTask(string title, string applicationPath, string iconResourcePath, string arguments = "", string description = "", string customCategory = "");
    }
}