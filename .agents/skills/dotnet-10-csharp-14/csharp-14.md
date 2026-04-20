# C# 14 Language Features

## MANDATORY: Use C# 14 Syntax for New Code

**When writing new code, ALWAYS use C# 14 syntax. Do not fall back to older patterns.**

---

## Extension Members (ALWAYS Use Extension Blocks)

**MANDATORY: Use C# 14 extension block syntax for all new extension members.**

C# 14 supports extension properties, operators, and static members via extension blocks:

❌ **WRONG** (Traditional - Do Not Use for New Code):
```csharp
public static class EnumerableExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> source)
        => !source.Any();

    public static T SafeElementAt<T>(this IEnumerable<T> source, int index)
        => source.Skip(index).FirstOrDefault();
}
```

✅ **CORRECT** (C# 14 Extension Block):

```csharp
public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        // Extension property
        public bool IsEmpty => !source.Any();

        // Extension indexer
        public T this[int index] => source.Skip(index).First();

        // Extension method
        public IEnumerable<T> Filter(Func<T, bool> predicate)
            => source.Where(predicate);
    }

    // Static extension members (no parameter name)
    extension<T>(IEnumerable<T>)
    {
        public static IEnumerable<T> Empty => Enumerable.Empty<T>();
    }
}

// Usage
var items = new[] { 1, 2, 3 };
if (!items.IsEmpty) Console.WriteLine(items[0]);
```

**Why extension blocks are better:**
- Enable extension **properties** (not just methods)
- Enable extension **indexers**
- Enable **static** extension members
- Cleaner, more expressive syntax
- Requires `<LangVersion>14</LangVersion>` in .csproj

---

## The `field` Keyword (ALWAYS Use for Property Validation)

**MANDATORY: Use the `field` keyword instead of manual backing fields when adding validation to properties.**

❌ **WRONG** (Manual Backing Field - Verbose):
```csharp
public class Person
{
    private string _name = "";
    private int _age;

    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    public int Age
    {
        get => _age;
        set => _age = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
}
```

✅ **CORRECT** (C# 14 `field` Keyword):

```csharp
public class Person
{
    public string Name
    {
        get => field;
        set => field = value?.Trim() ?? throw new ArgumentNullException();
    }

    // With validation and transformation
    public int Age
    {
        get => field;
        set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException();
    }
}
```

**Why `field` is better:**
- Less boilerplate (no manual backing field declaration)
- Compiler manages the backing field
- Cleaner, more maintainable code
- Works with `init` and `required` modifiers

---

## Null-Conditional Assignment (ALWAYS Use)

**MANDATORY: Replace null checks followed by assignment with `?.=` syntax.**

❌ **WRONG** (Verbose null check):
```csharp
if (customer != null)
{
    customer.LastVisit = DateTime.UtcNow;
}

if (list != null && list.Count > 0)
{
    list[0] = newValue;
}
```

✅ **CORRECT** (C# 14 Null-Conditional Assignment):
```csharp
// Direct assignment
customer?.LastVisit = DateTime.UtcNow;  // NOTE: Always use UtcNow, not Now

// Works with indexers
list?[0] = newValue;

// Compound assignment
customer?.Points += 100;
customer?.Orders.Add(newOrder);
```

**IMPORTANT:** Always use `DateTime.UtcNow`, never `DateTime.Now`. See anti-patterns.md.

## Other C# 14 Features

| Feature | Example | Use Case |
|---------|---------|----------|
| `nameof` unbound generics | `nameof(List<>)` → `"List"` | Logging, reflection |
| Lambda parameter modifiers | `(ref int x) => x++` | High-perf lambdas |
| Partial constructors/events | Split across files | Code generation |
| First-class Span support | Implicit Span↔T[] | Memory-efficient APIs |

### nameof with Unbound Generics

```csharp
// Before C# 14: nameof(List<int>) → "List"
// C# 14: nameof(List<>) → "List"

public class Repository<T>
{
    private readonly string _typeName = nameof(T); // Works in generic context
}
```

### Lambda Parameter Modifiers

```csharp
// No need to specify types when using modifiers
Span<int> span = stackalloc int[10];
span.Sort((ref int a, ref int b) => a.CompareTo(b));

// in, out, scoped also work
ProcessItems((in ReadOnlySpan<byte> data) => data.Length);
```

### Partial Constructors

```csharp
// File1.cs
public partial class Widget
{
    public partial Widget(string name);
}

// File2.cs (generated)
public partial class Widget
{
    public partial Widget(string name)
    {
        Name = name;
        Initialize();
    }
}
```

## Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```
