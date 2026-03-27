# IMapper.cs Implementation Details

**File Path:** `Data/Data.Abstractions/Mappers/IMapper.cs`
**Namespace:** `LightningArc.Data.Abstractions.Mappers`

## Overview
A lightweight abstraction for Object-to-Object mapping libraries.

## Code Analysis

### Methods
*   `Map<TDestination>(object source)`: Creates a new instance of destination mapped from source.
*   `Map<TSource, TDestination>(source, destination)`: Updates an existing destination instance.

### Purpose
To decouple the Data Access Layer from specific libraries like Mapster or AutoMapper. An adapter implementation should be provided in the concrete application or a specific integration library (e.g., `LightningArc.Data.Mapster`).
