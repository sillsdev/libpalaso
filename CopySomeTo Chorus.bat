set chorusDir=..\chorus
copy /Y output\debug\palaso.dll %chorusDir%\lib\debug
copy /Y output\debug\palaso.xml %chorusDir%\lib\debug
copy /Y output\debug\palaso.pdb %chorusDir%\lib\debug

copy /Y output\debug\palasouiwindowsforms.dll  %chorusDir%\lib\debug
copy /Y output\debug\palasouiwindowsforms.xml  %chorusDir%\lib\debug
copy /Y output\debug\palasouiwindowsforms.pdb  %chorusDir%\lib\debug


copy /Y output\debug\palaso.testutilities.dll %chorusDir%\lib\debug
copy /Y output\debug\palaso.testutilities.xml %chorusDir%\lib\debug
copy /Y output\debug\palaso.testutilities.pdb %chorusDir%\lib\debug

copy /Y output\debug\palaso.*  %chorusDir%\output\debug
copy /Y output\debug\palaso.testutilities.*  %chorusDir%\output\debug
copy /Y output\debug\palasouiwindowsforms.*  %chorusDir%\output\debug

copy /Y output\debug\palaso*.* %chorusDir%\lib\release
copy /Y output\debug\palaso*.* %chorusDir%\output\release

pause