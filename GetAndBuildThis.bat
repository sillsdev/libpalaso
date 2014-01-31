IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

IF "%2"=="" (
	call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
)

git pull --rebase
msbuild "Palaso VS2010.sln" /verbosity:quiet /maxcpucount /p:Configuration=%BUILD_CONFIG%
