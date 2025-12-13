# Use ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["NextErp.API/NextErp.API.csproj", "NextErp.API/"]
COPY ["NextErp.Application/NextErp.Application.csproj", "NextErp.Application/"]
COPY ["NextErp.Domain/NextErp.Domain.csproj", "NextErp.Domain/"]
COPY ["NextErp.Infrastructure/NextErp.Infrastructure.csproj", "NextErp.Infrastructure/"]
RUN dotnet restore "./NextErp.API/NextErp.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/NextErp.API"
RUN dotnet build "./NextErp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NextErp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NextErp.API.dll"]
