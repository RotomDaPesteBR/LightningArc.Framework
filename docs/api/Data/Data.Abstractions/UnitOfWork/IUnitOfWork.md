# IUnitOfWork

**Namespace:** `LightningArc.Data.Abstractions.UnitOfWork`  
**Type:** `interface`

Defines the contract for the Unit of Work pattern, coordinating business transactions and data persistence operations.

---

## Methods

### Begin() / BeginAsync()
*   **Description:** Starts a new business transaction.

### Commit() / CommitAsync()
*   **Description:** Commits the changes made during the transaction.

### Rollback() / RollbackAsync()
*   **Description:** Rolls back the changes made during the transaction.

---

## Usage Example

```csharp
using (var uow = _uowFactory.Create())
{
    await uow.BeginAsync();
    try 
    {
        await _userRepo.AddAsync(user);
        await uow.CommitAsync();
    }
    catch 
    {
        await uow.RollbackAsync();
        throw;
    }
}
```
