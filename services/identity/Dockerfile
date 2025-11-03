FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["services/identity/identity.csproj", "services/identity/"]
RUN dotnet restore "services/identity/identity.csproj"
COPY . .
WORKDIR "/src/services/identity"
RUN dotnet build "identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "identity.dll"]

