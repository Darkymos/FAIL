using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Reference : AST
{
    private readonly AST Variable;


    public Reference(List<AST?> scope, Token? token = null) : base(token)
    {
        var variable = Parser.GetVariableFromScope(scope, token!.Value.Value);
        if (variable is null) variable = Parser.GetFunctionFromScope(scope, token!.Value.Value);
        if (variable is null) throw ExceptionCreator.NotAssignedInScope(token!.Value.Value);

        Variable = variable;
    }


    public override dynamic? Call() => Variable.Call();
}
