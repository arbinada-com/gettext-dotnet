@echo off

call "%~pd0.\set_env.cmd"
if errorlevel 1 (
	echo.!!!
	echo.Rename file "set_env.cmd.template" to "set_env.cmd" and edit it to corect path for .NET framework
	echo.!!!
	goto batch_failed
)

set COMMON_BUILD_OPTIONS=/target:Build /verbosity:quiet
call "%DOTNET_HOME%\MSBuild" "%~pd0.\GNU.Gettext\GettextDotNetVS2010.sln" %COMMON_BUILD_OPTIONS% /property:configuration=Debug
if errorlevel 1 goto batch_failed
call "%DOTNET_HOME%\MSBuild" "%~pd0.\GNU.Gettext\GettextDotNetVS2010.sln" %COMMON_BUILD_OPTIONS% /property:configuration=Release
if errorlevel 1 goto batch_failed

goto batch_ok

:batch_failed
echo Build failed
exit /b 1

:batch_ok
echo Built OK
exit /b 0
