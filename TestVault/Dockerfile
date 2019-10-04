FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TestVault/TestVault.csproj", "TestVault/"]
RUN dotnet restore "TestVault/TestVault.csproj"
COPY . .
WORKDIR "/src/TestVault"
RUN dotnet build "TestVault.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TestVault.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TestVault.dll"]