FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["gestionMissionBack.Api/gestionMissionBack.Api.csproj", "gestionMissionBack.Api/"]
COPY ["gestionMissionBack.Application/gestionMissionBack.Application.csproj", "gestionMissionBack.Application/"]
COPY ["gestionMissionBack.Infrastructure/gestionMissionBack.Infrastructure.csproj", "gestionMissionBack.Infrastructure/"]
COPY ["gestionMissionBack.Domain/gestionMissionBack.Domain.csproj", "gestionMissionBack.Domain/"]
RUN dotnet restore "gestionMissionBack.Api/gestionMissionBack.Api.csproj"
COPY . .
WORKDIR "/src/gestionMissionBack.Api"
RUN dotnet build "gestionMissionBack.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "gestionMissionBack.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gestionMissionBack.Api.dll"] 