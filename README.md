# EcommerceApplicationWeb

EcommerceApplicationWeb is a **modular, clean architecture-based e-commerce backend** built on **.NET Core**, using **Entity Framework Core**, and **REST APIs**. This project demonstrates modern software engineering practices, including **Domain-Driven Design (DDD)**, **Repository & Unit of Work patterns**, **DTOs with AutoMapper**, and **metadata-based optimization** for reduced data load.

---

## Table of Contents

1. [Features](#features)  
2. [Architecture & Patterns](#architecture--patterns)  
3. [Tech Stack & Tools](#tech-stack--tools)  
4. [Database & Metadata Handling](#database--metadata-handling)  
5. [Services & Controllers](#services--controllers)  
6. [Setup & Migration](#setup--migration)  
7. [Usage](#usage)  
8. [Future Improvements](#future-improvements)  

---

## Features

### Product Module
- Create, update, delete, and retrieve products.
- Paging, sorting, and filtering (by price, category, etc.).
- Each product is linked to a category.
- Metadata JSON column to store dynamic attributes without schema changes.
- Automatic update of category product count when products are added, updated, or deleted.
- Includes parent-child relationships (for product variations) and category hierarchies.

### Category Module
- CRUD operations for categories.
- Self-referencing for parent-child relationships.
- Metadata JSON column to store additional info like product counts.

### API Features
- RESTful endpoints with **Swagger documentation**.
- Error handling using try/catch in controllers and services.
- DTO-based request/response separation to avoid overfetching.
- Pagination, sorting, and search for efficient data retrieval.

---

## Architecture & Patterns

### Clean Architecture Layers

1. **Domain Layer**
   - Entities: `Product`, `Category`.
   - Interfaces for repositories and services.
   - Domain logic encapsulated in entities and services.

2. **Application Layer**
   - Business logic for handling entities.
   - Service classes: `ProductService`, `CategoryService`.
   - Uses **Unit of Work** for transaction management.
   - DTOs for request/response using **AutoMapper**.
   - `GetSingle` and `GetBulk` base methods for querying efficiently.

3. **Infrastructure Layer**
   - EF Core `DbContext` implementation.
   - Repository implementations for each entity.
   - Metadata JSON column handled with `HasConversion` to reduce DB load.

4. **Web/API Layer**
   - Controllers for Products and Categories.
   - Try/catch for error handling and clear response messages.
   - Optional **IncludeWhen/ThenIncludeWhen** extensions for conditional eager loading.

---

### Key Patterns Used
- **Repository Pattern** – abstracts data access logic.  
- **Unit of Work Pattern** – manages transaction across multiple repositories.  
- **DTO Pattern** – separates request/response objects from domain entities.  
- **AutoMapper** – simplifies mapping between entities and DTOs.  
- **Self-referencing Entities** – categories and products can have parent-child relationships.  
- **Metadata JSON Column** – stores additional entity data dynamically to reduce joins and queries.  

---

## Tech Stack & Tools

- **Backend**: .NET 8/9 Core, C#  
- **ORM**: Entity Framework Core  
- **Database**: SQL Server (LocalDB or SQL Express)  
- **Authentication**: ASP.NET Identity  
- **Logging**: Serilog (File-based)  
- **API Documentation**: Swagger / Swashbuckle  
- **Object Mapping**: AutoMapper  
- **Frontend (optional)**: React + Next.js for consuming APIs  

---

## Database & Metadata Handling

- **Products**
  - Columns: `Id`, `Title`, `Code`, `Price`, `Stock`, `CategoryId`, `ParentId`, `Metadata`, etc.
  - `Metadata` is a JSON column that stores dynamic attributes such as `ExtraSpecs`, `Discounts`, etc.
- **Categories**
  - Columns: `Id`, `Title`, `ParentId`, `Metadata`, etc.
  - `Metadata` stores dynamic info like `ProductCount` for efficient querying.

**Benefits of Metadata JSON:**
- Avoids schema changes for minor additions.
- Reduces unnecessary joins for dynamic attributes.
- Lightweight and flexible for front-end consumption.

---

## Services & Controllers

### ProductService
- Handles CRUD for products.
- Updates category product count automatically.
- Uses `Query()` from repository for LINQ queries.
- Supports Include/ThenInclude for category and parent-child relations.

### CategoryService
- Handles CRUD for categories.
- Uses dynamic filtering and paging.
- Updates metadata like `ProductCount`.

### Controllers
- REST API with try/catch error handling.
- Include joins via EF Core `Include()` and conditional `IncludeWhen()`.

---

## Setup & Migration

1. Update `appsettings.json` with your connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EcommerceApp;Integrated Security=True;TrustServerCertificate=True;"
}
