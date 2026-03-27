# IConnectionFactory.cs Implementation Details

**File Path:** `Data/Data.Abstractions/Factories/IConnectionFactory.cs`
**Namespace:** `LightningArc.Data.Abstractions.Factories`

## Overview
This interface defines the contract for abstracting the creation of database connections (`DbConnection`). It allows the application to be agnostic of the specific database provider (SQL Server, PostgreSQL, etc.) and connection logic (connection strings, pooling).

## Code Analysis

### `GetConnection`
```csharp
DbConnection GetConnection();
```
*   **Purpose**: Returns an **open** database connection.
*   **Lifecycle**: The caller is typically responsible for disposing the connection, although some implementations might handle pooling internally.
*   **Abstraction**: By returning `DbConnection` (System.Data.Common), it supports any ADO.NET provider.
