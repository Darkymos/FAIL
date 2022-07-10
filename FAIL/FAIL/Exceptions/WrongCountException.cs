namespace FAIL.Exceptions;
internal class WrongCountException : Exception
{
    public int Expected { get; }
    public int Given { get; }

    public WrongCountException(int expected, int given, string message) : base(message)
    {
        Expected = expected;
        Given = given;
    }
}
