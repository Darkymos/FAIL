namespace FAIL.Exceptions;
internal class WrongCountException : FAILException
{
    public int Expected { get; }
    public int Given { get; }


    public WrongCountException(int expected, int given, string message, uint line, uint column, string file) : base(message, line, column, file)
    {
        Expected = expected;
        Given = given;
    }
}
