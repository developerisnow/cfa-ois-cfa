FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["services/compliance/compliance.csproj", "services/compliance/"]
COPY ["packages/domain/domain.csproj", "packages/domain/"]
RUN dotnet restore "services/compliance/compliance.csproj"
COPY . .
WORKDIR "/src/services/compliance"
RUN dotnet build "compliance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "compliance.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "compliance.dll"]

