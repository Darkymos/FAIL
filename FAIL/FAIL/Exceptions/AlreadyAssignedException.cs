namespace FAIL.Exceptions;
internal class AlreadyAssignedException : Exception
{
    public string Name { get; init; }


    public AlreadyAssignedException(string name, string? message) : base(message) => Name = name;
}
