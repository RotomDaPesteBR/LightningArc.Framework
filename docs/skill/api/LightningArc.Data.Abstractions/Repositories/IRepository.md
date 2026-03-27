# IRepository

**Namespace:** `LightningArc.Data.Abstractions.Repositories`  
**Type:** `interface`

A base marker interface and generic definition for all repositories in the system.

---

## IRepository (Marker)

*   **Description:** A base marker interface for all repositories.

---

## IRepository<TEntity>

*   **Generic Parameters:**
    *   `TEntity` (`class`): The type of the domain entity.
*   **Description:** Defines a generic repository for a specific entity type.

---

## Usage Example

```csharp
public interface IUserRepository : IRepository<User>
{
    // Custom repository methods...
    Task<User?> GetByEmailAsync(string email);
}
```
