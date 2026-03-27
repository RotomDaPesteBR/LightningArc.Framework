# ParameterDefinition.cs Implementation Details

**File Path:** `Meta/Metalama/Extensions/Definitions/ParameterDefinition.cs`
**Namespace:** `LightningArc.Metalama`

## Overview
A simple POCO (Plain Old CLR Object) class marked with `[CompileTime]`. It represents the metadata of a method parameter (Name, Type, RefKind, DefaultValue) to be used during code generation.

## Code Analysis

### Purpose
When generating methods via Metalama aspects, it's often necessary to define lists of parameters dynamically. This class provides a structured way to hold that definition before passing it to the `IMethodBuilder`.

### Constructors
It offers constructors accepting both `Type` (System.Reflection) and `IType` (Metalama), normalizing them into Metalama's `IType` system using `TypeFactory`.
