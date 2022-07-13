using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using System;
using System.Collections.Generic;

namespace System.Application.UI
{
    /// <summary>
    /// Includes the fluent theme in an application.
    /// </summary>
    public class CustomTheme : AvaloniaObject, IStyle, IResourceProvider
    {
        private readonly Uri _baseUri;
        private Styles _sharedStyles = new();
        //private Styles _fluentDark = new();
        //private Styles _fluentLight = new();
        private bool _isLoading;
        private IStyle? _loaded;
        private readonly ResourceDictionary _themeResources = new ResourceDictionary();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTheme"/> class.
        /// </summary>
        /// <param name="baseUri">The base URL for the XAML context.</param>
        public CustomTheme(Uri baseUri)
        {
            _baseUri = baseUri;
            InitStyles(baseUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTheme"/> class.
        /// </summary>
        /// <param name="serviceProvider">The XAML service provider.</param>
        public CustomTheme(IServiceProvider serviceProvider)
        {
            _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
            InitStyles(_baseUri);
        }

        public static readonly StyledProperty<string> ModeProperty =
            AvaloniaProperty.Register<CustomTheme, string>(nameof(Mode), "Dark");

        /// <summary>
        /// Gets or sets the mode of the fluent theme (light, dark).
        /// </summary>
        public string Mode
        {
            get => GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged<T>(change);

            if (_loaded is null)
            {
                // If style wasn't yet loaded, no need to change children styles,
                // it will be applied later in Loaded getter.
                return;
            }

            if (change.Property == ModeProperty)
            {
                //if (Mode == FluentAvaloniaTheme.DarkModeString)
                //{
                //    //(Loaded as Styles)![0] = _fluentDark[0];
                //}
                //else
                //{
                //    //(Loaded as Styles)![0] = _fluentLight[0];
                //}

                // Remove the old theme of any resources

                _themeResources.MergedDictionaries[0] =
                    (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri($"avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/Theme{Mode}.xaml"), _baseUri);

                Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
            }
        }

        public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

        /// <summary>
        /// Gets the loaded style.
        /// </summary>
        public IStyle Loaded
        {
            get
            {
                if (_loaded == null)
                {
                    _isLoading = true;

                    if (Mode == FluentAvaloniaTheme.LightModeString)
                    {
                        _loaded = new Styles() { _sharedStyles };
                    }
                    else
                    {
                        _loaded = new Styles() { _sharedStyles };
                    }

                    _isLoading = false;
                }

                return _loaded!;
            }
        }

        bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

        IReadOnlyList<IStyle> IStyle.Children => _loaded?.Children ?? Array.Empty<IStyle>();

        public event EventHandler? OwnerChanged
        {
            add
            {
                if (Loaded is IResourceProvider rp)
                {
                    rp.OwnerChanged += value;
                }
            }

            remove
            {
                if (Loaded is IResourceProvider rp)
                {
                    rp.OwnerChanged -= value;
                }
            }
        }

        public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

        public bool TryGetResource(object key, out object? value)
        {
            // Github build failing with this not being set, even tho it passes locally
            value = null;

            // We also search the app level resources so resources can be overridden.
            // Do not search App level styles though as we'll have to iterate over them
            // to skip the FluentAvaloniaTheme instance or we'll stack overflow
            if (Avalonia.Application.Current?.Resources.TryGetResource(key, out value) == true)
                return true;

            // This checks the actual ResourceDictionary where the SystemResources are stored
            // and checks the merged dictionaries where base resources and theme resources are
            if (_themeResources.TryGetResource(key, out value))
                return true;

            if (!_isLoading && Loaded is IResourceProvider p)
            {
                return p.TryGetResource(key, out value);
            }

            value = null;
            return false;
        }

        void IResourceProvider.AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);

        void IResourceProvider.RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

        private void InitStyles(Uri baseUri)
        {
            AvaloniaLocator.CurrentMutable.Bind<CustomTheme>().ToConstant(this);

            _sharedStyles = new Styles
            {
                new StyleInclude(baseUri)
                {
                    Source = new Uri("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/ThemeAccent.xaml")
                },
                new StyleInclude(baseUri)
                {
                    Source = new Uri("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/Controls.xaml")
                },
            };

            //_fluentLight = new Styles
            //{
            //    new StyleInclude(baseUri)
            //    {
            //        Source = new Uri("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/ThemeLight.xaml")
            //    }
            //};

            //_fluentDark = new Styles
            //{
            //    new StyleInclude(baseUri)
            //    {
            //        Source = new Uri("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/ThemeDark.xaml")
            //    }
            //};

            // Theme resource colors/brushes
            _themeResources.MergedDictionaries.Add(
                (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri($"avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/Theme{Mode}.xaml"), _baseUri));
        }
    }
}
