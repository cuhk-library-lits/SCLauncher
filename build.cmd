@echo off

set PATH=%PATH%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319;

csc -target:winexe -out:SCLauncher.exe *.cs

pause
