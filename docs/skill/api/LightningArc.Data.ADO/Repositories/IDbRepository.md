# IDbRepository<TRepository>

**Namespace:** `LightningArc.Data.ADO.Repositories`  
**Type:** `interface`

Defines the contract for a self-creating ADO.NET/Dapper repository. This interface uses static abstract members (available in .NET 7+) to allow factories to create repository instances.

---

## Methods (Static Abstract - .NET 7+)

### Create(DbConnection connection, DbTransaction transaction, IMapper? mapper = null, ILogger<TRepository>? logger = null)
*   **Description:** Creates a new instance of the repository using an existing connection and transaction.

### Create(IConnectionFactory connectionFactory, IMapper? mapper = null, ILogger<TRepository>? logger = null)
*   **Description:** Creates a new instance of the repository using a connection factory.

---

## Usage Example

```csharp
public class UserRepository : IDbRepository<UserRepository>
{
    // Implementation of static abstract methods...
}
```
