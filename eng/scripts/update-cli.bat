dotnet tool uninstall -g Quinntyne.Endpoint.Engineering.Cli
cd /d %~dp0..\..
dotnet pack src\Endpoint.Engineering.Cli\Endpoint.Engineering.Cli.csproj -c Release
dotnet tool install --global --add-source .\src\Endpoint.Engineering.Cli\nupkg Quinntyne.Endpoint.Engineering.Cli --version 0.3.1
