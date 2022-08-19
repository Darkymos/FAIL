using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class Boolean : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Object, Object, Object>)>> BinaryOperations = new()
    {
        { BinaryOperation.And, new() {
            { new(nameof(Boolean)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value && second.Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { new(nameof(Boolean)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value == second.Value)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { new(nameof(Boolean)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value != second.Value)) },
        }},
        { BinaryOperation.Or, new() {
            { new(nameof(Boolean)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value || second.Value)) },
        }},
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Object, Object>)> UnaryOperations = new()
    {
        { UnaryOperation.Not, (new(nameof(Boolean)), (value) => new Boolean(!value.Value)) }
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Object, Object>>> Conversions = new()
    {
    };


    public Boolean(bool value, Token? token = null) : base(value, token)
    {
    }


    public override Type GetType() => new(nameof(Boolean));
}
