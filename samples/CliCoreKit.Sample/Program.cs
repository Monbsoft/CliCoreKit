using Microsoft.Extensions.Hosting;
using Monbsoft.CliCoreKit.Core;
using Monbsoft.CliCoreKit.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureCli(cli =>
{
    cli.AddCommand<GreetCommand>("greet", "Greets a person")
       .AddCommand<AddCommand>("add", "Adds two numbers")
       .AddCommand<ListCommand>("list", "List operations")
       .AddSubCommand<ListFilesCommand>("file", "list", "Lists files")       
       .UseValidation();  
});

var host = builder.Build();
var exitCode = await host.RunCliAsync(args);
return exitCode;

// Commands

public class ListCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Please specify a subcommand. Use 'list --help' for available subcommands.");
        return Task.FromResult(1);
    }
}

public class GreetCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.Arguments.HasOption("help") || context.Arguments.HasOption("h"))
        {
            Console.WriteLine("Usage: greet [options] [name]");
            Console.WriteLine();
            Console.WriteLine("Greets a person.");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  name              The name to greet (default: World)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --name <name>     The name to greet");
            Console.WriteLine("  --formal          Use a formal greeting");
            Console.WriteLine("  -h, --help        Show this help message");
            return Task.FromResult(0);
        }

        var name = context.Arguments.GetOptionValue("name")
                   ?? context.Arguments.GetPositional(0)
                   ?? "World";

        var greeting = context.Arguments.HasOption("formal") ? "Good day" : "Hello";

        Console.WriteLine($"{greeting}, {name}!");
        return Task.FromResult(0);
    }
}

public class AddCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.Arguments.HasOption("help") || context.Arguments.HasOption("h"))
        {
            Console.WriteLine("Usage: add <number1> <number2>");
            Console.WriteLine();
            Console.WriteLine("Adds two numbers together.");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  number1           The first number");
            Console.WriteLine("  number2           The second number");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help        Show this help message");
            return Task.FromResult(0);
        }

        if (context.Arguments.Positional.Count < 2)
        {
            Console.Error.WriteLine("Usage: add <number1> <number2>");
            return Task.FromResult(1);
        }

        if (int.TryParse(context.Arguments.GetPositional(0), out var a) &&
            int.TryParse(context.Arguments.GetPositional(1), out var b))
        {
            Console.WriteLine($"{a} + {b} = {a + b}");
            return Task.FromResult(0);
        }

        Console.Error.WriteLine("Invalid numbers provided");
        return Task.FromResult(1);
    }
}

public class ListFilesCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.Arguments.HasOption("help") || context.Arguments.HasOption("h"))
        {
            Console.WriteLine("Usage: list file [options]");
            Console.WriteLine();
            Console.WriteLine("Lists files in a directory.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --path <path>     The directory path (default: current directory)");
            Console.WriteLine("  --pattern <pat>   File pattern to match (default: *.*)");
            Console.WriteLine("  -h, --help        Show this help message");
            return Task.FromResult(0);
        }

        var path = context.Arguments.GetOptionValue("path") ?? ".";
        var pattern = context.Arguments.GetOptionValue("pattern") ?? "*.*";

        try
        {
            var files = Directory.GetFiles(path, pattern);

            Console.WriteLine($"Files in '{path}' matching '{pattern}':");
            foreach (var file in files)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)}");
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
