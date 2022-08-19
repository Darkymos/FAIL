using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class Boolean : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, Type>> BinaryOperations = new()
    {
        { BinaryOperation.And, new() {
            { new(nameof(Boolean)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.Equal, new() {
            { new(nameof(Boolean)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { new(nameof(Boolean)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.Or, new() {
            { new(nameof(Boolean)), new(nameof(Boolean)) },
        }},
    };

    public static new readonly Dictionary<UnaryOperation, Type> UnaryOperations = new()
    {
        { UnaryOperation.Not, new(nameof(Boolean)) }
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Object, Object>>> Conversions = new()
    {
    };


    public Boolean(bool value, Token? token = null) : base(value, token)
    {
    }


    public override Type GetType() => new(nameof(Boolean));
}
