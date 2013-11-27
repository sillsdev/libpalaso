
copy /Y output\debug\palaso.dll c:\dev\chorus\lib\debug
copy /Y output\debug\palaso.xml c:\dev\chorus\lib\debug
copy /Y output\debug\palaso.pdb c:\dev\chorus\lib\debug

copy /Y output\debug\palasouiwindowsforms.dll  c:\dev\chorus\lib\debug
copy /Y output\debug\palasouiwindowsforms.xml  c:\dev\chorus\lib\debug
copy /Y output\debug\palasouiwindowsforms.pdb  c:\dev\chorus\lib\debug


copy /Y output\debug\palaso.testutilities.dll c:\dev\chorus\lib\debug
copy /Y output\debug\palaso.testutilities.xml c:\dev\chorus\lib\debug
copy /Y output\debug\palaso.testutilities.pdb c:\dev\chorus\lib\debug

copy /Y output\debug\palaso.*  c:\dev\chorus\output\debug
copy /Y output\debug\palaso.testutilities.*  c:\dev\chorus\output\debug
copy /Y output\debug\palasouiwindowsforms.*  c:\dev\chorus\output\debug

copy /Y output\debug\palaso*.* c:\dev\chorus\lib\release
copy /Y output\debug\palaso*.* c:\dev\chorus\output\release

pause