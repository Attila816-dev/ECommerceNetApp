# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ECommerceNetApp.Api/ECommerceNetApp.Api.csproj", "ECommerceNetApp.Api/"]
COPY ["ECommerceNetApp.Domain/ECommerceNetApp.Domain.csproj", "ECommerceNetApp.Domain/"]
COPY ["ECommerceNetApp.Persistence/ECommerceNetApp.Persistence.csproj", "ECommerceNetApp.Persistence/"]
COPY ["ECommerceNetApp.Service/ECommerceNetApp.Service.csproj", "ECommerceNetApp.Service/"]
RUN dotnet restore "./ECommerceNetApp.Api/ECommerceNetApp.Api.csproj"
COPY . .
WORKDIR "/src/ECommerceNetApp.Api"
RUN dotnet build "./ECommerceNetApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ECommerceNetApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerceNetApp.Api.dll"]