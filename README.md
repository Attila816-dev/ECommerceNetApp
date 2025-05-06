# ECommerceNetApp  

## Overview  
ECommerceNetApp is a modular and scalable e-commerce application built using .NET 8.0. It is designed with a clean architecture approach, ensuring separation of concerns and maintainability. The solution includes multiple projects for domain, service, persistence, API, and testing layers.  

## Solution Structure  

### Projects  
- **ECommerceNetApp.Domain**  
 Contains core domain models, interfaces, and business logic.  

- **ECommerceNetApp.Service**  
 Implements application services, commands, and validators using MediatR and FluentValidation.  

- **ECommerceNetApp.Persistence**  
 Handles database interactions and data persistence using Entity Framework Core and LiteDb NoSql database.  

- **ECommerceNetApp.Api**  
 Exposes RESTful APIs with versioning, Swagger documentation, and health checks.  

- **ECommerceNetApp.Domain.UnitTest**  
 Unit tests for the domain layer using xUnit, Moq, and Shouldly.  

- **ECommerceNetApp.Service.UnitTest**  
 Unit tests for the service layer using xUnit, Moq, and Shouldly.  

- **ECommerceNetApp.Persistence.UnitTest**  
 Unit tests for the persistence layer using xUnit, Moq, and Entity Framework Core, LiteDb In-Memory provider.  

- **ECommerceNetApp.IntegrationTests**  
 Integration tests for the API layer using Microsoft.AspNetCore.Mvc.Testing and Entity Framework Core In-Memory provider.  

## Key Features  

- **Clean Architecture**: Separation of concerns across domain, service, persistence, and API layers.  
- **API Versioning**: Supports multiple API versions using `Asp.Versioning`.  
- **Swagger Integration**: Auto-generated API documentation with Swagger UI.  
- **Health Checks**: Built-in health checks for database connectivity.  
- **Logging**: Integrated with Serilog for structured logging.  
- **Validation**: Request validation using FluentValidation.  
- **Testing**: Comprehensive unit and integration tests using xUnit, Moq, and Shouldly.  

## Prerequisites  

- .NET 8.0 SDK  
- Visual Studio 2022 or later  
- SQL Server (for production database)  

## Getting Started  

### Build and Run  

1. Clone the repository:
2. Build the solution:
3. Run the API project:
4. Access the Swagger UI

### Running Tests  

Run all unit and integration tests:

## Configuration  

- **Database Configuration**: Update connection strings in `appsettings.json` for production and development environments.  
- **Logging**: Configure Serilog settings in `appsettings.json`.  

## Contributing  

Contributions are welcome! Please follow the standard GitHub workflow:  
1. Fork the repository.  
2. Create a feature branch.  
3. Commit your changes.  
4. Submit a pull request.  

## License  

This project is licensed under the MIT License.  

## Acknowledgments  

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)  
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)  
- [MediatR](https://github.com/jbogard/MediatR)  
- [FluentValidation](https://fluentvalidation.net/)  
- [Serilog](https://serilog.net/)
