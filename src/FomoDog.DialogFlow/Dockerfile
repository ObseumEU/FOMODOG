# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the SDK image to build the project files
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the necessary project files
COPY ["FomoDog.DialogFlow/FomoDog.DialogFlow.csproj", "FomoDog.DialogFlow/FomoDog.DialogFlow.csproj"]
COPY ["FomoDog.MessageTypes/FomoDog.MessageTypes.csproj", "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj"]

# Restore for the specific projects
RUN dotnet restore "FomoDog.DialogFlow/FomoDog.DialogFlow.csproj"
RUN dotnet restore "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj"

COPY . .

ARG RUN_TESTS
RUN if [ "$RUN_TESTS" = "true" ]; then \
      dotnet test "FomoDog.DialogFlow/FomoDog.DialogFlow.Tests/FomoDog.DialogFlow.Tests.csproj" --collect:"XPlat Code Coverage" --results-directory:"/app/coverage/"; \
    fi

# Build the projects
RUN dotnet build "FomoDog.DialogFlow/FomoDog.DialogFlow.csproj" -c Release -o /app/build
RUN dotnet build "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj" -c Release -o /app/build

# Publish the projects
FROM build AS publish
RUN dotnet publish "FomoDog.DialogFlow/FomoDog.DialogFlow.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/coverage ./coverage
ENTRYPOINT ["dotnet", "FomoDog.DialogFlow.dll"]