using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class Integer : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, Type>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { new(nameof(Integer)), new(nameof(Integer)) }, 
            { new(nameof(Double)), new(nameof(Double)) }, 
        }},
        { BinaryOperation.Division, new() {
            { new(nameof(Integer)), new(nameof(Integer)) },
            { new(nameof(Double)), new(nameof(Double)) },
        }},
        { BinaryOperation.Equal, new() {
            { new(nameof(Integer)), new(nameof(Boolean)) },
            { new(nameof(Double)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.GreaterThan, new() {
            { new(nameof(Integer)), new(nameof(Boolean)) },
            { new(nameof(Double)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.GreaterThanOrEqual, new() {
            { new(nameof(Integer)), new(nameof(Boolean)) },
            { new(nameof(Double)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.LessThan, new() {
            { new(nameof(Integer)), new(nameof(Boolean)) },
            { new(nameof(Double)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.LessThanOrEqual, new() {
            { new(nameof(Integer)), new(nameof(Boolean)) },
            { new(nameof(Double)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.Multiplication, new() {
            { new(nameof(Integer)), new(nameof(Integer)) },
            { new(nameof(Double)), new(nameof(Double)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { new(nameof(Integer)), new(nameof(Boolean)) },
            { new(nameof(Double)), new(nameof(Boolean)) },
        }},
        { BinaryOperation.Substraction, new() {
            { new(nameof(Integer)), new(nameof(Integer)) },
            { new(nameof(Double)), new(nameof(Double)) },
        }}
    };

    public static new readonly Dictionary<UnaryOperation, Type> UnaryOperations = new()
    {
        { UnaryOperation.Negation, new(nameof(Integer)) }
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Object, Object>>> Conversions = new()
    {
        { ConversionType.Explicit, new() {
            { new(nameof(String)), (value) => new String(value.Value.ToString(), value.Token) },
            { new(nameof(Object)), (value) => new Object(value.Value, value.Token) },
        }},
    };


    public Integer(int value, Token? token = null) : base(value, token)
    {
    }


    public override Type GetType() => new(nameof(Integer));
}
