FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Stats.Client/Stats.Client.csproj", "Stats.Client/"]
RUN dotnet restore "Stats.Client/Stats.Client.csproj"
COPY . .
WORKDIR "/src/Stats.Client"
RUN dotnet build "Stats.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stats.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stats.Client.dll"]
