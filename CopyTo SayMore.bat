set sayMoreDir=..\SayMore

copy /Y output\debug\NAudio.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\Ionic.Zip.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\icu.net.dll %sayMoreDir%\lib\dotnet
copy /Y output\Debug\lib\win-x86\icudt56.dll %sayMoreDir%\lib\dotnet
copy /Y output\Debug\lib\win-x86\icuin56.dll %sayMoreDir%\lib\dotnet
copy /Y output\Debug\lib\win-x86\icuuc56.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\L10NSharp.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\L10NSharp.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Core.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Core.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Core.Desktop.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Core.Desktop.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Media.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Media.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Windows.Forms.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Windows.Forms.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Archiving.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Archiving.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.TestUtilities.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.TestUtilities.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.WritingSystems.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.WritingSystems.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Windows.Forms.WritingSystems.dll %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.Windows.Forms.WritingSystems.pdb %sayMoreDir%\lib\dotnet
copy /Y output\debug\SIL.BuildTasks.dll %sayMoreDir%\build

copy /Y %sayMoreDir%\lib\dotnet\SIL.* %sayMoreDir%\output\Debug
copy /Y %sayMoreDir%\lib\dotnet\L10* %sayMoreDir%\output\Debug
copy /Y %sayMoreDir%\lib\dotnet\icu* %sayMoreDir%\output\Debug
copy /Y %sayMoreDir%\lib\dotnet\Naudio* %sayMoreDir%\output\Debug
copy /Y %sayMoreDir%\lib\dotnet\Ionic* %sayMoreDir%\output\Debug

pause