using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Function : AST
{
    public string Name { get; }
    public Type ReturnType { get; }
    public CommandList Parameters { get; }
    public CommandList? Body { get; }


    public Function(string name, Type returnType, CommandList arguments, CommandList? body = null, Token? token = null) : base(token)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = arguments;
        Body = body;
    }


    public override dynamic? Call()
    {
        try
        {
            Body!.Call();
            return null;
        }
        catch (ReturnException ex)
        {
            return ex.Value;
        }
    }
}
