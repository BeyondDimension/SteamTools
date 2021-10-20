//using ReactiveUI;

//namespace System.Application.Services
//{
//    public abstract class MvvmService<TService> : ReactiveObject where TService : MvvmService<TService>, new()
//    {
//        static TService? mCurrent;
//        public static TService Current => mCurrent ?? new TService();

//        public MvvmService()
//        {
//            mCurrent = (TService)this;
//        }
//    }
//}
