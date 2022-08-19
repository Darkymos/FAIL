using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree;
internal class BinaryOperator : AST
{
    public BinaryOperation Operation { get; }
    public AST FirstParameter { get; }
    public AST SecondParameter { get; }
    public (Type Type, Func<DataTypes.Object, DataTypes.Object, DataTypes.Object> Function) ReturnMetadata { get; }


    public BinaryOperator(BinaryOperation operation, AST firstParameter, AST secondParameter, Token? token = null) : base(token)
    {
        Operation = operation;
        FirstParameter = firstParameter;
        SecondParameter = secondParameter;

        ReturnMetadata = GetReturnMetadata(operation);
    }


    public override DataTypes.Object? Call() => ReturnMetadata.Function.Invoke(FirstParameter.Call()!, SecondParameter.Call()!);
    public override Type GetType() => ReturnMetadata.Type;
    public override string ToString() => $"{nameof(BinaryOperator)}.{Operation}";

    private (Type, Func<DataTypes.Object, DataTypes.Object, DataTypes.Object>) GetReturnMetadata(BinaryOperation operation)
    {
        try
        {
            return ((Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<DataTypes.Object, DataTypes.Object, DataTypes.Object>)>>)
                Type.GetUnderlyingType(FirstParameter.GetType())
                    .GetField("BinaryOperations")!
                    .GetValue(null)!)[operation][SecondParameter.GetType()];
        }
        catch (KeyNotFoundException)
        {
            throw ExceptionCreator.BinaryOperationNotSupported(Token!.Value, FirstParameter.GetType(), SecondParameter.GetType());
        }
    }
}
