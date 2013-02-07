copy /Y output\debug\*.dll c:\dev\bloom\lib\dotnet
copy /Y output\debug\*.xml c:\dev\bloom\lib\dotnet
copy /Y output\debug\*.pdb c:\dev\bloom\lib\dotnet
copy /Y output\debug\Palaso.BuildTasks.dll c:\dev\bloom\build

copy /Y c:\dev\bloom\lib\dotnet\palaso*.* c:\dev\bloom\output\debug

pause