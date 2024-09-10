using FAIL.ElementTree;
using FAIL.LanguageIntegration;
using FAIL.Metadata;
using static System.FormattableString;
using Object = FAIL.ElementTree.Object;
using Type = FAIL.ElementTree.Type;

namespace FAIL.BuiltIn.DataTypes;
internal class Double : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { Integer.Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value + second.GetValueAs<Integer>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value + second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.Division, new() {
            { Integer.Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value / second.GetValueAs<Integer>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value / second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { Integer.Type, (Boolean.Type,(first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value == second.GetValueAs<Integer>().Value)) },
            { Type, (Boolean.Type,(first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value == second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.GreaterThan, new() {
            { Integer.Type, (Boolean.Type,(first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value > second.GetValueAs<Integer>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value > second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.GreaterThanOrEqual, new() {
            { Integer.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value >= second.GetValueAs<Integer>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value >= second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.LessThan, new() {
            { Integer.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value < second.GetValueAs<Integer>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value < second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.LessThanOrEqual, new() {
            { Integer.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value <= second.GetValueAs<Integer>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value <= second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.Multiplication, new() {
            { Integer.Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value * second.GetValueAs<Integer>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value * second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { Integer.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value != second.GetValueAs<Integer>().Value)) },
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Double>().Value != second.GetValueAs<Double>().Value)) },
        }},
        { BinaryOperation.Substraction, new() {
            { Integer.Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value - second.GetValueAs<Integer>().Value)) },
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Double>().Value - second.GetValueAs<Double>().Value)) },
        }}
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Instance, Instance>)> UnaryOperations = new()
    {
        { UnaryOperation.Negation, (Type, (value) => new Instance(Type, -value.GetValueAs<Double>().Value)) }
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>> Conversions = new()
    {
    };


    public double Value { get; }

    public static new Type Type => new(nameof(Double));


    public Double(double value, Token? token = null) : base(nameof(Double), token) => Value = value;


    public override string ToString() => Invariant($"{Value}");
}
