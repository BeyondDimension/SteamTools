extern alias JumpLists;
using JumpLists::System.Windows.Shell;

using System.Application.Models;
using JumpList = System.Application.UI.JumpLists;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows")]
    internal sealed class SystemJumpListServiceImpl : ISystemJumpListService
    {
        public void InitJumpList()
        {
            JumpList.Init();
        }

        public void AddJumpTask(string title, string applicationPath, string iconResourcePath, string arguments = "", string description = "", string customCategory = "")
        {
            JumpList.AddJumpTask(new JumpTask
            {
                // Get the path to Calculator and set the JumpTask properties.
                ApplicationPath = applicationPath,
                IconResourcePath = iconResourcePath,
                Arguments = arguments,
                Title = title,
                Description = description,
                CustomCategory = customCategory,
            });
        }
    }
}
