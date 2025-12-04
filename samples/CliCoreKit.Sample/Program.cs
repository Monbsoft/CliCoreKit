using Microsoft.Extensions.Hosting;
using Monbsoft.CliCoreKit.Core;
using Monbsoft.CliCoreKit.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureCli(cli =>
{
    cli.AddCommand<GreetCommand>("greet", "Greets a person")
       .AddCommand<AddCommand>("add", "Adds two numbers")
       .AddSubCommand<ListFilesCommand>("list", "file", "Lists files")
       .UseValidation();
});

var host = builder.Build();
var exitCode = await host.RunCliAsync(args);
return exitCode;

// Commands

public class GreetCommand : ICommand
{
    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
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
