echo off
set /P REBUILD=Do you want to rebuild now [y/n]? 
if "%REBUILD%"=="y" goto:REBUILD
goto:ENDBUILD
:REBUILD
set MSBUILD="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set MSTEST="C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe"

echo 1. Build
%MSBUILD% /t:Rebuild /p:Configuration=Release "..\INotify.sln"

echo 2. Run tests
%MSTEST% /testcontainer:..\INotify.Test\bin\Release\INotify.Test.dll /test:INotify.Test.Features
:ENDBUILD

set /P NUGET=Do you want to publish to NuGet now [y/n]? 
if /i "%NUGET%"=="y" goto:NUGET
goto:EOF

:NUGET
NOTEPAD INotify.nuspec
echo 3. Create NuGet package
xcopy INotify.nuspec ..\INotify\bin\Release\
move /Y ..\INotify\bin\Release\INotify.dll ..\INotify\bin\Release\lib\net40\
nuget pack ..\INotify\bin\Release\INotify.nuspec

:VERSION
set /P VERSION=Enter version: 
if /i "%VERSION%"=="" goto:VERSION
set PACKAGE=INotify.%VERSION%.nupkg
echo 4. Publish NuGet package
nuget push %PACKAGE%