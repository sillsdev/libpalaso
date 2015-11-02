echo off
IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

set chorusDir=..\chorus

echo on
echo Copying some %BUILD_CONFIG% files to Chorus

copy /Y output\%BUILD_CONFIG%StrongName\SIL.Core.dll %chorusDir%\lib\%BUILD_CONFIG%
copy /Y output\%BUILD_CONFIG%StrongName\SIL.Core.pdb %chorusDir%\lib\%BUILD_CONFIG%

copy /Y output\%BUILD_CONFIG%StrongName\SIL.Lift.dll %chorusDir%\lib\%BUILD_CONFIG%
copy /Y output\%BUILD_CONFIG%StrongName\SIL.Lift.pdb %chorusDir%\lib\%BUILD_CONFIG%

copy /Y output\%BUILD_CONFIG%StrongName\SIL.Testutilities.dll %chorusDir%\lib\%BUILD_CONFIG%
copy /Y output\%BUILD_CONFIG%StrongName\SIL.Testutilities.pdb %chorusDir%\lib\%BUILD_CONFIG%

copy /Y output\%BUILD_CONFIG%StrongName\SIL.windows.Forms.dll  %chorusDir%\lib\%BUILD_CONFIG%
copy /Y output\%BUILD_CONFIG%StrongName\SIL.Windows.Forms.pdb  %chorusDir%\lib\%BUILD_CONFIG%

copy /Y output\%BUILD_CONFIG%StrongName\SIL.windows.Forms.GeckoBrowserAdapter.dll  %chorusDir%\lib\%BUILD_CONFIG%
copy /Y output\%BUILD_CONFIG%StrongName\SIL.Windows.Forms.GeckoBrowserAdapter.pdb  %chorusDir%\lib\%BUILD_CONFIG%

pause