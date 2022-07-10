using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Reference : AST
{
    private readonly AST Variable;


    public Reference(List<AST?> scope, Token? token = null) : base(token)
    {
        var variable = scope.Where(x => x is Variable variable && variable.Name == token!.Value.Value)
                            .FirstOrDefault();
        if (variable is null) variable = scope.Where(x => x is Function function && function.Name == token!.Value.Value)
                                              .FirstOrDefault();
        if (variable is null) throw ExceptionCreator.NotAssignedInScope(token!.Value.Value);

        Variable = variable;
    }


    public override dynamic? Call() => Variable.Call();
}
