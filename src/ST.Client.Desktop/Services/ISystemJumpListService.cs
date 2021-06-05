namespace System.Application.Services
{
    public interface ISystemJumpListService
    {
        void InitJumpList();

        void AddJumpTask(string title, string applicationPath, string iconResourcePath, string arguments = "", string description = "", string customCategory = "");
    }
}