@echo off
dotnet tool uninstall -g Quinntyne.Endpoint.Cli
cd ..\..
dotnet pack -c Release
dotnet tool install --global --add-source .\src\Endpoint.Cli\nupkg Quinntyne.Endpoint.Cli --version 0.3.1
