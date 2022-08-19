using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class Object : AST
{
    public static readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Object, Object, Object>)>> BinaryOperations = new()
    {
    };

    public static readonly Dictionary<UnaryOperation, (Type, Func<Object, Object>)> UnaryOperations = new()
    {
    };

    public static readonly Dictionary<ConversionType, Dictionary<Type, Func<Object, Object>>> Conversions = new()
    {
    };


    public dynamic Value { get; }


    public Object(object value, Token? token = null) : base(token) => Value = value;


    public override Object? Call() => this;
    public override Type GetType() => new(nameof(Object));
}
