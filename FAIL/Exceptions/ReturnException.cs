namespace FAIL.Exceptions;
internal class ReturnException : Exception
{
    public dynamic? Value { get; }

    public ReturnException(dynamic? value) => Value = value;
}
