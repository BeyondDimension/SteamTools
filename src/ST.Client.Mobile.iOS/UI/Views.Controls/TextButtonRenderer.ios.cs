// https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Material.iOS/MaterialButtonRenderer.cs
using CoreGraphics;
using MaterialComponents;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Material.iOS;
using Xamarin.Forms.Platform.iOS;
using Button = Xamarin.Forms.Button;
using MDCButton = MaterialComponents.Button;

namespace System.Application.UI.Views.Controls
{
    partial class TextButtonRenderer : ViewRenderer<Button, MDCButton>, IImageVisualElementRenderer, IButtonLayoutRenderer
    {
        bool _isDisposed;
        ButtonScheme? _defaultButtonScheme;
        ButtonScheme? _buttonScheme;
        ButtonLayoutManager? _buttonLayoutManager;

        public TextButtonRenderer()
        {
            _buttonLayoutManager = new ButtonLayoutManager(this,
                preserveInitialPadding: true,
                spacingAdjustsPadding: false,
                borderAdjustsPadding: false,
                collapseHorizontalPadding: true);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (Control != null)
            {
                Control.TouchUpInside -= OnButtonTouchUpInside;
                Control.TouchDown -= OnButtonTouchDown;
                _buttonLayoutManager?.Dispose();
                _buttonLayoutManager = null;
            }

            _isDisposed = true;

            base.Dispose(disposing);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var measured = base.SizeThatFits(size);
            return _buttonLayoutManager?.SizeThatFits(size, measured) ?? measured;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            // recreate the scheme
            _buttonScheme?.Dispose();
            _buttonScheme = CreateButtonScheme();

            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                //e.NewElement.BorderColor = Color.Transparent;
                //e.NewElement.BorderWidth = 0;
                //e.NewElement.BackgroundColor = Color.Transparent;
                //e.NewElement.Background = Brush.Transparent;
                //e.NewElement.CornerRadius = 0;

                if (Control == null)
                {
                    _defaultButtonScheme = CreateButtonScheme();

                    SetNativeControl(CreateNativeControl());

                    Control.TouchUpInside += OnButtonTouchUpInside;
                    Control.TouchDown += OnButtonTouchDown;
                }

                UpdateFont();
                UpdateCornerRadius();
                UpdateTextColor();
                _buttonLayoutManager?.Update();
                ApplyTheme();
            }
        }

        protected virtual ButtonScheme CreateButtonScheme()
        {
            return new ButtonScheme
            {
                ColorScheme = MaterialColors.Light.CreateColorScheme(),
                ShapeScheme = new ShapeScheme(),
                TypographyScheme = new TypographyScheme(),
            };
        }

        protected virtual void ApplyTheme()
        {
            // Colors have to be re-applied to Character spacing
            _buttonLayoutManager?.UpdateText();

            Color textColor = Element.TextColor;

            if (textColor.IsDefault)
                Control.SetTitleColor(MaterialColors.Light.DisabledColor, UIControlState.Disabled);
            else
                Control.SetTitleColor(textColor.ToUIColor(), UIControlState.Disabled);
        }

        protected override MDCButton CreateNativeControl()
        {
            // https://material.io/components/buttons/ios#text-button
            // https://material.io/develop/ios/components/theming
            var containerScheme = new ContainerScheme();
            var button = new MDCButton();
            button.ApplyTextThemeWithScheme(containerScheme);
            return button;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var updatedTheme = false;
            if (e.PropertyName == Button.TextColorProperty.PropertyName)
            {
                UpdateTextColor();
                updatedTheme = true;
            }
            else if (e.PropertyName == Button.FontProperty.PropertyName)
            {
                UpdateFont();
                updatedTheme = true;
            }
            else if (e.PropertyName == Button.CornerRadiusProperty.PropertyName)
            {
                UpdateCornerRadius();
                updatedTheme = true;
            }

            if (updatedTheme)
                ApplyTheme();
        }

        protected override void SetAccessibilityLabel()
        {
            // If we have not specified an AccessibilityLabel and the AccessibilityLabel is currently bound to the Title,
            // exit this method so we don't set the AccessibilityLabel value and break the binding.
            // This may pose a problem for users who want to explicitly set the AccessibilityLabel to null, but this
            // will prevent us from inadvertently breaking UI Tests that are using Query.Marked to get the dynamic Title
            // of the Button.

            var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);
            if (string.IsNullOrWhiteSpace(elemValue) && Control?.AccessibilityLabel == Control?.Title(UIControlState.Normal))
                return;

            base.SetAccessibilityLabel();
        }

        void OnButtonTouchUpInside(object sender, EventArgs eventArgs)
        {
            Element?.SendReleased();
            Element?.SendClicked();
        }

        void OnButtonTouchDown(object sender, EventArgs eventArgs) => Element?.SendPressed();

        void UpdateCornerRadius()
        {
            int cornerRadius = Element.CornerRadius;

            if (cornerRadius == (int)Button.CornerRadiusProperty.DefaultValue)
            {
                _buttonScheme.CornerRadius = _defaultButtonScheme.CornerRadius;
            }
            else
            {
                _buttonScheme.CornerRadius = cornerRadius;
                if (_buttonScheme.ShapeScheme is ShapeScheme shapeScheme)
                {
                    shapeScheme.SmallComponentShape = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);
                    shapeScheme.MediumComponentShape = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);
                    shapeScheme.LargeComponentShape = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);
                }
            }
        }

        void UpdateFont()
        {
            if (_buttonScheme.TypographyScheme is TypographyScheme typographyScheme)
            {
                if (Element.Font == (Font)Button.FontProperty.DefaultValue)
                    typographyScheme.Button = _defaultButtonScheme.TypographyScheme.Button;
                else
                    typographyScheme.Button = Element.ToUIFont();
            }
        }

        void UpdateTextColor()
        {
            if (_buttonScheme.ColorScheme is SemanticColorScheme colorScheme)
            {
                Color textColor = Element.TextColor;

                if (textColor.IsDefault)
                    colorScheme.OnPrimaryColor = _defaultButtonScheme.ColorScheme.OnPrimaryColor;
                else
                    colorScheme.OnPrimaryColor = textColor.ToUIColor();
            }
        }

        // IImageVisualElementRenderer
        bool IImageVisualElementRenderer.IsDisposed => _isDisposed;
        void IImageVisualElementRenderer.SetImage(UIImage image) => _buttonLayoutManager.SetImage(image);
        UIImageView IImageVisualElementRenderer.GetImage() => Control?.ImageView;

        // IButtonLayoutRenderer
        UIButton IButtonLayoutRenderer.Control => Control;
        IImageVisualElementRenderer IButtonLayoutRenderer.ImageVisualElementRenderer => this;
        nfloat IButtonLayoutRenderer.MinimumHeight => _buttonScheme?.MinimumHeight ?? -1;
    }
}