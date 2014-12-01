IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

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


:build
git pull --rebase
msbuild "Palaso.sln" /verbosity:quiet /maxcpucount /p:Configuration=%BUILD_CONFIG%
