FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["services/registry/registry.csproj", "services/registry/"]
COPY ["packages/domain/domain.csproj", "packages/domain/"]
RUN dotnet restore "services/registry/registry.csproj"
# Copy only required source to keep context minimal and exclude tests
COPY packages/domain/ packages/domain/
COPY services/registry/ services/registry/
RUN rm -rf services/registry/registry.Tests || true
WORKDIR "/src/services/registry"
RUN dotnet build "registry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "registry.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "registry.dll"]
