using FAIL.ElementTree;
using FAIL.LanguageIntegration;
using FAIL.Metadata;
using Object = FAIL.ElementTree.Object;
using Type = FAIL.ElementTree.Type;

namespace FAIL.BuiltIn.DataTypes;
internal class Integer : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { Double.Type, (Double.Type, (first, second) => new Instance(Double.Type, first.GetValueAs<Integer>().Value + second.GetValueAs<Double>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Integer>().Value + second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.Division, new() {
            { Double.Type, (Double.Type, (first, second) => new Instance(Double.Type, first.GetValueAs<Integer>().Value / second.GetValueAs<Double>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Integer>().Value / second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { Double.Type, (Boolean.Type,(first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value == second.GetValueAs<Double>().Value)) },
            { Type, (Boolean.Type,(first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value == second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.GreaterThan, new() {
            { Double.Type, (Boolean.Type,(first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value > second.GetValueAs<Double>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value > second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.GreaterThanOrEqual, new() {
            { Double.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value >= second.GetValueAs<Double>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value >= second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.LessThan, new() {
            { Double.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value < second.GetValueAs<Double>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value < second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.LessThanOrEqual, new() {
            { Double.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value <= second.GetValueAs<Double>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value <= second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.Multiplication, new() {
            { Double.Type, (Double.Type, (first, second) => new Instance(Double.Type, first.GetValueAs<Integer>().Value * second.GetValueAs<Double>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Integer>().Value * second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { Double.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value != second.GetValueAs<Double>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Integer>().Value != second.GetValueAs<Integer>().Value)) },
        }},
        { BinaryOperation.Substraction, new() {
            { Double.Type, (Double.Type, (first, second) => new Instance(Double.Type, first.GetValueAs<Integer>().Value - second.GetValueAs<Double>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Integer>().Value - second.GetValueAs<Integer>().Value)) },
        }}
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Instance, Instance>)> UnaryOperations = new()
    {
        { UnaryOperation.Negation, (Type, (value) => new Instance(Type, -value.GetValueAs<Integer>().Value)) }
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>> Conversions = new()
    {
        { ConversionType.Explicit, new() {
            { String.Type, (value) => new Instance(String.Type, value.GetValueAs<Integer>().Value.ToString(), value.Token) }
        }},
    };


    public int Value { get; }

    public static new Type Type => new(nameof(Integer));


    public Integer(int value, Token? token = null) : base(nameof(Integer), token) => Value = value;


    public override string ToString() => Value.ToString();
}
