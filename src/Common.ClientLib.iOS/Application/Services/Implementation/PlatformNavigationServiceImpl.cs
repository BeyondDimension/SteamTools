namespace System.Application.Services.Implementation;

public abstract class PlatformNavigationServiceImpl : IPlatformNavigationService
{
    public void Pop()
    {
        throw new NotImplementedException();
    }

    public void Push(Type type, PushFlags flags = PushFlags.Empty)
    {
        throw new NotImplementedException();
    }
}