# Agile360 API – multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Agile360.sln ./
COPY global.json ./
COPY src/Agile360.API/Agile360.API.csproj src/Agile360.API/
COPY src/Agile360.Application/Agile360.Application.csproj src/Agile360.Application/
COPY src/Agile360.Domain/Agile360.Domain.csproj src/Agile360.Domain/
COPY src/Agile360.Infrastructure/Agile360.Infrastructure.csproj src/Agile360.Infrastructure/
COPY src/Agile360.Shared/Agile360.Shared.csproj src/Agile360.Shared/

RUN dotnet restore Agile360.sln

COPY src ./src
RUN dotnet build src/Agile360.API/Agile360.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish src/Agile360.API/Agile360.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .
# Health: GET /health ou GET /api/health (ver README)

ENTRYPOINT ["dotnet", "Agile360.API.dll"]
