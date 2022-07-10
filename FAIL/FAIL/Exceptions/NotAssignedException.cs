namespace FAIL.Exceptions;
internal class NotAssignedException : Exception
{
    public string Name { get; init; }


    public NotAssignedException(string name, string? message) : base(message) => Name = name;
}
