@echo off
dotnet build -c Release -f netcoreapp1.1
pushd bin\Release\netcoreapp1.1
echo.
echo Execution time, ms:
for /l %%x in (1, 1, 3) do (
  powershell Measure-Command {dotnet Fasta-Original.dll 1000000 ^> ..\..\..\Output.txt} ^| Select -ExpandProperty TotalMilliseconds
)
popd
