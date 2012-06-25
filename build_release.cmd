@echo off

call "%~pd0.\set_env.cmd"
if errorlevel 1 (
	echo.!!!
	echo.Rename file "set_env.cmd.template" to "set_env.cmd" and edit it to corect path for Mono
	echo.!!!
	goto batch_failed
)

set COMMON_BUILD_OPTIONS=/property:configuration=Release /target:Build /verbosity:quiet
call:BuildProject "%~pd0.\GNU.Gettext\GNU.Gettext.sln"
if errorlevel 1 goto batch_failed

goto batch_ok

:batch_failed
echo Build failed
exit /b 1

:batch_ok
echo Built OK
exit /b 0

:BuildProject
call "%MONO_BIN%\xbuild" "%~1" %COMMON_BUILD_OPTIONS%
goto:eof