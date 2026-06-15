# NK Silk — multi-stage container build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NKSilk.sln ./
COPY src/ ./src/
RUN dotnet restore NKSilk.sln
RUN dotnet publish src/NKSilk.Web/NKSilk.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# Liveness/readiness: GET /health
HEALTHCHECK --interval=30s --timeout=5s --retries=5 \
  CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "NKSilk.Web.dll"]
