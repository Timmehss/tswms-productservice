# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the solution file and all the project files
COPY TSWMS.ProductService.sln . 
COPY TSWMS.ProductService.Api/ TSWMS.ProductService.Api/
COPY TSWMS.ProductService.Business/ TSWMS.ProductService.Business/
COPY TSWMS.ProductService.Configurations/ TSWMS.ProductService.Configurations/
COPY TSWMS.ProductService.Data/ TSWMS.ProductService.Data/
COPY TSWMS.ProductService.Shared/ TSWMS.ProductService.Shared/

# Restore dependencies
RUN dotnet restore "TSWMS.ProductService.sln"

# Build the application
WORKDIR "/src/TSWMS.ProductService.Api"
RUN dotnet build "TSWMS.ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the API project
FROM build AS publish
RUN dotnet publish "TSWMS.ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

EXPOSE 8080
EXPOSE 8081

# Set the HMAC secret during build
ENV HMAC_SECRET_KEY qWX4IlPFoIKLeSoiiT1JBAl7KvzIRwVm

# Final stage - run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TSWMS.ProductService.Api.dll"]
