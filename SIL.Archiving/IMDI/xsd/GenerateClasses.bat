@echo off
rem for /f "tokens=2-8 delims=.:/ " %%a in ("%date% %time%") do set DateNtime=%%c-%%a-%%b_%%d-%%e-%%f.%%g
for /f "tokens=2-8 delims=.:/ " %%a in ("%date% %time%") do set DateNtime=%%c%%a%%b%%d%%e%%f
echo Renaming existing file...
rename "C:\Projects\Palaso\SIL.Windows.Forms.Archiving\IMDI\Schema\IMDI_3_0.cs" "IMDI_3_0.cs.%DateNtime%.bak"
echo.
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\x64\xsd.exe" -c -l:c# -n:IMDI.Schema "C:\Projects\Palaso\SIL.Windows.Forms.Archiving\IMDI\xsd\IMDI_3.0.xsd" -o:"C:\Projects\Palaso\SIL.Windows.Forms.Archiving\IMDI\Schema"
echo.
echo Fixing output file...
cd C:\Projects\Palaso\SIL.Windows.Forms.Archiving\IMDI\Schema
c:\python27\python imdi_3_0_fix.py
pause