namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Parses command-line arguments according to POSIX and GNU conventions.
/// </summary>
public sealed class ArgumentParser
{
    private readonly ArgumentParserOptions _options;

    public ArgumentParser(ArgumentParserOptions? options = null)
    {
        _options = options ?? new ArgumentParserOptions();
    }

    /// <summary>
    /// Parses the command-line arguments.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    /// <returns>Parsed arguments.</returns>
    public ParsedArguments Parse(string[] args)
    {
        var result = new ParsedArguments();
        var i = 0;
        var endOfOptions = false;

        while (i < args.Length)
        {
            var arg = args[i];

            // -- indicates end of options
            if (arg == "--" && !endOfOptions)
            {
                endOfOptions = true;
                i++;
                continue;
            }

            // After --, everything is positional
            if (endOfOptions)
            {
                result.AddPositional(arg);
                i++;
                continue;
            }

            // Long option: --option or --option=value
            if (arg.StartsWith("--") && arg.Length > 2)
            {
                i = ParseLongOption(args, i, result);
                continue;
            }

            // Short option: -o or -o value or -abc (combined)
            if (arg.StartsWith("-") && arg.Length > 1 && arg[1] != '-')
            {
                i = ParseShortOption(args, i, result);
                continue;
            }

            // Windows style: /option
            if (_options.AllowWindowsStyle && arg.StartsWith("/") && arg.Length > 1)
            {
                i = ParseWindowsOption(args, i, result);
                continue;
            }

            // Positional argument
            result.AddPositional(arg);
            i++;
        }

        return result;
    }

    private int ParseLongOption(string[] args, int index, ParsedArguments result)
    {
        var arg = args[index];
        var option = arg.Substring(2);
        var equalIndex = option.IndexOf('=');

        if (equalIndex > 0)
        {
            // --option=value
            var name = option.Substring(0, equalIndex);
            var value = option.Substring(equalIndex + 1);
            result.AddOption(name, value);
            return index + 1;
        }

        // Long options without = are treated as flags (no value from next argument)
        result.AddOption(option);
        return index + 1;
    }

    private int ParseShortOption(string[] args, int index, ParsedArguments result)
    {
        var arg = args[index];
        var options = arg.Substring(1);

        // Single short option: -o or -o value
        if (options.Length == 1)
        {
            var option = options[0].ToString();

            // Check if next arg is a value
            if (index + 1 < args.Length && !IsOption(args[index + 1]))
            {
                result.AddOption(option, args[index + 1]);
                return index + 2;
            }

            result.AddOption(option);
            return index + 1;
        }

        // Combined short options: -abc means -a -b -c
        if (_options.AllowCombinedShortOptions)
        {
            foreach (var opt in options)
            {
                result.AddOption(opt.ToString());
            }
            return index + 1;
        }

        // Treat as a single option if combined not allowed
        result.AddOption(options);
        return index + 1;
    }

    private int ParseWindowsOption(string[] args, int index, ParsedArguments result)
    {
        var arg = args[index];
        var option = arg.Substring(1);

        // Check if next arg is a value
        if (index + 1 < args.Length && !IsOption(args[index + 1]))
        {
            result.AddOption(option, args[index + 1]);
            return index + 2;
        }

        result.AddOption(option);
        return index + 1;
    }

    private bool IsOption(string arg)
    {
        if (string.IsNullOrEmpty(arg))
            return false;

        return arg.StartsWith("-") || (_options.AllowWindowsStyle && arg.StartsWith("/"));
    }
}

/// <summary>
/// Options for configuring the argument parser.
/// </summary>
public sealed class ArgumentParserOptions
{
    /// <summary>
    /// Gets or sets whether to allow Windows-style options (/option).
    /// </summary>
    public bool AllowWindowsStyle { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to allow combined short options (-abc = -a -b -c).
    /// </summary>
    public bool AllowCombinedShortOptions { get; set; } = true;
}
