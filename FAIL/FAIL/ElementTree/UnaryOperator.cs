using FAIL.LanguageIntegration;
using FAIL.Metadata;
using System.Reflection.Metadata;

namespace FAIL.ElementTree;
internal abstract class UnaryOperator : AST
{
    public AST Parameter { get; }
    public Type ReturnType { get; protected set; }


    public UnaryOperator(AST parameter, Token? token = null) : base(token) => Parameter = parameter;

    public override DataTypes.Object? Call() => Calculate(Parameter.Call()!);
    public override Type GetType() => ReturnType;
    public override string ToString() => $"{nameof(UnaryOperator)}";

    public abstract DataTypes.Object Calculate(DataTypes.Object parameter);

    protected Type GetReturnType(UnaryOperation operation)
    {
        try
        {
            return ((Dictionary<UnaryOperation, Type>)Type.GetUnderlyingType(Parameter.GetType())
                                                          .GetField("UnaryOperations")!
                                                          .GetValue(null)!)[operation];
        }
        catch (KeyNotFoundException)
        {
            throw ExceptionCreator.UnaryOperationNotSupported(Token!.Value, Parameter.GetType());
        }
    }
}
