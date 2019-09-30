FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TestDb/TestDb.csproj", "TestDb/"]
RUN dotnet restore "TestDb/TestDb.csproj"
COPY . .
WORKDIR "/src/TestDb"
RUN dotnet build "TestDb.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TestDb.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TestDb.dll"]