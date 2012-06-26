@echo off

set CSHARP_BIN_DIR=C:\Windows\Microsoft.NET\Framework\v3.5
set PATH=%CSHARP_BIN_DIR%;%PATH%

%~d0
cd "%~dp0"

for /F %%l in (.\po\locales.lst) do (
   if exist "%~dp0.\po\%%l.po" (
		echo.Compiling satellite assembly. Locale "%%l"
		"%~dp0..\..\Bin\Debug\GNU.Gettext.Msgfmt.exe" -i.\po\%%l.po -l%%l -d.\Bin\Debug -bExamples.Hello.Messages -ccsc -L"%~dp0..\..\Bin\Debug" -v
		if errorlevel 1 exit /b 1
   )
)
exit /b 0


