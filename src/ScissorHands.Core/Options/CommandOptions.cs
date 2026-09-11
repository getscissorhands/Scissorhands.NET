namespace ScissorHands.Core.Options;

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
        ArgumentNullException.ThrowIfNull(args);

        var mode = CommandMode.Unknown;
        foreach (var arg in args)
        {
            if (string.Equals(arg, "--preview", StringComparison.OrdinalIgnoreCase))
            {
                return new CommandOptions(CommandMode.Preview);
            }

            if (string.Equals(arg, "--build", StringComparison.OrdinalIgnoreCase))
            {
                mode = CommandMode.Build;
            }
            else if (mode == CommandMode.Unknown && string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase))
            {
                mode = CommandMode.Help;
            }
        }

        return new CommandOptions(mode);
    }
}
