@echo off
REM Copyright (c) Quinntyne Brown. All Rights Reserved.
REM Licensed under the MIT License. See License.txt in the project root for license information.

echo Running EventDrivenMicroservices project...
echo Output directory: C:\out

REM Check if output directory exists, create if it doesn't
if not exist "C:\out" (
    echo Creating output directory: C:\out
    mkdir "C:\out"
)

dotnet run --project EventDrivenMicroservices\EventDrivenMicroservices.csproj C:\out
