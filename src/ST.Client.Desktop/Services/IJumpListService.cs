namespace System.Application.Services
{
    public interface IJumpListService
    {
        void InitJumpList();

        void AddJumpTask(string title, string applicationPath, string iconResourcePath, string arguments = "", string description = "", string customCategory = "");
    }
}