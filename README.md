# DAO Generator Developer Guide

## Overview
The DAO Generator is a utility that automatically generates Data Access Object (DAO) classes and their corresponding interfaces for Entity Framework Core.  
It speeds up the development process by producing standardized CRUD (Create, Read, Update, Delete) methods, as well as extra read methods based on **foreign keys** and **indexed columns**.

The generated code adheres to good practices like safe deletes, optional concurrency checks, and pagination.

---

## Features
- Generates both DAO **classes** and **interfaces**
- CRUD methods automatically included
- Optional concurrency checks for `Update()`
- `GetAll()` method with pagination support
- Extra read methods generated for:
  - Foreign key columns
  - Indexed columns
- Fully formatted, ready-to-use C# code
- Consistent method naming conventions

---

## Example Generated Interface
Below is an example of a manually written DAO interface that the generator can produce automatically:

```csharp
public interface IWishListItemDAO
{
    Task<WishListItem?> GetById(int id);
    Task<List<WishListItem>> GetByUserId(int id);
    Task<int> Add(WishListItem newEntity);
    Task<int> Update(WishListItem updatedEntity);
    Task<int> Delete(int id);
}
```

The generator would detect the foreign keys (`UserId`, `ItemId`) and create matching `GetBy...` methods.

---

## Generated CRUD Method Details

### `GetById(int id)`
Fetches an entity by its primary key. Throws `KeyNotFoundException` if not found.

### `Add(T newEntity)`
Adds a new entity to the database and returns the new ID.

### `Update(T updatedEntity)`
Updates an entity.  
- Optional concurrency checks (commented out by default).  
- Returns:
  - `1` if updated successfully
  - `-1` if no changes saved
  - `-2` if concurrency conflict detected

### `Delete(int id)`
Safely deletes an entity after fetching it by ID.  
Prevents accidental deletion of detached entities.

### `GetAll(int? page, int? pageSize)`
Returns a `PagedResult<T>` containing items and pagination metadata.

---

## PagedResult<T> Structure

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; }
    public int TotalPages { get; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PagedResult(List<T> items, int totalCount, int? page, int? pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page ?? 1;
        PageSize = pageSize ?? totalCount;

        TotalPages = pageSize.HasValue && pageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize)
            : 1;
    }
}
```

---

## Installation & Usage

### 1. Run the Generator
Run the generator program and provide:
- **Database connection string — used to inspect the database schema.**

### 2. Place the files in Your Project
Copy the generated `.cs` files into your project.

### 3. Register with Dependency Injection
```csharp
services.AddScoped<IWishListItemDAO, WishListItemDAO>();
```

---

## Customization
- **Concurrency**: Uncomment concurrency check lines in `Update()` to enforce optimistic locking.
- **Generated Method Naming**: Adjust `CamelCase()` logic if your database naming conventions differ.
- **Pagination**: You can modify the `GetAll()` implementation to return results sorted by a specific column.

---

## Summary
This DAO Generator ensures all your EF Core DAO classes follow consistent structure and patterns, improving maintainability and development speed.
## 7. Extending the Generator

### Adding New Method Templates
- Locate the relevant generator (`DaoClassGenerator` or `DaoInterfaceGenerator`).
- Add a `StringBuilder` append block in the desired position.
- Follow the existing pattern for formatting and naming.

### Keeping DAO & Interface in Sync
- The FK/index method logic is shared — changes to one will be reflected in the other.

## 8. Known Limitations / TODOs
- No support for composite primary keys.
- No eager-loading customization.
- No soft delete support.
- Assumes a simple `Id` primary key convention.
