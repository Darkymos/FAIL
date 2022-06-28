namespace FAIL.Exceptions;
internal class SyntaxException : Exception
{
    public string FileName = "";


    public SyntaxException(string message) : base(message) { }
    public SyntaxException(string message, string fileName) : base(message) 
        => FileName = fileName;
}
