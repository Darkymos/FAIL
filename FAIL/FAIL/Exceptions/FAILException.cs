namespace FAIL.Exceptions;
internal class FAILException : Exception
{
    public override string Message { get; }
    public uint Line { get; }
    public uint Column { get; }
    public string File { get; }


    public FAILException(string message, uint line, uint column, string file)
    {
        Message = message;
        Line = line;
        Column = column;
        File = file;
    }
}
