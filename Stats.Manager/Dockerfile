FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Stats.Manager/Stats.Manager.csproj", "Stats.Manager/"]
RUN dotnet restore "Stats.Manager/Stats.Manager.csproj"
COPY . .
WORKDIR "/src/Stats.Manager"
RUN dotnet build "Stats.Manager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stats.Manager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stats.Manager.dll"]
