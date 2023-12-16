# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the SDK image to build the project files
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the necessary project files
COPY ["FomoDog/FomoDog.csproj", "FomoDog/FomoDog.csproj"]
COPY ["FomoDog/FomoDog.Tests/FomoDog.Tests.csproj", "FomoDog/FomoDog.Tests/FomoDog.Tests.csproj"]
COPY ["FomoDog.MessageTypes/FomoDog.MessageTypes.csproj", "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj"]

# Restore for the specific projects
RUN dotnet restore "FomoDog/FomoDog.csproj"
RUN dotnet restore "FomoDog/FomoDog.Tests/FomoDog.Tests.csproj"
RUN dotnet restore "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj"

COPY . .

# Optional: Run tests
ARG RUN_TESTS
RUN if [ "$RUN_TESTS" = "true" ]; then \
      dotnet test "FomoDog/FomoDog.Tests/FomoDog.Tests.csproj" --collect:"XPlat Code Coverage" --results-directory:"/app/coverage/"; \
    fi

# Build the projects
RUN dotnet build "FomoDog/FomoDog.csproj" -c Release -o /app/build
RUN dotnet build "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj" -c Release -o /app/build

# Publish the projects
FROM build AS publish
RUN dotnet publish "FomoDog/FomoDog.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "FomoDog.MessageTypes/FomoDog.MessageTypes.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/coverage ./coverage
ENTRYPOINT ["dotnet", "FomoDog.dll"]