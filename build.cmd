@echo off

set PATH=%PATH%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319;

if not exist "bin" mkdir bin
csc -target:winexe -out:bin\SCLauncher.exe *.cs

pause
