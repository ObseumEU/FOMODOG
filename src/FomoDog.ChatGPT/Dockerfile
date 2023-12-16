# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the SDK image to build the project files
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the necessary project files
COPY ["FomoDog.ChatGPT/FomoDog.ChatGPT.csproj", "FomoDog.ChatGPT/FomoDog.ChatGPT.csproj"]
COPY ["FomoDog.MessageTypes/FomoDog.MessageTypes.csproj", "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj"]

# Restore for the specific projects
RUN dotnet restore "FomoDog.ChatGPT/FomoDog.ChatGPT.csproj"
RUN dotnet restore "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj"

COPY . .

# Build the projects
RUN dotnet build "FomoDog.ChatGPT/FomoDog.ChatGPT.csproj" -c Release -o /app/build
RUN dotnet build "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj" -c Release -o /app/build

# Publish the projects
FROM build AS publish
RUN dotnet publish "FomoDog.ChatGPT/FomoDog.ChatGPT.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FomoDog.dll"]