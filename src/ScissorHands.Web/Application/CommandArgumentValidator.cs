namespace ScissorHands.Web.Application;

using ScissorHands.Core.Options;

/// <summary>
/// This represents a validator entity for command arguments.
/// </summary>
public static class CommandArgumentValidator
{
    /// <summary>
    /// Validates the command arguments.
    /// </summary>
    /// <param name="args">List of command arguments.</param>
    /// <returns>Returns <see cref="CommandValidationResult"/> value.</returns>
    public static CommandValidationResult Validate(IEnumerable<string> args)
    {
        var command = CommandOptions.Parse(args);

        return command.Mode switch
        {
            CommandMode.Help => CommandValidationResult.Help(),
            CommandMode.Unknown => CommandValidationResult.ErrorUnknown(),
            _ => CommandValidationResult.Ok(command.Mode)
        };
    }
}
