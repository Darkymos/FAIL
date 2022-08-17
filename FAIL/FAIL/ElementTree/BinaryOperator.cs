using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal abstract class BinaryOperator : AST
{
    public AST FirstParameter { get; init; }
    public AST SecondParameter { get; init; }


    public BinaryOperator(AST firstParameter, AST secondParameter, Token? token = null) : base(token)
    {
        FirstParameter = firstParameter;
        SecondParameter = secondParameter;
    }


    public override DataTypes.Object? Call() => Calculate(FirstParameter.Call()!, SecondParameter.Call()!);
    public override Type GetType() => GetCombinedType();
    public override string ToString() => $"{nameof(BinaryOperator)}";

    public abstract DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter);

    protected Type GetCombinedType()
    {
        if (FirstParameter.GetType() == SecondParameter.GetType()) return FirstParameter.GetType();
        if (FirstParameter.GetType().Name == nameof(DataTypes.Double) || SecondParameter.GetType().Name == nameof(DataTypes.Double))
            return new(nameof(DataTypes.Double));
        return new(nameof(String));


    }
}
