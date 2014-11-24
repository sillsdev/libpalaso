set weSayDir=..\wesayDev
copy /Y output\debug\*.dll %weSayDir%\lib\debug
copy /Y output\debug\*.pdb %weSayDir%\lib\debug
copy /Y output\debug\Palaso.BuildTasks.dll %weSayDir%\bld

copy /Y %weSayDir%\lib\debug\palaso*.* %weSayDir%\output\debug


pause