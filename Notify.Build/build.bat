echo off
set MSBUILD="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set MSTEST="C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe"

rem ************** Rebuild ************** 
set /P REBUILD=Do you want to rebuild now [y/n]? 
if "%REBUILD%"=="y" goto:REBUILD
goto:ENDBUILD
:REBUILD

echo 1. Build
%MSBUILD% /t:Rebuild /p:Configuration=Release "..\Notify.sln"

echo 2. Run tests
%MSTEST% /testcontainer:..\Notify.Test\bin\Release\Notify.Test.dll /test:Notify.Test.Features
:ENDBUILD

rem ************** NuGet ************** 
set /P NUGET=Do you want to publish to NuGet now [y/n]? 
if /i "%NUGET%"=="y" goto:NUGET
goto:EOF

:NUGET
NOTEPAD Notify.nuspec
echo 3. Create NuGet package
xcopy Notify.nuspec ..\Notify\bin\Release\
mkdir ..\Notify\bin\Release\lib\net40\ 
move /Y ..\Notify\bin\Release\Notify.dll ..\Notify\bin\Release\lib\net40\
move /Y ..\Notify\bin\Release\Notify.xml ..\Notify\bin\Release\lib\net40\
nuget pack ..\Notify\bin\Release\Notify.nuspec

:VERSION
set /P VERSION=Enter version: 
if /i "%VERSION%"=="" goto:VERSION
set PACKAGE=Notify.%VERSION%.nupkg
echo 4. Publish NuGet package
nuget push %PACKAGE%