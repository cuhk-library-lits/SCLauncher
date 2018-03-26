@echo off

set PATH=%PATH%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319;

if not exist "bin" mkdir bin

csc -lib:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0" ^
    -target:winexe ^
    -out:bin\SCLauncherShell.exe ^
    -main:CUHKSelfCheckLauncher.SCLauncherShell ^
    *.cs

csc -lib:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0" ^
    -target:winexe ^
    -out:bin\SCLauncher.exe ^
    -main:CUHKSelfCheckLauncher.SCLauncher ^
    *.cs

