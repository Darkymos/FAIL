using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal abstract class UnaryOperator : AST
{
    public AST Parameter { get; }


    public UnaryOperator(AST parameter, Token? token = null) : base(token) => Parameter = parameter;


    public override DataTypes.Object? Call() => Calculate(Parameter.Call()!);
    public override Type GetType() => new(nameof(DataTypes.Boolean));
    public override string ToString() => $"{nameof(UnaryOperator)}";

    public abstract DataTypes.Object Calculate(DataTypes.Object parameter);
}
