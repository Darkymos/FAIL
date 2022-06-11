using FAIL.Language_Integration;

namespace FAIL.ElementTree.IntegratedFunctions;
internal class WriteLine : IntegratedFunction
{
    public AST? Value { get; }

    public WriteLine(Token? token = null, AST? value = null) : base(token) 
    { 
        Value = value;
    }

    public override object? Call()
    {
        Console.WriteLine(Value?.Call());

        return null;
    }
}
