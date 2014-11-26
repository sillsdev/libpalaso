set sayMoreDir=..\SayMore
copy /Y output\debug\Palaso.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.xml %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.TestUtilities.* %sayMoreDir%\lib\dotnet
copy /Y output\debug\PalasoUIWindowsForms.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.Media.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.Media.xml %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.Media.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\PalasoUIWindowsForms.xml %sayMoreDir%\lib\dotnet
copy /Y output\debug\PalasoUIWindowsForms.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\Palaso.BuildTasks.dll %sayMoreDir%\build

copy /Y %sayMoreDir%\lib\dotnet\Palaso*.* %sayMoreDir%\output\debug

pause