dotnet tool uninstall -g Quinntyne.Endpoint.Angular.Cli
dotnet pack -c Release
dotnet tool install --global --add-source ./nupkg Quinntyne.Endpoint.Angular.Cli --version 0.1.0
