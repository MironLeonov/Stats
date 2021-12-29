FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Stats.Worker/Stats.Worker.csproj", "Stats.Worker/"]
RUN dotnet restore "Stats.Worker/Stats.Worker.csproj"
COPY . .
WORKDIR "/src/Stats.Worker"
RUN dotnet build "Stats.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stats.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stats.Worker.dll"]
