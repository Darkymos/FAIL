using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.DataTypes;
internal class Integer : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Object, Object, Object>)>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { new(nameof(Integer)), (new(nameof(Integer)), (first, second) => new Integer(first.Value + second.Value)) }, 
            { new(nameof(Double)), (new(nameof(Double)), (first, second) => new Double(first.Value + second.Value)) }, 
        }},
        { BinaryOperation.Division, new() {
            { new(nameof(Integer)), (new(nameof(Integer)), (first, second) => new Integer(first.Value / second.Value)) },
            { new(nameof(Double)), (new(nameof(Double)), (first, second) => new Double(first.Value / second.Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { new(nameof(Integer)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value == second.Value)) },
            { new(nameof(Double)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value == second.Value)) },
        }},
        { BinaryOperation.GreaterThan, new() {
            { new(nameof(Integer)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value > second.Value)) },
            { new(nameof(Double)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value > second.Value)) },
        }},
        { BinaryOperation.GreaterThanOrEqual, new() {
            { new(nameof(Integer)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value >= second.Value)) },
            { new(nameof(Double)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value >= second.Value)) },
        }},
        { BinaryOperation.LessThan, new() {
            { new(nameof(Integer)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value < second.Value)) },
            { new(nameof(Double)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value < second.Value)) },
        }},
        { BinaryOperation.LessThanOrEqual, new() {
            { new(nameof(Integer)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value <= second.Value)) },
            { new(nameof(Double)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value <= second.Value)) },
        }},
        { BinaryOperation.Multiplication, new() {
            { new(nameof(Integer)), (new(nameof(Integer)), (first, second) => new Integer(first.Value * second.Value)) },
            { new(nameof(Double)), (new(nameof(Double)), (first, second) => new Double(first.Value * second.Value)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { new(nameof(Integer)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value != second.Value)) },
            { new(nameof(Double)), (new(nameof(Boolean)), (first, second) => new Boolean(first.Value != second.Value)) },
        }},
        { BinaryOperation.Substraction, new() {
            { new(nameof(Integer)), (new(nameof(Integer)), (first, second) => new Integer(first.Value - second.Value)) },
            { new(nameof(Double)), (new(nameof(Double)), (first, second) => new Double(first.Value - second.Value)) },
        }}
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Object, Object>)> UnaryOperations = new()
    {
        { UnaryOperation.Negation, (new(nameof(Integer)), (value) => new Integer(-value.Value)) }
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
