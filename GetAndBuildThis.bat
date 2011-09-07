call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
hg pull -u --rebase
msbuild "Palaso VS2010.sln" /verbosity:quiet /maxcpucount
