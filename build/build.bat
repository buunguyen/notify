echo off
set MSBUILD="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set MSTEST="C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe"

rem ************** Build ************** 
set /P BUILD=Do you want to build now [y/n]? 
if "%BUILD%"=="y" goto BUILD
goto END_BUILD

:BUILD
echo *** Building...
%MSBUILD% /t:Rebuild /p:Configuration=Release "..\src\Notify.sln"
if errorlevel 1 goto BUILD_FAIL

echo *** Running tests...
%MSTEST% /testcontainer:..\src\Notify.Test\bin\Release\Notify.Test.dll /test:Notify.Test.Features
if errorlevel 1 goto TEST_FAIL
:END_BUILD

rem ************** NuGet ************** 
set /P NUGET=Do you want to publish to NuGet now [y/n]? 
if /i "%NUGET%"=="y" goto NUGET
goto END

:NUGET
NOTEPAD Notify.nuspec
echo *** Creating NuGet package
xcopy Notify.nuspec ..\src\Notify\bin\Release\
mkdir ..\src\Notify\bin\Release\lib\net40\ 
move /Y ..\src\Notify\bin\Release\Notify.dll ..\src\Notify\bin\Release\lib\net40\
move /Y ..\src\Notify\bin\Release\Notify.xml ..\src\Notify\bin\Release\lib\net40\
nuget pack ..\src\Notify\bin\Release\Notify.nuspec
if errorlevel 1 goto PACK_FAIL

:VERSION
set /P VERSION=Enter version: 
if /i "%VERSION%"=="" goto VERSION
set PACKAGE=Notify.%VERSION%.nupkg
echo *** Publishing NuGet package...
nuget push %PACKAGE%
goto END

:BUILD_FAIL
echo *** BUILD FAILED ***
goto END

:TEST_FAIL
echo *** TEST FAILED ***
goto END

:PACK_FAIL
echo *** PACKING FAILED ***
goto END

:END