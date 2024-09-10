namespace FAIL.Exceptions;
internal class SyntaxException : FAILException
{
    public SyntaxException(string message, uint line, uint column, string file) : base(message, line, column, file)
    {
    }
}
