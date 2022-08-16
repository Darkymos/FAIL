using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class For : AST
{
    public AST IteratorVariable { get; }
    public AST IteratorTest { get; }
    public AST IteratorAction { get; }
    public CommandList Body { get; }


    public For(AST iteratorVariable, AST iteratorTest, AST iteratorAction, CommandList body, Token? token = null) : base(token)
    {
        IteratorVariable = iteratorVariable;
        IteratorTest = iteratorTest;
        IteratorAction = iteratorAction;
        Body = body;
    }


    public override DataTypes.Object? Call()
    {
        try
        {
            _ = IteratorVariable.Call();

            while (IteratorTest.Call()!.Value)
            {
                try
                {
                    _ = Body.Call();
                    _ = IteratorAction.Call();
                }
                catch (ContinueException) { continue; }
            }

            return null;
        }
        catch (BreakException) { return null; }
    }
    public override Type GetType() => new("Undefined");
}
