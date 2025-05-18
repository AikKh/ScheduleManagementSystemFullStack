#!/bin/bash
# Download and install .NET SDK
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
export PATH="$HOME/.dotnet:$PATH"

# Build the Blazor app
dotnet publish ScheduleManagementSystem.Client/ScheduleManagementSystem.Client.csproj -c Release

# Copy the output to the publish directory
mkdir -p public
cp -r ScheduleManagementSystem.Client/bin/Release/net9.0/publish/wwwroot/* public/

# Add redirects for client-side routing
echo "/* /index.html 200" > public/_redirects