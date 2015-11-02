echo off
IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

set weSayDir=..\WeSay

echo on
echo Copying %BUILD_CONFIG% files to WeSay


copy /Y output\%BUILD_CONFIG%StrongName\*.dll %weSayDir%\lib\%BUILD_CONFIG%
copy /Y output\%BUILD_CONFIG%StrongName\*.pdb %weSayDir%\lib\%BUILD_CONFIG%

pause