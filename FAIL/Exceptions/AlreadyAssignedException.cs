namespace FAIL.Exceptions;
internal class AlreadyAssignedException : FAILException
{
    public string Name { get; init; }


    public AlreadyAssignedException(string name, string message, uint line, uint column, string file) : base(message, line, column, file) => Name = name;
}
