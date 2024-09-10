namespace FAIL.Exceptions;
internal class InvalidTypeException : FAILException
{
    public string TargetName { get; }
    public ElementTree.Type Given { get; }
    public ElementTree.Type Expected { get; }


    public InvalidTypeException(string targetName,
                                ElementTree.Type given,
                                ElementTree.Type expected,
                                string message, uint line, uint column, string file) : base(message, line, column, file)
    {
        TargetName = targetName;
        Given = given;
        Expected = expected;
    }
}
