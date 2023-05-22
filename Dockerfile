# Use the official Microsoft .NET Core SDK image as the build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory within the build container
WORKDIR /src

# Copy your source code to the build container
COPY . ./

#WTF i dont know why this is needed but it works


WORKDIR /src
# Restore NuGet packages
RUN dotnet restore

# Build the application in Release mode
RUN dotnet build --configuration Release --no-restore -o /app/build

# Publish the application
RUN dotnet publish --configuration Release -o /app/publish /p:UseAppHost=false

# Use the official Microsoft .NET Core runtime image as the base image for the final stage
FROM mcr.microsoft.com/dotnet/runtime:6.0

# Set the working directory within the container
WORKDIR /app

# Copy the published application files from the build container to the runtime container
COPY --from=build /app/publish .

# Set the entrypoint command to execute your console application
ENTRYPOINT ["dotnet", "FomoDog.dll"]
