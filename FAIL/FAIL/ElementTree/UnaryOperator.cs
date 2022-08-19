using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree;
internal class UnaryOperator : AST
{
    public UnaryOperation Operation { get; }
    public AST Parameter { get; }
    public (Type Type, Func<DataTypes.Object, DataTypes.Object> Function) ReturnMetadata { get; }


    public UnaryOperator(UnaryOperation operation, AST parameter, Token? token = null) : base(token)
    {
        Operation = operation;
        Parameter = parameter;

        ReturnMetadata = GetReturnType(operation);
    }

    public override DataTypes.Object? Call() => ReturnMetadata.Function.Invoke(Parameter.Call()!);
    public override Type GetType() => ReturnMetadata.Type;
    public override string ToString() => $"{nameof(UnaryOperator)}.{Operation}";

    private (Type, Func<DataTypes.Object, DataTypes.Object>) GetReturnType(UnaryOperation operation)
    {
        try
        {
            return ((Dictionary<UnaryOperation, (Type, Func<DataTypes.Object, DataTypes.Object>)>)
                Type.GetUnderlyingType(Parameter.GetType())
                    .GetField("UnaryOperations")!
                    .GetValue(null)!)[operation];
        }
        catch (KeyNotFoundException)
        {
            throw ExceptionCreator.UnaryOperationNotSupported(Token!.Value, Parameter.GetType());
        }
    }
}
