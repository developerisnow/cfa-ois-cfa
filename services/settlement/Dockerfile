FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["services/settlement/settlement.csproj", "services/settlement/"]
COPY ["packages/domain/domain.csproj", "packages/domain/"]
RUN dotnet restore "services/settlement/settlement.csproj"
COPY packages/domain/ packages/domain/
COPY services/settlement/ services/settlement/
WORKDIR "/src/services/settlement"
RUN dotnet build "settlement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "settlement.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "settlement.dll"]
