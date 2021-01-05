@echo off

dotnet test "%APPVEYOR_BUILD_FOLDER%\tests\Titanium.Web.Proxy.IntegrationTests\Titanium.Web.Proxy.IntegrationTests.csproj" -c %CONFIGURATION%
dotnet test "%APPVEYOR_BUILD_FOLDER%\tests\Titanium.Web.Proxy.UnitTests\Titanium.Web.Proxy.UnitTests.csproj" -c %CONFIGURATION%
exit /B %errorlevel%