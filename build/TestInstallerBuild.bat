@echo off
if "%INCLUDE%%LIB%" == "" (

	if not "%VS120COMNTOOLS%" == "" (
		echo Setting up Visual Studio Pro 2013 Tools...
		@call "%VS120COMNTOOLS%vsvars32.bat"
		goto build
	)

	if not "%VS100COMNTOOLS%" == "" (
		echo Setting up Visual Studio Pro 2010 Tools...
		@call "%VS100COMNTOOLS%vsvars32.bat"
		goto build
	)

	if not "%VS110COMNTOOLS%" == "" (
		echo Setting up Visual Studio Express 2010 Tools...
		@call "%VS110COMNTOOLS%vsvars32.bat"
		goto build
	)
)
:build
if "%~1" == "" (
	SET BUILD=Release
) else (
	SET BUILD=%~1
)
@echo on
@pushd "%~dp0"

msbuild Palaso.proj /target:build /property:Configuration="%BUILD%" /property:Minor="1"

@popd