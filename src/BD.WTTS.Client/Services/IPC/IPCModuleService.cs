// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IPCModuleService : IDisposable
{
    static string GetClientPipeName(string moduleName, string pipeName)
        => $"{pipeName}_{moduleName}";
}
