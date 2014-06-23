function ExitWithCode 
{ 
    param 
    ( 
        $exitcode 
    )

    $host.SetShouldExit($exitcode) 
    exit 
}

[string] $VSCOMNTOOLS
[string] $Architecture = (Get-Process -Id $PID).StartInfo.EnvironmentVariables["PROCESSOR_ARCHITECTURE"];
if(Test-Path ${env:VS120COMNTOOLS})
{
    $VSCOMNTOOLS = ${env:VS120COMNTOOLS}
}
elseif(Test-Path ${env:VS110COMNTOOLS})
{
    $VSCOMNTOOLS = ${env:VS110COMNTOOLS}
}
elseif(Test-Path ${env:VS100COMNTOOLS})
{
    $VSCOMNTOOLS = ${env:VS100COMNTOOLS}
}
elseif(Test-Path ${env:VS90COMNTOOLS})
{
    $VSCOMNTOOLS = ${env:VS90COMNTOOLS}
}
else
{
    Write-Error "Visual Studio Environment Variable not found"
    ExitWithCode 1
}

[string] $VSCOMNTOOLSPATH = "$VSCOMNTOOLS..\..\VC\"
pushd "$VSCOMNTOOLSPATH"
cmd /c "vcvarsall.bat&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
  }
}
popd
write-host "`nVisual Studio Command Prompt variables set." -ForegroundColor Yellow

$Root = Get-Item (Split-Path $MyInvocation.MyCommand.Path -Parent)
Write-Host $Root
$Solution = Get-ChildItem $Root "PK.Common.sln" -File
Write-Host $Solution.FullName
$TestResult = Get-ChildItem $Root "TestResults" -Directory
Write-Host $TestResult.FullName
$Sources = Get-ChildItem $Root "Sources" -Directory
Write-Host $Sources.FullName
$Tests = Get-ChildItem $Root "Tests" -Directory
Write-Host $Tests.FullName
$Package = Get-ChildItem $Root "Package" -Directory
Write-Host $Package.FullName
$Packages = Get-ChildItem $Root "packages" -Directory
Write-Host $Packages.FullName

if(Get-Command "NuGet.exe" -ErrorAction	SilentlyContinue)
{
    $NuGet = (Get-Command "NuGet.exe").Path
}
else
{
    $NuGet = Get-ChildItem $Root -Recurse -File | Where-Object { $_.Name -EQ "NuGet.exe" } | Select-Object -First 1
    if($NuGet)
    {
        $NuGet = $NuGet.FullName
    }
    else
    {
        Invoke-WebRequest https://www.nuget.org/nuget.exe -OutFile $env:temp\NuGet.exe
        $NuGet = (Get-Item $env:temp\NuGet.exe).FullName
    }
}
Write-Host "Using: $NuGet"

$PKCommon = Get-ChildItem $Sources "PK.Common" -Directory
Write-Host $PKCommon.FullName
$PKCommonProj = Get-ChildItem $PKCommon.FullName "PK.Common.csproj" -File
Write-Host $PKCommonProj.FullName

#goto :eof
& $NuGet restore $Solution.FullName

& msbuild /t:Pack /p:Configuration=Release $PKCommonProj.FullName /p:NuGetExePath=$NuGet