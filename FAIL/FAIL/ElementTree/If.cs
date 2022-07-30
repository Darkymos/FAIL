using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class If : AST
{
    public AST TestCommand { get; }
    public CommandList IfBody { get; }
    public AST? ElseBody { get; }


    public If(AST testCommand, CommandList ifBody, AST? elseBody, Token? token = null) : base(token)
    {
        TestCommand = testCommand;
        IfBody = ifBody;
        ElseBody = elseBody;
    }


    public override dynamic? Call()
    {
        if (TestCommand.Call()) IfBody.Call();
        else ElseBody?.Call();

        return null;
    }
    public override Type GetType() => new("Undefined");
}
