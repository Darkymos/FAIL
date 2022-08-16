using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class While : AST
{
    public AST TestCommand { get; }
    public CommandList Body { get; }


    public While(AST testCommand, CommandList body, Token? token = null) : base(token)
    {
        TestCommand = testCommand;
        Body = body;
    }


    public override DataTypes.Object? Call()
    {
        try
        {
            while (TestCommand.Call()!.Value) 
                try { _ = Body.Call(); }
                catch (ContinueException) { continue; }

            return null;
        }
        catch (BreakException) { return null; }
    }
    public override Type GetType() => new("Undefined");
}
