using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree;
internal abstract class BinaryOperator : AST
{
    public AST FirstParameter { get; }
    public AST SecondParameter { get; }
    public Type ReturnType { get; protected set; }


    public BinaryOperator(AST firstParameter, AST secondParameter, Token? token = null) : base(token)
    {
        FirstParameter = firstParameter;
        SecondParameter = secondParameter;
    }


    public override DataTypes.Object? Call() => Calculate(FirstParameter.Call()!, SecondParameter.Call()!);
    public override Type GetType() => ReturnType;
    public override string ToString() => $"{nameof(BinaryOperator)}";

    public abstract DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter);

    protected Type GetReturnType(BinaryOperation operation)
    {
        try
        {
            return ((Dictionary<BinaryOperation, Dictionary<Type, Type>>)Type.GetUnderlyingType(FirstParameter.GetType())
                                                                             .GetField("BinaryOperations")!
                                                                             .GetValue(null)!)[operation][SecondParameter.GetType()];
        }
        catch (KeyNotFoundException)
        {
            throw ExceptionCreator.BinaryOperationNotSupported(Token!.Value, FirstParameter.GetType(), SecondParameter.GetType());
        }
    }
}
