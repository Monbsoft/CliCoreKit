# Automatic Help System

CliCoreKit includes a powerful automatic help generation system that eliminates the need to manually handle `--help` in your commands.

## Overview

By default, **all commands** automatically support `--help` and `-h` options. The help is generated from the command's metadata (arguments, options, description).

### Key Features

✅ **Automatic**: No code needed to handle `--help`  
✅ **Type-aware**: Shows types in help (`<int>`, `<string>`, etc.)  
✅ **Default values**: Displays default values for options  
✅ **Required indicators**: Shows which arguments/options are required  
✅ **Hierarchical**: Works for parent and child commands  
✅ **Opt-out**: Can disable auto-help if needed  

## Basic Usage

### Before (Manual Help)

```csharp
public class GreetCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken ct)
    {
        // ❌ Manual help handling - NO LONGER NEEDED!
        if (context.Arguments.HasOption("help"))
        {
            Console.WriteLine("Usage: greet [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --name    Your name");
            return Task.FromResult(0);
        }

        var name = context.Arguments.GetOption<string>("name", "World");
        Console.WriteLine($"Hello, {name}!");
        return Task.FromResult(0);
    }
}
```

### After (Automatic Help)

```csharp
// Configuration
cli.AddCommand<GreetCommand>("greet", "Greets a person")
   .AddOption<string>("name", 'n', "Your name", defaultValue: "World");

// Command - Clean and focused!
public class GreetCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken ct)
    {
        // ✅ No help handling needed - just business logic!
        var name = context.Arguments.GetOption<string>("name", "World");
        Console.WriteLine($"Hello, {name}!");
        return Task.FromResult(0);
    }
}
```

## Automatic Help Output

When a user runs `greet --help`:

```
Usage: greet [options]

Greets a person

Options:
  -n, --name <string>       Your name (default: World)
  -h, --help                Show this help message
```

## With Arguments

```csharp
cli.AddCommand<DeployCommand>("deploy", "Deploy application")
   .AddArgument<string>("environment", "Target environment", required: true)
   .AddArgument<string>("version", "Version to deploy", required: false)
   .AddOption<bool>("force", 'f', "Force deployment");
```

Help output for `deploy --help`:

```
Usage: deploy <environment> [version] [options]

Deploy application

Arguments:
  environment               Target environment [string] (required)
  version                   Version to deploy [string]

Options:
  -f, --force               Force deployment
  -h, --help                Show this help message
```

## With Typed Options

```csharp
cli.AddCommand<ServeCommand>("serve", "Start web server")
   .AddOption<int>("port", 'p', "Port number", defaultValue: 8080)
   .AddOption<string>("host", 'h', "Host address", defaultValue: "localhost")
   .AddOption<bool>("watch", 'w', "Watch for changes");
```

Help output for `serve --help`:

```
Usage: serve [options]

Start web server

Options:
  -p, --port <int>          Port number (default: 8080)
  -h, --host <string>       Host address (default: localhost)
  -w, --watch               Watch for changes
  -h, --help                Show this help message
```

## Hierarchical Commands

Automatic help works for parent commands too:

```csharp
cli.AddCommand<GitCommand>("git", "Git operations")
   .AddCommand<CommitCommand>("commit", "Commit changes")
   .AddCommand<PushCommand>("push", "Push to remote");
```

Help output for `git --help`:

```
Usage: git <command> [options]

Git operations

Commands:
  commit                Commit changes
  push                  Push to remote

Options:
  -h, --help            Show this help message

Run 'git <command> --help' for more information on a command.
```

## Disabling Auto-Help

If you need custom help handling, disable auto-help:

```csharp
cli.AddCommand<CustomCommand>("custom", "Command with fancy help")
.WithoutHelp();  // ← Disable automatic help

public class CustomCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken ct)
    {
        // Now you handle --help yourself
        if (context.Arguments.HasOption("help"))
        {
            Console.WriteLine("╔══════════════════════╗");
            Console.WriteLine("║   FANCY CUSTOM HELP  ║");
            Console.WriteLine("╚══════════════════════╝");
            // Your custom help...
            return Task.FromResult(0);
        }

        // Command logic...
        return Task.FromResult(0);
    }
}
```

## Global Help

Global help (no command specified) is also automatic:

```bash
$ myapp --help
Usage: [command] [options]

Available commands:

  greet                Greets a person
  add                  Adds two numbers
  list                 List operations
    files              Lists files

Options:
  -h, --help          Show this help message

Run '[command] --help' for more information on a command.
```

## Best Practices

### ✅ DO

- **Define metadata**: Add descriptions to commands, options, and arguments
- **Use typed options**: Types are shown in help automatically
- **Set default values**: Users see defaults in help
- **Mark required**: Users know what's mandatory

```csharp
cli.AddCommand<ProcessCommand>("process", "Process data files")
   .AddArgument<string>("input", "Input file path", required: true)
   .AddOption<string>("output", 'o', "Output directory", defaultValue: "./output")
   .AddOption<int>("threads", 't', "Number of threads", defaultValue: 4)
   .AddOption<bool>("verbose", 'v', "Verbose logging");
```

### ❌ DON'T

- **Don't handle --help manually** (unless you use `WithoutHelp()`)
- **Don't forget descriptions** (help becomes less useful)
- **Don't use generic descriptions** like "An option"

## Migration Guide

### Step 1: Remove Manual Help

**Before:**
```csharp
public Task<int> ExecuteAsync(CommandContext context, CancellationToken ct)
{
    if (context.Arguments.HasOption("help"))  // ← Remove this
    {
        ShowHelp();
        return Task.FromResult(0);
    }
    // ...
}
```

**After:**
```csharp
public Task<int> ExecuteAsync(CommandContext context, CancellationToken ct)
{
    // Just business logic - help is automatic!
    // ...
}
```

### Step 2: Add Metadata

```csharp
cli.AddCommand<MyCommand>("mycommand", "Clear description")
   .AddArgument<string>("arg1", "What is this argument?", required: true)
   .AddOption<int>("opt1", 'o', "What does this option do?", defaultValue: 10);
```

### Step 3: Test

```bash
$ myapp mycommand --help
```

Verify the help looks good and contains all the information users need.

## Advanced: Conditional Help

If you need to show help conditionally but still use auto-help:

```csharp
public Task<int> ExecuteAsync(CommandContext context, CancellationToken ct)
{
    if (context.Arguments.Positional.Count == 0)
    {
        Console.WriteLine("No input provided. Use --help for usage information.");
        return Task.FromResult(1);
    }

    // Normal execution...
}
```

## Summary

- ✅ **All commands** have automatic help by default
- ✅ Help is generated from **Arguments** and **Options** metadata
- ✅ Shows **types**, **defaults**, **required** indicators
- ✅ Works for **hierarchical** commands
- ✅ Can opt-out with **`WithoutHelp()`**
- ✅ **Cleaner code** - no manual help handling

The automatic help system makes your CLI more consistent, maintainable, and user-friendly! 🚀
