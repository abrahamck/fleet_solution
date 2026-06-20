FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["appfleet-nexus-ui/appfleet-nexus-ui.csproj", "appfleet-nexus-ui/"]
RUN dotnet restore "appfleet-nexus-ui/appfleet-nexus-ui.csproj"
COPY . .
WORKDIR "/src/appfleet-nexus-ui"
RUN dotnet build "appfleet-nexus-ui.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "appfleet-nexus-ui.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
