using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class MyPageViewModel : PageViewModel
    {
        public MyPageViewModel()
        {
            Title = AppResources.My;

            UserService.Current.WhenAnyValue(x => x.User).Subscribe(value =>
            {
                if (value == null)
                {
                    nickNameNullValLangChangeSubscribe?.RemoveTo(this);
                    nickNameNullValLangChangeSubscribe = R.Current.WhenAnyValue(x => x)
                    .Subscribe(value =>
                    {
                        // 未登录时显示的文本，多语言绑定
                        NickName = NickNameNullVal;
                    }).AddTo(this);
                }
                else
                {
                    nickNameNullValLangChangeSubscribe?.RemoveTo(this);
                    nickNameNullValLangChangeSubscribe = null;
                    NickName = value.NickName;
                }
            }).AddTo(this);
        }

        IDisposable? nickNameNullValLangChangeSubscribe;
        static string NickNameNullVal => AppResources.LoginAndRegister;
        string nickName = NickNameNullVal;
        public string NickName
        {
            get => nickName;
            set => this.RaiseAndSetIfChanged(ref nickName, value);
        }
    }
}