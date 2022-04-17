using System.Properties;
using System.Collections.Generic;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPConstructor = MessagePack.SerializationConstructorAttribute;

namespace System.Application.Models
{
    /// <inheritdoc cref="IInterface"/>
    [MPObj]
    public class NotificationBuilder : NotificationBuilder.IInterface
    {
        /// <summary>
        /// 默认通知标题
        /// </summary>
        public const string DefaultTitle = ThisAssembly.AssemblyTrademark;
        public const bool DefaultAutoCancel = true;
        public const Entrance DefaultEntrance = Entrance.Main;

        [MPKey(1)]
        public string Title { get; set; } = DefaultTitle;

        [MPKey(2)]
        public string Content { get; set; } = string.Empty;

        [MPKey(3)]
        public NotificationType Type { get; set; }

        [MPKey(4)]
        public bool AutoCancel { get; set; } = DefaultAutoCancel;

        /// <inheritdoc cref="IInterface.Click"/>
        [MPKey(5)]
        public ClickAction? Click { get; set; }

        ClickAction.IInterface? IInterface.Click => Click;

        /// <inheritdoc cref="IInterface"/>
        [MPObj]
        public class ClickAction : ClickAction.IInterface
        {
            [MPConstructor]
            public ClickAction()
            {

            }

            public ClickAction(string requestUri)
            {
                RequestUri = requestUri;
                Entrance = Entrance.Browser;
            }

            public ClickAction(Action action)
            {
                Action = action;
                Entrance = Entrance.Delegate;
            }

            [MPKey(0)]
            public Entrance Entrance { get; set; } = DefaultEntrance;

            [MPKey(LAST_MP_INDEX)]
            public string? RequestUri { get; set; }

            [MPIgnore]
            public Action? Action { get; set; }

            protected const int LAST_MP_INDEX = 1;

            /// <summary>
            /// 点击该通知的事件或跳转页面或打开浏览器等
            /// </summary>
            public interface IInterface
            {
                /// <summary>
                /// 点击通知的入口点
                /// </summary>
                Entrance Entrance { get; }

                /// <summary>
                /// 入口点为 <see cref="Entrance.Browser"/> 时的 HttpUrl
                /// </summary>
                string? RequestUri { get; }

                /// <summary>
                /// 入口点为 <see cref="Entrance.Delegate"/> 时的 Delegate
                /// </summary>
                Action? Action { get; }
            }
        }

        /// <inheritdoc cref="IInterface.Buttons"/>
        [MPKey(6)]
        public List<ButtonAction>? Buttons { get; set; }

        IReadOnlyList<ButtonAction.IInterface>? IInterface.Buttons => Buttons;

        [MPKey(7)]
        public string? ImageUri { get; set; }

        /// <inheritdoc cref="EImageDisplayType"/>
        [MPKey(8)]
        public EImageDisplayType ImageDisplayType { get; set; }

        [MPKey(9)]
        public string? AttributionText { get; set; }

        [MPKey(10)]
        public DateTimeOffset CustomTimeStamp { get; set; }

        /// <inheritdoc cref="IInterface"/>
        [MPObj]
        public class ButtonAction : ClickAction, ButtonAction.IInterface
        {
            [MPConstructor]
            public ButtonAction()
            {

            }

            public ButtonAction(string text, string requestUri) : base(requestUri)
            {
                Text = text;
            }

            public ButtonAction(string text, Action action) : base(action)
            {
                Text = text;
            }

            /// <summary>
            /// 按钮文本
            /// </summary>
            [MPKey(LAST_MP_INDEX + 1)]
            public string Text { get; set; } = string.Empty;

            /// <summary>
            /// 点击按钮的事件或跳转页面或打开浏览器等
            /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax#buttons</para>
            /// </summary>
            public new interface IInterface : ClickAction.IInterface
            {
                /// <summary>
                /// 按钮文本
                /// </summary>
                string Text { get; }
            }
        }

        /// <summary>
        /// 本地通知 Builder
        /// </summary>
        public interface IInterface
        {
            /// <summary>
            /// 通知标题
            /// <para>https://developer.android.google.cn/reference/androidx/core/app/NotificationCompat.Builder?hl=zh-cn#setContentTitle(java.lang.CharSequence)</para>
            /// </summary>
            string Title { get; }

            /// <summary>
            /// 通知正文
            /// <para>https://developer.android.google.cn/reference/androidx/core/app/NotificationCompat.Builder?hl=zh-cn#setContentText(java.lang.CharSequence)</para>
            /// </summary>
            string Content { get; }

            /// <inheritdoc cref="NotificationType"/>
            NotificationType Type { get; }

            /// <summary>
            /// 点击通知时自动取消通知
            /// <para>https://developer.android.google.cn/reference/androidx/core/app/NotificationCompat.Builder?hl=zh-cn#setAutoCancel(boolean)</para>
            /// </summary>
            bool AutoCancel { get; }

            /// <inheritdoc cref="ClickAction.IInterface"/>
            ClickAction.IInterface? Click { get; }

            /// <summary>
            /// 通知的按钮组
            /// </summary>
            IReadOnlyList<ButtonAction.IInterface>? Buttons { get; }

            /// <summary>
            /// 通知的主要图片
            /// </summary>
            string? ImageUri { get; }

            /// <inheritdoc cref="EImageDisplayType"/>
            EImageDisplayType ImageDisplayType { get; }

            /// <summary>
            /// 署名文本
            /// <para>周年更新中的新增功能：如果你需要引用你的内容源，可以使用署名文本。 此文本始终显示在通知底部，与应用标识或通知时间戳一起显示。</para>
            /// <para>在不支持署名文本的旧 Windows 版本中，该文本仅显示为另一文本元素（假设你还没有达到最多的三个文本元素）。</para>
            /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax#attribution-text</para>
            /// </summary>
            string? AttributionText => null;

            /// <summary>
            /// 自定义时间戳
            /// <para>创建者更新中的新增功能：现在，你可以用自己的准确表示消息/信息/内容生成时间的时间戳替代系统提供的时间戳。 可在操作中心查看此时间戳。</para>
            /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax#custom-timestamp</para>
            /// </summary>
            DateTimeOffset CustomTimeStamp => default;
        }

        /// <summary>
        /// 图片显示类型
        /// <para>图像大小限制</para>
        /// <para>Toast 通知中使用的图像可源自以下位置...</para>
        /// <para>http://</para>
        /// <para>(Impl中验证函数未支持未测试)ms-appx:///</para>
        /// <para>(Impl中验证函数未支持未测试)ms-appdata:///</para>
        /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax#image-size-restrictions</para>
        /// </summary>
        public enum EImageDisplayType : byte
        {
            /// <summary>
            /// 主图
            /// <para>周年更新的新增功能：toast 可显示主图，这是在 toast 横幅中以及在操作中心时突出显示的特别 ToastGenericHeroImage。 图像尺寸在 100% 缩放时为 364x180 像素。</para>
            /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax#hero-image</para>
            /// </summary>
            HeroImage,

            /// <summary>
            /// 内联图像
            /// <para>可提供在展开 toast 时显示的全宽内联图像。</para>
            /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax#inline-image</para>
            /// </summary>
            InlineImage,
        }
    }
}
