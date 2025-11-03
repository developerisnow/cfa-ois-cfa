FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["services/integrations/bank-nominal/bank-nominal.csproj", "services/integrations/bank-nominal/"]
RUN dotnet restore "services/integrations/bank-nominal/bank-nominal.csproj"
COPY . .
WORKDIR "/src/services/integrations/bank-nominal"
RUN dotnet build "bank-nominal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bank-nominal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "bank-nominal.dll"]

