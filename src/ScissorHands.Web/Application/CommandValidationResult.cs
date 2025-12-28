namespace ScissorHands.Web.Application;

using ScissorHands.Core.Options;

/// <summary>
/// This represents the record entity for command validation result.
/// </summary>
/// <param name="Mode"><see cref="CommandMode"/> value.</param>
/// <param name="IsHelp">Indicates whether the command is a help command.</param>
/// <param name="IsError">Indicates whether the command resulted in an error.</param>
public sealed record CommandValidationResult(CommandMode Mode, bool IsHelp, bool IsError)
{
    /// <summary>
    /// This represents the help command validation result.
    /// </summary>
    public static CommandValidationResult Help() => new(CommandMode.Help, IsHelp: true, IsError: false);

    /// <summary>
    /// This represents the unknown command validation result.
    /// </summary>
    public static CommandValidationResult ErrorUnknown() => new(CommandMode.Unknown, IsHelp: false, IsError: true);

    /// <summary>
    /// This represents the OK command validation result.
    /// </summary>
    public static CommandValidationResult Ok(CommandMode mode) => new(mode, IsHelp: false, IsError: false);
}
