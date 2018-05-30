@ECHO OFF

set DOTNETFX4=%systemroot%\Microsoft.NET\Framework\v4.0.30319\
set PATH=%PATH%;%DOTNETFX4%

echo Installing WindowsService...
echo ---------------------------------------------------
InstallUtil /ServiceName=CnE.Scheduler  -u CNE.Scheduler.exe
InstallUtil /ServiceName=CnE.Scheduler /i CNE.Scheduler.exe
echo ---------------------------------------------------
echo Done.