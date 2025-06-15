# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files in dependency order for better layer caching
COPY ["Directory.Build.props", "./"]
COPY ["ECommerceNetApp.Domain/ECommerceNetApp.Domain.csproj", "ECommerceNetApp.Domain/"]
COPY ["ECommerceNetApp.Persistence/ECommerceNetApp.Persistence.csproj", "ECommerceNetApp.Persistence/"]
COPY ["ECommerceNetApp.Service/ECommerceNetApp.Service.csproj", "ECommerceNetApp.Service/"]
COPY ["ECommerceNetApp.Api/ECommerceNetApp.Api.csproj", "ECommerceNetApp.Api/"]

# Restore dependencies
RUN dotnet restore "ECommerceNetApp.Api/ECommerceNetApp.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/ECommerceNetApp.Api"
RUN dotnet build "ECommerceNetApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Development stage for debugging
FROM build AS development
EXPOSE 8080
EXPOSE 8081

# Create directories and set permissions
RUN mkdir -p /app/data/litedb /logs /https

# Build the project for development
WORKDIR /src
RUN dotnet build "ECommerceNetApp.Api/ECommerceNetApp.Api.csproj" -c Debug -o /app/dev

# Set working directory to the output directory
WORKDIR /app/dev

# Copy appsettings files
COPY ECommerceNetApp.Api/appsettings*.json ./

# Generate development certificate if not exists
RUN if [ ! -f /https/aspnetapp.pfx ]; then \
    dotnet dev-certs https -ep /https/aspnetapp.pfx -p password; \
    fi

# Use the built DLL instead of dotnet run
CMD ["dotnet", "ECommerceNetApp.Api.dll"]

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ECommerceNetApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directories and user
RUN mkdir -p /app/data/litedb /logs && \
    groupadd -r appuser --gid=1000 && \
    useradd -r -g appuser --uid=1000 --home-dir=/app --shell=/bin/bash appuser && \
    chown -R appuser:appuser /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "ECommerceNetApp.Api.dll"]