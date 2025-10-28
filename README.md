# Product Service Docker Setup

This document provides essential commands for managing the Docker setup of the Product Service application. Below are the commands listing and an overview of the project's structure.

## Project Structure

- **TSMWS.ProductService.Api**: The main entry point for the Product Service application, hosting the API interfaces.
- **TSMWS.ProductService.Business**: Contains business logic and services used by the application.
- **TSMWS.ProductService.Data**: Manages data access and interactions with the underlying database.
- **TSMWS.ProductService.Configurations**: Holds configuration settings and service setup extensions.
- **TSMWS.ProductService.Shared**: Contains shared models and utilities used across different projects.

### Tests Folder

- **TSMWS.ProductService.Data.IntegrationTests**: Integration Tests for the Data layer, ensuring correct messaging behavior.

## Docker Commands

### 1. Docker Compose for docker-compose.prod.yml (All services).

```bash
docker-compose -f docker-compose.prod.yml -p tswms up --pull always --detach
```

```bash
docker-compose -f docker-compose.test.yml -p tswms up --pull always --detach
```
# Run Dapr Sidecar
```bash
dapr run --app-id productservice --app-port 3300 -- dotnet watch run --project ./TSWMS.ProductService.Api/TSWMS.ProductService.Api.csproj
```