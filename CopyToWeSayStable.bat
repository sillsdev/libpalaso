set weSayDir=..\wesayStable
copy /Y output\debug\*.dll %weSayDir%\lib\net2.0\
copy /Y output\debug\*.pdb %weSayDir%\lib\net2.0\

copy /Y output\debug\*.dll %weSayDir%\output\debug
copy /Y output\debug\*.pdb %weSayDir%\output\debug

copy /Y output\debug\Palaso.BuildTasks.dll %weSayDir%\bld

pause