using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class String : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Object, Object, Object>)>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { new(nameof(String)), (new(nameof(String)), (first, second) => new String(first.Value + second.Value)) },
            { new(nameof(Char)), (new(nameof(String)), (first, second) => new String(first.Value + second.Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { new(nameof(String)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value == second.Value)) },
            { new(nameof(Char)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value == second.Value.ToString())) },
        }},
        { BinaryOperation.NotEqual, new() {
            { new(nameof(String)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value != second.Value)) },
            { new(nameof(Char)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value != second.Value)) },
        }},
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Object, Object>)> UnaryOperations = new()
    {
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Object, Object>>> Conversions = new()
    {
    };


    public String(string value, Token? token = null) : base(value, token)
    {
    }


    public override Type GetType() => new(nameof(String));
}
