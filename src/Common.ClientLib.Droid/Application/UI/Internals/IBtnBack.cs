using Android.Views;
using System.Application.UI.Internals;
using System.Common;

namespace System.Application.UI.Internals
{
    internal interface IBtnBack : IFindViewById, View.IOnClickListener
    {
        /// <summary>
        /// 是否有返回按钮
        /// <para>如果重写 <see cref="BindingBtnBack"/> 则可忽略此项</para>
        /// </summary>
        bool HasBtnBack { get; }

        /// <summary>
        /// 绑定返回按钮
        /// </summary>
        void BindingBtnBack();

        /// <inheritdoc cref="BindingBtnBack"/>
        public static void BindingBtnBack(IBtnBack t)
        {
            if (t.HasBtnBack)
            {
                var btnBack = t.FindViewById(R.id.btn_back);
                if (btnBack != null)
                {
                    btnBack.SetOnClickListener(t);
                }
            }
        }
    }
}

namespace System.Application.UI.Activities
{
    partial class BaseActivity : IBtnBack
    {
        /// <inheritdoc cref="IBtnBack.HasBtnBack"/>
        protected virtual bool HasBtnBack { get; }

        bool IBtnBack.HasBtnBack => HasBtnBack;

        /// <inheritdoc cref="IBtnBack.BindingBtnBack"/>
        protected virtual void BindingBtnBack() => IBtnBack.BindingBtnBack(this);

        void IBtnBack.BindingBtnBack() => BindingBtnBack();
    }
}