using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Reference : AST
{
    private readonly AST Variable;


    public Reference(Scope scope, Token? token = null) : base(token) => Variable = Parser.GetValidVariable(scope, token!.Value.Value);


    public override dynamic? Call() => Variable.Call();
}
