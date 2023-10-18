#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FomoDog.csproj", "."]
RUN dotnet restore "./FomoDog.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "FomoDog.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR /src
COPY . .
RUN if [ "$RUN_TESTS" = "true" ]; then \
      dotnet test "./FomoDog.Tests" --logger "console;verbosity=detailed" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput='./coverage/'
    fi

FROM build AS publish
RUN dotnet publish "FomoDog.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FomoDog.dll"]