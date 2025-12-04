using Microsoft.Extensions.Hosting;
using Monbsoft.CliCoreKit.Core;
using Monbsoft.CliCoreKit.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureCli(cli =>
{
    // Simple command with typed arguments and options
    cli.AddCommand<GreetCommand>("greet", "Greets a person")
       .AddArgument<string>("name", "The name to greet", defaultValue: "World")
       .AddOption<string>("greeting", 'g', "Custom greeting message", defaultValue: "Hello")
       .AddOption<bool>("formal", 'f', "Use a formal greeting")
       .AddOption<int>("repeat", 'r', "Number of times to repeat", defaultValue: 1);
    
    // Command with required arguments
    cli.AddCommand<AddCommand>("add", "Adds two numbers")
       .AddArgument<int>("number1", "The first number", required: true)
       .AddArgument<int>("number2", "The second number", required: true)
       .AddOption<bool>("verbose", 'v', "Show detailed output");
    
    // Hierarchical commands
    cli.AddCommand<ListCommand>("list", "List operations")
       .AddCommand<ListFilesCommand>("files", "Lists files in a directory")
           .AddOption<string>("path", 'p', "Directory path", defaultValue: ".")
           .AddOption<string>("pattern", 't', "File pattern", defaultValue: "*.*")
           .AddOption<bool>("invert", 'i', "Invert sort order")
           .AddOption<bool>("recursive", 'r', "Search recursively");
    
    cli.UseValidation();
});

var host = builder.Build();
var exitCode = await host.RunCliAsync(args);
return exitCode;

// Commands implementation

public class ListCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Please specify a command. Use 'list --help' for available commands.");
        return Task.FromResult(1);
    }
}

public class GreetCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        // Clean API - default values come from definition!
        var name = context.GetArgument<string>("name");
        var greeting = context.GetOption<string>("greeting");
        var formal = context.GetOption<bool>("formal");
        var repeat = context.GetOption<int?>("repeat");

        var message = formal == true ? $"Good day, {name}!" : $"{greeting}, {name}!";

        int repeatCount = repeat ?? 1;
        for (int i = 0; i < repeatCount; i++)
        {
            Console.WriteLine(message);
        }

        return Task.FromResult(0);
    }
}

public class AddCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        // Access by name - more readable and less error-prone
        var a = context.GetArgument<int>("number1");
        var b = context.GetArgument<int>("number2");
        var verbose = context.GetOption<bool>("verbose");

        if (a == 0 && b == 0 && context.Positional.Count < 2)
        {
            Console.Error.WriteLine("Error: Two numbers are required.");
            Console.Error.WriteLine("Use 'add --help' for usage information.");
            return Task.FromResult(1);
        }

        var result = a + b;

        if (verbose)
        {
            Console.WriteLine($"Adding {a} and {b}...");
            Console.WriteLine($"Result: {result}");
        }
        else
        {
            Console.WriteLine(result);
        }

        return Task.FromResult(0);
    }
}

public class ListFilesCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        var path = context.GetOption<string>("path") ?? ".";
        var pattern = context.GetOption<string>("pattern") ?? "*.*";
        var invert = context.GetOption<bool>("invert");
        var recursive = context.GetOption<bool>("recursive");

        try
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(path, pattern, searchOption);
            
            var sortedFiles = invert 
                ? files.OrderByDescending(f => Path.GetFileName(f))
                : files.OrderBy(f => Path.GetFileName(f));

            Console.WriteLine($"Files in '{path}' matching '{pattern}'{(invert ? " (inverted)" : "")}{(recursive ? " (recursive)" : "")}:");
            foreach (var file in sortedFiles)
            {
                var relativePath = Path.GetRelativePath(path, file);
                Console.WriteLine($"  - {relativePath}");
            }

            Console.WriteLine($"\nTotal: {files.Length} file(s)");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return Task.FromResult(1);
        }
    }
}

