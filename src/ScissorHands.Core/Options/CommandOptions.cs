namespace ScissorHands.Core.Options;

/// <summary>
/// This specifies the command mode.
/// </summary>
public enum CommandMode
{
    /// <summary>
    /// Identifies the mode is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Identifies the preview mode.
    /// </summary>
    Preview,

    /// <summary>
    /// Identifies the build mode.
    /// </summary>
    Build
}

/// <summary>
/// This represents the record for the command options.
/// </summary>
/// <param name="Mode"><see cref="CommandMode"/> value.</param>
public sealed record CommandOptions(CommandMode Mode)
{
    /// <summary>
    /// Parses the command line arguments.
    /// </summary>
    /// <param name="args">List of command line arguments.</param>
    /// <returns>Parsed <see cref="CommandOptions"/> instance.</returns>
    public static CommandOptions Parse(IEnumerable<string> args)
    {
        if (args.Any(arg => string.Equals(arg, "--preview", StringComparison.OrdinalIgnoreCase)))
        {
            return new CommandOptions(CommandMode.Preview);
        }

        if (args.Any(arg => string.Equals(arg, "--build", StringComparison.OrdinalIgnoreCase)))
        {
            return new CommandOptions(CommandMode.Build);
        }

        return new CommandOptions(CommandMode.Unknown);
    }
}
