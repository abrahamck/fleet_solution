FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["appfleet-nexus-api/appfleet-nexus-api.csproj", "appfleet-nexus-api/"]
COPY ["appfleet-nexus-data/appfleet-nexus-data.csproj", "appfleet-nexus-data/"]
RUN dotnet restore "appfleet-nexus-api/appfleet-nexus-api.csproj"
COPY . .
WORKDIR "/src/appfleet-nexus-api"
RUN dotnet build "appfleet-nexus-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "appfleet-nexus-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "appfleet-nexus-api.dll"]
