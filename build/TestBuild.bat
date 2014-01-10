@echo off
if "%INCLUDE%%LIB%" == "" (
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

msbuild "/target:Clean;Compile" /property:Configuration=Release /property:RootDir=..  /property:BUILD_NUMBER="0.0.0.abcd" build.win.proj
msbuild "/target:Compile" /property:Configuration=Debug /property:RootDir=..  /property:BUILD_NUMBER="0.0.0.abcd" build.win.proj
