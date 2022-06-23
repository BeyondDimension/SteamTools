using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using DynamicData.Binding;
using System.Application.Services;

namespace System.Application.UI.Views.Controls
{
    public partial class AdDialog : UserControl
    {
        private Button? closeButton;

        /// <summary>
        /// Defines the <see cref="AutoScroll"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> SupportCloseProperty =
            AvaloniaProperty.Register<CarouselBanner, bool>(nameof(SupportClose), false);

        /// <summary>
        /// SupportClose
        /// </summary>
        public bool SupportClose
        {
            get => GetValue(SupportCloseProperty);
            set => SetValue(SupportCloseProperty, value);
        }

        public AdDialog()
        {
            InitializeComponent();

            closeButton = this.FindControl<Button>("CloseAdBtn");

            if (closeButton != null)
            {
                this.GetObservable(SupportCloseProperty)
                    .Subscribe(x => closeButton.IsVisible = x);

                closeButton.Click += (s, e) =>
                {
                    RemoveAd();
                };
            }

            UserService.Current.WhenValueChanged(x => x.User, false)
                  .Subscribe(_ => Check());

            AdvertiseService.Current.WhenValueChanged(x => x.Advertisements, false)
                  .Subscribe(_ => Check());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            Check();
        }

        void Check()
        {
            if (UserService.Current.User != null && UserService.Current.IsSponsorUser)
            {
                RemoveAd();
            }
            if (AdvertiseService.Current.IsInitialized && !AdvertiseService.Current.Advertisements.Any_Nullable())
            {
                RemoveAd();
            }
        }

        void RemoveAd()
        {
            this.IsVisible = false;
        }
    }
}
