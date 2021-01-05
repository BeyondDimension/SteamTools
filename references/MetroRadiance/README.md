# MetroRadiance

[![Build status](https://img.shields.io/appveyor/ci/Grabacr07/MetroRadiance/master.svg?style=flat-square)](https://ci.appveyor.com/project/Grabacr07/MetroRadiance)
[![NuGet](https://img.shields.io/nuget/v/MetroRadiance.Core.svg?style=flat-square)](https://www.nuget.org/packages/MetroRadiance.Core/)
[![Downloads](https://img.shields.io/nuget/dt/MetroRadiance.Core.svg?style=flat-square)](https://www.nuget.org/packages/MetroRadiance.Core/)
[![License](https://img.shields.io/github/license/Grabacr07/MetroRadiance.svg?style=flat-square)](https://github.com/Grabacr07/MetroRadiance/blob/master/LICENSE.txt)

UI control libraries for create WPF window like Visual Studio 2012/2013/2015.

![ss150730085651kd](https://cloud.githubusercontent.com/assets/1779073/8972861/0e3eed28-3699-11e5-9bfe-18af42a6ed73.png)


## Installation

Install NuGet package(s).

```powershell
PM> Install-Package MetroRadiance
```

* [MetroRadiance.Core](https://www.nuget.org/packages/MetroRadiance.Core/) - MetroRadiance core library.
* [MetroRadiance.Chrome](https://www.nuget.org/packages/MetroRadiance.Chrome/) - Chrome library for WPF Window.
* [MetroRadiance](https://www.nuget.org/packages/MetroRadiance/) - WPF custom control library.


## Features / How to use

### MetroRadiance.Core

* DPI / Per-Monitor DPI support
  - Get system DPI
  - Get monitor DPI from HwndSource or window handle

```csharp
using MetroRadiance.Interop;
```

```csharp
// Get system dpi
var systemDpi = window.GetSystemDpi();

if (PerMonitorDpi.IsSupported)
{
    // Get monitor dpi.
    var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
    var monitorDpi = hwndSource.GetDpi();
}
```

* Windows theme support
  - Get Windows theme (Light or Dark, only Windows 10)
  - Get Windows accent color
  - Subscribe color change event from Windows

```csharp
using MetroRadiance.Platform;
```

```csharp
// Get Windows accent color
var color = WindowsTheme.GetAccentColor();

// Subscribe accent color change event from Windows theme.
var disposable = WindowsTheme.RegisterAccentColorListener(color =>
{
    // apply color to your app.
});

// Unsubscribe color change event.
disposable.Dispose();
```

* HSV color model support

```csharp
using MetroRadiance.Media;
```

```csharp
// Get Windows accent color (using MetroRadiance.Platform;)
var rgbColor = WindowsTheme.GetAccentColor();

// Convert from RGB to HSV color.
var hsvColor = rgbColor.ToHsv();
hsvColor.V *= 0.8;

// Convert from HSV to RGB color.
var newColor = hsvColor.ToRgb();
```

### MetroRadiance.Chrome

* Add window chrome like Visual Studio to WPF Window
  - `MetroRadiance.Chrome.WindowChrome`

```XAML
<Window xmlns:chrome="http://schemes.grabacr.net/winfx/2014/chrome">
    <chrome:WindowChrome.Instance>
        <chrome:WindowChrome />
    </chrome:WindowChrome.Instance>
</Window>
```

* Add any UI elements to window chrome
  - `MetroRadiance.Chrome.WindowChrome.Top` / `.Left` / `.Right` / `.Bottom`

```XAML
<Window xmlns:chrome="http://schemes.grabacr.net/winfx/2014/chrome">
    <chrome:WindowChrome.Instance>
        <chrome:WindowChrome>
            <chrome:WindowChrome.Top>
                <Border Background="DarkRed"
                        Padding="24,3"
                        Margin="8,0"
                        HorizontalAlignment="Right">
                    <TextBlock Text="any UI elements"
                               Foreground="White" />
                </Border>
            </chrome:WindowChrome.Top>
        </chrome:WindowChrome>
    </chrome:WindowChrome.Instance>
</Window>
```

![ss160128005316ls](https://cloud.githubusercontent.com/assets/1779073/12619010/9d8d6e24-c559-11e5-8be1-04aa0eba278f.png)

### MetroRadiance

* Theme support

```csharp
// Change theme.
ThemeService.Current.ChangeTheme(Theme.Dark);

// Change theme (sync Windows)
ThemeService.Current.ChangeTheme(Theme.Windows);

// Change Accent
ThemeService.Current.ChangeAccent(Accent.Blue);

// Change accent (sync Windows)
ThemeService.Current.ChangeAccent(Accent.Windows);

// Change accent (from RGB Color)
var accent = Colors.Red.ToAccent();
ThemeService.Current.ChangeAccent(accent);
```

* Custom controls
* Custom behaviors
* Custom converters


## License

This library is under [the MIT License (MIT)](LICENSE.txt).
