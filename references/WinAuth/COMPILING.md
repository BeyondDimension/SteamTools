# How to compile WinAuth

## Windows (Visual Studio) 

### Prerequisites

* Visual Studio 2017
* Windows 8.1 SDK

### Installation

1. Install Visual Studio 2017. In the installer's component selection "Individual components", make sure to enable "Windows 8.1 SDK". See [screenshot](https://stackoverflow.com/questions/43704734/how-to-fix-the-error-windows-sdk-version-8-1-was-not-found/43888773#43888773).
2. Git clone or download and extract the codebase.

### Building

1. Before starting, make sure to backup your `%AppData%\WinAuth\winauth.xml` file or export your authenticators to prevent data loss.
2. Open the solution file `Net4.5\WinAuth.sln` or `Net3.5\WinAuth-Net3.5.sln` in Visual Studio.
3. Visual Studio > Build > Build Solution.
4. If successful, the executable location should be `bin\Debug\WinAuth.exe` in the solution directory.
