set myWorkSafeDir=..\myWorkSafe
copy /Y output\debug\Palaso.dll %myWorkSafeDir%\lib
copy /Y output\debug\Palaso.xml %myWorkSafeDir%\lib
copy /Y output\debug\Palaso.pdb %myWorkSafeDir%\lib
copy /Y output\debug\Palaso.TestUtilities.* %myWorkSafeDir%\lib
copy /Y output\debug\PalasoUIWindowsForms.dll %myWorkSafeDir%\lib
copy /Y output\debug\PalasoUIWindowsForms.xml %myWorkSafeDir%\lib
copy /Y output\debug\PalasoUIWindowsForms.pdb %myWorkSafeDir%\lib
copy /Y output\debug\Palaso.BuildTasks.dll %myWorkSafeDir%\build

copy /Y %myWorkSafeDir%\lib\Palaso*.* %myWorkSafeDir%\output\debug

pause