namespace FAIL.Exceptions;
internal class OverloadAlreadyExists : FAILException
{
    public string FunctionName { get; }


    public OverloadAlreadyExists(string functionName, string message, uint line, uint column, string file) : base(message, line, column, file)
        => FunctionName = functionName;
}
