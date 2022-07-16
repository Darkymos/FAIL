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


    public override dynamic? Call()
    {
        while (TestCommand.Call()) Body.Call();
        return null;
    }
}
