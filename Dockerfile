FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/CreatioAccounts.Api.csproj backend/
RUN dotnet restore backend/CreatioAccounts.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/CreatioAccounts.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet CreatioAccounts.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]