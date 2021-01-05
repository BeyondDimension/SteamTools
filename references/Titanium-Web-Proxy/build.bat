@echo off

powershell -NoProfile -ExecutionPolicy bypass -Command "%~dp0.build\setup.ps1 %*; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%