FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["services/issuance/issuance.csproj", "services/issuance/"]
COPY ["packages/domain/domain.csproj", "packages/domain/"]
RUN dotnet restore "services/issuance/issuance.csproj"
COPY packages/domain/ packages/domain/
COPY services/issuance/ services/issuance/
RUN rm -rf services/issuance/issuance.Tests || true
WORKDIR "/src/services/issuance"
RUN dotnet build "issuance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "issuance.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "issuance.dll"]
