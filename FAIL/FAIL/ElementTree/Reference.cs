using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Reference : AST
{
    public Variable Variable { get; }


    public Reference(Variable variable, Token? token = null) : base(token) => Variable = variable;


    public override DataTypes.Object? Call()
    {
        if (!Variable.IsSet()) throw ExceptionCreator.UseOfUnassignedVariable(Variable.Name, Token);
        return Variable.Call();
    }

    public override Type GetType() => Variable.GetType();
}
