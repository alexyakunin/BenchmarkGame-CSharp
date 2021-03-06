@echo off
dotnet build -c Release -f netcoreapp1.1
pushd bin\Release\netcoreapp1.1
echo.
echo Execution time, ms:
for /l %%x in (1, 1, 5) do (
  powershell Measure-Command {dotnet RegexRedux.dll ^< ..\..\..\..\_Data\RegexRedux.txt} ^| Select -ExpandProperty TotalMilliseconds
)
popd
