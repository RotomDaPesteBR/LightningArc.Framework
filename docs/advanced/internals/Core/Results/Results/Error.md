# Error.cs Implementation Details

**File Path:** `Core/Results/Results/Error.cs`
**Namespace:** `LightningArc.Results`

## Overview
The `Error` class represents a standardized failure in the domain. It encapsulates an error code, a message (which can be localized), and optional detailed information.

## Code Analysis

### Key Properties
*   `Code`: A composite integer representing the full error code. It is calculated as `CodePrefix * 1000 + CodeSuffix`.
    *   *Example:* If Prefix is 10 and Suffix is 01, Code is 10001.
*   `CodePrefix`: Categorizes the error (e.g., "Validation Error", "Database Error").
*   `CodeSuffix`: Identifies the specific error within the category.
*   `Message`: The descriptive text of the error. It uses an `IMessageProvider` to support localization.
*   `Details`: A read-only list of `ErrorDetail` objects, useful for scenarios like form validation where multiple fields might be invalid.

### Constructors
*   **Protected**: The class is designed to be inherited by specific error types (e.g., `ValidationError`, `NotFoundError`).
*   **Validation**: Ensures that prefix and suffix are positive integers.
*   **Message Providers**: Supports both literal strings (`LiteralMessageProvider`) and localized messages via `IMessageProvider`.

### Design Pattern
This class acts as the base for a **Smart Enum** or a comprehensive Error Hierarchy. By defining specific error classes that inherit from `Error`, the application can maintain a catalog of all possible failures.
