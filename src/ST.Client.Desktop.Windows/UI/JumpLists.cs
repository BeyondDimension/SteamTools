#pragma warning disable CA1416 // 验证平台兼容性
extern alias JumpLists;
using JumpLists::System.Windows.Shell;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.UI
{
    /// <summary>
    /// 表示作为菜单显示在 Windows 7 任务栏按钮上的项和任务的列表。
    /// <para><see cref="https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.shell.jumplist?view=net-5.0"/></para>
    /// <para>Taskbar Extensions</para>
    /// <para><see cref="https://docs.microsoft.com/zh-cn/windows/win32/shell/taskbar-extensions?redirectedfrom=MSDN"/></para>
    /// </summary>
    public static class JumpLists
    {
        public static void Init()
        {
            var jumpList1 = new JumpList
            {
                ShowRecentCategory = true,
                ShowFrequentCategory = true,
            };
            //jumpList1.JumpItemsRejected += JumpList1_JumpItemsRejected;
            //jumpList1.JumpItemsRemovedByUser += JumpList1_JumpItemsRemovedByUser;
            //jumpList1.JumpItems.Add(new JumpTask
            //{
            //    Title = "RuaRua",
            //    Description = "以该账号启动Steam",
            //    ApplicationPath = AppHelper.ProgramPath,
            //    Arguments = "-clt steam -account rmbadmin",
            //    IconResourcePath = AppHelper.ProgramPath,
            //});
            //jumpList1.JumpItems.Add(new JumpTask
            //{
            //    Title = "DarkMaple",
            //    Description = "以该账号启动Steam",
            //    ApplicationPath = AppHelper.ProgramPath,
            //    Arguments = "-clt steam -account kuafaaf",
            //    IconResourcePath = AppHelper.ProgramPath,
            //});
            //jumpList1.JumpItems.Add(new JumpTask
            //{
            //    Title = "Read Me",
            //    Description = "Open readme.txt in Notepad.",
            //    ApplicationPath = @"C:\Windows\notepad.exe",
            //    IconResourcePath = @"C:\Windows\System32\imageres.dll",
            //    IconResourceIndex = 14,
            //    WorkingDirectory = @"C:\Users\Public\Documents",
            //    Arguments = "readme.txt",
            //});
            JumpList.SetJumpList(AvaloniaApplication.Current, jumpList1);
        }


        public static void AddJumpTask(JumpTask task)
        {
            var jumpList1 = new JumpList
            {
                ShowRecentCategory = true,
                ShowFrequentCategory = true,
            };
            //jumpList1.JumpItemsRejected += JumpList1_JumpItemsRejected;
            //jumpList1.JumpItemsRemovedByUser += JumpList1_JumpItemsRemovedByUser;
            jumpList1.JumpItems.Add(task);
            JumpList.SetJumpList(AvaloniaApplication.Current, jumpList1);
        }


        //static void AddJumpTask()
        //{
        //    // Configure a new JumpTask.
        //    var jumpTask1 = new JumpTask
        //    {
        //        // Get the path to Calculator and set the JumpTask properties.
        //        ApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "calc.exe"),
        //        IconResourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "calc.exe"),
        //        Title = "Calculator",
        //        Description = "Open Calculator.",
        //        CustomCategory = "User Added Tasks"
        //    };
        //    // Get the JumpList from the application and update it.
        //    JumpList jumpList1 = JumpList.GetJumpList(WpfApplication.Current);
        //    jumpList1.JumpItems.Add(jumpTask1);
        //    JumpList.AddToRecentCategory(jumpTask1);
        //    jumpList1.Apply();
        //}
    }
}
#pragma warning restore CA1416 // 验证平台兼容性