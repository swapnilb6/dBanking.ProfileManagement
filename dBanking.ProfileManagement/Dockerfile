# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 9090

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Use simple COPY paths (not JSON array) to avoid Windows path parsing issues
COPY dBanking.ProfileManagement/dBanking.ProfileManagement.API.csproj dBanking.ProfileManagement/
COPY dBanking.ProfileManagement.Core/dBanking.ProfileManagement.Core.csproj dBanking.ProfileManagement.Core/
COPY dBanking.ProfileManagement.Infrastructure/dBanking.ProfileManagement.Infrastructure.csproj dBanking.ProfileManagement.Infrastructure/

RUN dotnet restore "dBanking.ProfileManagement/dBanking.ProfileManagement.API.csproj"
COPY . .
WORKDIR "/src/dBanking.ProfileManagement"
RUN dotnet build "dBanking.ProfileManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "dBanking.ProfileManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dBanking.ProfileManagement.API.dll"]