# Poirot OSINT API - Docker Build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SherlockOsint.Api/SherlockOsint.Api.csproj", "src/SherlockOsint.Api/"]
COPY ["src/SherlockOsint.Shared/SherlockOsint.Shared.csproj", "src/SherlockOsint.Shared/"]
RUN dotnet restore "src/SherlockOsint.Api/SherlockOsint.Api.csproj"
COPY . .
WORKDIR "/src/src/SherlockOsint.Api"
RUN dotnet build "SherlockOsint.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SherlockOsint.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SherlockOsint.Api.dll"]
