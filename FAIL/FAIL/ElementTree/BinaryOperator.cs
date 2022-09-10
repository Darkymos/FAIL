using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree;
internal class BinaryOperator : AST
{
    public BinaryOperation Operation { get; }
    public AST FirstParameter { get; }
    public AST SecondParameter { get; }
    public (Type Type, Func<Instance, Instance, Instance> Function) ReturnMetadata { get; }


    public BinaryOperator(BinaryOperation operation, AST firstParameter, AST secondParameter, Token? token = null) : base(token)
    {
        Operation = operation;
        FirstParameter = firstParameter;
        SecondParameter = secondParameter;

        ReturnMetadata = GetReturnMetadata(operation);
    }


    public override Instance? Call() => ReturnMetadata.Function.Invoke(FirstParameter.Call()!, SecondParameter.Call()!);
    public override Type GetType() => ReturnMetadata.Type;
    public override string ToString() => $"{nameof(BinaryOperator)}.{Operation}";

    private (Type, Func<Instance, Instance, Instance>) GetReturnMetadata(BinaryOperation operation)
    {
        try
        {
            return ((Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>>)
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
