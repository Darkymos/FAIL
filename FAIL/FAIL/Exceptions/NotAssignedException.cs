namespace FAIL.Exceptions;
internal class NotAssignedException : Exception
{
    public string VarName { get; init; }


    public NotAssignedException(string varName, string? message) : base(message) => VarName = varName;
}
