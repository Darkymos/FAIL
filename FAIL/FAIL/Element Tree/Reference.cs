using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Reference : AST
{
    private readonly Variable Variable;


    public Reference(List<AST?> scope, Token? token = null) : base(token)
    {
        Variable = scope.Where(x => x is Variable variable && variable.Name == token!.Value.Value)
                        .FirstOrDefault() as Variable ?? throw ExceptionCreator.NotAssignedInScope(token!.Value.Value);
    }


    public override dynamic? Call() => Variable.Call();
}
