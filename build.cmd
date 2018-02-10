@echo off

set PATH=%PATH%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319;

if not exist "bin" mkdir bin

csc -lib:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0" ^
    -reference:UIAutomationClient.dll;UIAutomationClientsideProviders.dll;UIAutomationProvider.dll;UIAutomationTypes.dll ^
    -target:winexe ^
    -out:bin\SCLauncher.exe ^
    *.cs

