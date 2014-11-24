set pdfDropletDir=..\pdfDropletDir
copy /Y output\debug\Palaso.dll %pdfDropletDir%\lib
copy /Y output\debug\Palaso.xml %pdfDropletDir%\lib
copy /Y output\debug\Palaso.pdb %pdfDropletDir%\lib
copy /Y output\debug\Palaso.TestUtilities.* %pdfDropletDir%\lib
REM copy /Y output\debug\PalasoUIWindowsForms.dll %pdfDropletDir%\lib
REM copy /Y output\debug\PalasoUIWindowsForms.xml %pdfDropletDir%\lib
REM copy /Y output\debug\PalasoUIWindowsForms.pdb %pdfDropletDir%\lib
copy /Y output\debug\Palaso.BuildTasks.dll %pdfDropletDir%\build

copy /Y %pdfDropletDir%\lib\Palaso.dll %pdfDropletDir%\output\debug

pause