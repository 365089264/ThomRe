@ECHO OFF

set DOTNETFX4=%systemroot%\Microsoft.NET\Framework\v4.0.30319\
set PATH=%PATH%;%DOTNETFX4%

echo Installing WindowsService...
echo ---------------------------------------------------
InstallUtil /ServiceName=VAV.Scheduler  -u VAV.Scheduler.exe
InstallUtil /ServiceName=VAV.Scheduler /i VAV.Scheduler.exe
echo ---------------------------------------------------
echo Done.