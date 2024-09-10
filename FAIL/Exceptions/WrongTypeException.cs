namespace FAIL.Exceptions;
internal class WrongTypeException : FAILException
{
    public WrongTypeException(string message, uint line, uint column, string file) : base(message, line, column, file)
    {
    }
}
