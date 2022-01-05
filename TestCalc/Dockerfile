FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TestCalc/TestCalc.csproj", "TestCalc/"]
RUN dotnet restore "TestCalc/TestCalc.csproj"
COPY . .
WORKDIR "/src/TestCalc"
RUN dotnet build "TestCalc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestCalc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestCalc.dll"]
