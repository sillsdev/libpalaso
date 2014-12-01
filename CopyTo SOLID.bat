set solidDir=..\solid
copy /Y output\debug\*.dll %solidDir%\lib
copy /Y output\debug\*.pdb %solidDir%\lib
copy /Y output\debug\*.xml %solidDir%\lib

copy /Y output\debug\*.dll %solidDir%\output\debug
copy /Y output\debug\*.pdb %solidDir%\output\debug
copy /Y output\debug\*.xml %solidDir%\output\debug


pause