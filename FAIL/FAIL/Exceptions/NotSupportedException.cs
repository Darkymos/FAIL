namespace FAIL.Exceptions;
internal class NotSupportedException : FAILException
{
    public NotSupportedException(string message, uint line, uint column, string file) : base(message, line, column, file)
    {
    }
}
