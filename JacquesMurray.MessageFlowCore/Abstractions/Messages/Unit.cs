namespace JacquesMurray.MessageFlowCore.Abstractions.Messages;

/// <summary>
/// Represents a void type, which is used as the return type for command handlers that don't return a value.
/// </summary>
public sealed class Unit
{
    /// <summary>
    /// The singleton instance of the Unit type.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Prevents external instantiation of the Unit class.
    /// </summary>
    private Unit() { }
}