using ReactiveUI;

namespace System.Application.Services
{
    public abstract class MvvmService<TService> : ReactiveObject where TService : MvvmService<TService>
    {
        static TService? mCurrent;
        public static TService Current => mCurrent ??
            throw new NullReferenceException($"{typeof(TService).Name} not initialized.");

        public MvvmService()
        {
            mCurrent = (TService)this;
        }
    }
}
