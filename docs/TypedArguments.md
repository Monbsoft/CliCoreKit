# CliCoreKit - Typed Command-Line Arguments

## Type-Safe API with Generics

CliCoreKit now supports **strongly-typed arguments and options** using C# generics, making your CLI applications more robust and easier to maintain.

### Defining Commands with Typed Options

```csharp
builder.ConfigureCli(cli =>
{
    cli.AddCommand<GreetCommand>("greet", "Greets a person")
       .AddArgument<string>("name", "The name to greet", defaultValue: "World")
       .AddOption<string>("greeting", 'g', "Custom greeting", defaultValue: "Hello")
       .AddOption<bool>("formal", 'f', "Use formal greeting")
       .AddOption<int>("repeat", 'r', "Repeat count", defaultValue: 1);
});
```

### Accessing Typed Values in Commands

```csharp
public class GreetCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        // Type-safe access with IntelliSense support
        var name = context.Arguments.GetArgument<string>(0, "World");
        var greeting = context.Arguments.GetOption<string>("greeting", "Hello");
        var formal = context.Arguments.GetOption<bool>("formal");
        var repeat = context.Arguments.GetOption<int>("repeat", 1);

        var message = formal ? $"Good day, {name}!" : $"{greeting}, {name}!";
        
        for (int i = 0; i < repeat; i++)
        {
            Console.WriteLine(message);
        }

        return Task.FromResult(0);
    }
}
```

## Supported Types

CliCoreKit supports automatic conversion for:

- **Primitive types**: `string`, `int`, `long`, `double`, `float`, `bool`
- **Nullable types**: `int?`, `double?`, etc.
- **Enums**: Automatic parsing with case-insensitive matching
- **Custom types**: Implementing `IConvertible`

### Boolean Options

Boolean options automatically set `HasValue = false`, making them work as flags:

```csharp
cli.AddCommand<BuildCommand>("build")
   .AddOption<bool>("watch", 'w', "Watch for changes")      // Flag option
   .AddOption<string>("output", 'o', "Output directory");   // Value option
```

Usage:
```bash
app build -w -o ./dist
app build --watch --output ./dist
```

## Type-Safe API Methods

### Getting Options

```csharp
// Get with default value
var port = context.Arguments.GetOption<int>("port", 8080);
var name = context.Arguments.GetOption<string>("name", "default");
var verbose = context.Arguments.GetOption<bool>("verbose"); // defaults to false

// Try get with error handling
if (context.Arguments.TryGetValue<int>("port", out var port))
{
    Console.WriteLine($"Using port: {port}");
}
else
{
    Console.Error.WriteLine("Invalid port number");
}
```

### Getting Arguments (Positional)

```csharp
// Get typed positional argument
var count = context.Arguments.GetArgument<int>(0);
var name = context.Arguments.GetArgument<string>(1, "default");

// With null safety
var value = context.Arguments.GetArgument<double?>(0);
if (value.HasValue)
{
    Console.WriteLine($"Value: {value.Value}");
}
```

### Multiple Values

```csharp
// Define option that accepts multiple values
cli.AddCommand<ProcessCommand>("process")
   .AddOption<string>("file", 'f', "Files to process");

// Get all values
var files = context.Arguments.GetOptionValues<string>("file");
foreach (var file in files)
{
    Console.WriteLine($"Processing: {file}");
}
```

Usage:
```bash
app process -f file1.txt -f file2.txt -f file3.txt
```

## Complete Examples

### Temperature Converter

```csharp
cli.AddCommand<ConvertCommand>("convert", "Convert temperature")
   .AddArgument<double>("value", "Temperature value", required: true)
   .AddOption<string>("from", 'f', "Source unit (C/F/K)", required: true)
   .AddOption<string>("to", 't', "Target unit (C/F/K)", required: true);

public class ConvertCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var value = context.Arguments.GetArgument<double>(0);
        var from = context.Arguments.GetOption<string>("from")!.ToUpper();
        var to = context.Arguments.GetOption<string>("to")!.ToUpper();

        // Conversion logic...
        Console.WriteLine($"{value}°{from} = {result:F2}°{to}");
        return Task.FromResult(0);
    }
}
```

Usage:
```bash
app convert 100 -f C -t F
# Output: 100°C = 212.00°F

app convert 32 --from F --to C
# Output: 32°F = 0.00°C
```

### Calculator with Multiple Operations

```csharp
cli.AddCommand<CalcCommand>("calc", "Perform calculations")
   .AddArgument<double>("a", "First number", required: true)
   .AddArgument<double>("b", "Second number", required: true)
   .AddOption<string>("operation", 'o', "Operation (+,-,*,/)", defaultValue: "+");

public class CalcCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var a = context.Arguments.GetArgument<double>(0);
        var b = context.Arguments.GetArgument<double>(1);
        var op = context.Arguments.GetOption<string>("operation", "+");

        var result = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b != 0 ? a / b : double.NaN,
            _ => throw new ArgumentException("Invalid operation")
        };

        Console.WriteLine($"{a} {op} {b} = {result}");
        return Task.FromResult(0);
    }
}
```

## Automatic Help Generation

The help system automatically shows type information:

```bash
$ app convert --help
Usage: convert <value> [options]

Convert temperature

Arguments:
  value                     Temperature value [double] (required)

Options:
  -f, --from <string>       Source unit (C/F/K) (required)
  -t, --to <string>         Target unit (C/F/K) (required)
  -h, --help                Show this help message
```

## Migration from Non-Generic API

**Before:**
```csharp
var name = context.Arguments.GetOptionValue("name") ?? "default";
if (int.TryParse(context.Arguments.GetOptionValue("port"), out var port))
{
    // use port
}
```

**After:**
```csharp
var name = context.Arguments.GetOption<string>("name", "default");
var port = context.Arguments.GetOption<int>("port", 8080);
```

## Benefits

✅ **Type Safety**: Catch type errors at compile time  
✅ **IntelliSense**: Full IDE support with auto-completion  
✅ **Less Boilerplate**: No manual parsing or conversion  
✅ **Better Documentation**: Types shown in help  
✅ **Default Values**: Type-safe defaults  
✅ **Validation**: Automatic type validation  

## See Also

- [Full Sample Application](../samples/CliCoreKit.Sample/Program.cs)
