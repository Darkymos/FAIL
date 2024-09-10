namespace FAIL.Exceptions;
internal class NotAssignedException : FAILException
{
    public string Name { get; init; }


    public NotAssignedException(string name, string message, uint line, uint column, string file) : base(message, line, column, file) => Name = name;
}
