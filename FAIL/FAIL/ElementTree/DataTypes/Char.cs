using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class Char : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, Type>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { new(nameof(String)), new(nameof(String)) },
        }},
        { BinaryOperation.Equal, new() {
            { new(nameof(Char)), new(nameof(Boolean)) },
            { new(nameof(String)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.Multiplication, new() {
            { new(nameof(Integer)), new(nameof(String)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { new(nameof(Char)), new(nameof(Boolean)) },
            { new(nameof(String)), new(nameof(Boolean)) },
        }},
    };

    public static new readonly Dictionary<UnaryOperation, Type> UnaryOperations = new()
    {
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Object, Object>>> Conversions = new()
    {
    };


    public Char(char value, Token? token = null) : base(value, token)
    {
    }


    public override Type GetType() => new(nameof(Char));
}
