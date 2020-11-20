using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SteamTools.Views.Controls
{
    public class AppIcon : Control
    {
        static AppIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppIcon), new FrameworkPropertyMetadata(typeof(AppIcon)));
        }


        #region AnchorVisibility 依存関係プロパティ

        public Visibility AnchorVisibility
        {
            get { return (Visibility)this.GetValue(AnchorVisibilityProperty); }
            set { this.SetValue(AnchorVisibilityProperty, value); }
        }
        public static readonly DependencyProperty AnchorVisibilityProperty =
            DependencyProperty.Register(nameof(AnchorVisibility), typeof(Visibility), typeof(AppIcon), new UIPropertyMetadata(Visibility.Visible));

        #endregion

        #region BandVisibility 依存関係プロパティ

        public Visibility BandVisibility
        {
            get { return (Visibility)this.GetValue(BandVisibilityProperty); }
            set { this.SetValue(BandVisibilityProperty, value); }
        }
        public static readonly DependencyProperty BandVisibilityProperty =
            DependencyProperty.Register(nameof(BandVisibility), typeof(Visibility), typeof(AppIcon), new UIPropertyMetadata(Visibility.Visible));

        #endregion
    }
}
