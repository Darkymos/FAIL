using FAIL.ElementTree;
using FAIL.LanguageIntegration;
using FAIL.Metadata;
using Type = FAIL.ElementTree.Type;

namespace FAIL.BuiltIn.DataTypes;
internal class Boolean : ElementTree.Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>> BinaryOperations = new()
    {
        { BinaryOperation.And, new() {
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Boolean>().Value && second.GetValueAs<Boolean>().Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<Boolean>().Value == second.GetValueAs<Boolean>().Value)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { Type, (Type,(first, second) => new Instance(Type, first.GetValueAs<Boolean>().Value != second.GetValueAs<Boolean>().Value)) },
        }},
        { BinaryOperation.Or, new() {
            { Type, (Type,(first, second) => new Instance(Type, first.GetValueAs<Boolean>().Value || second.GetValueAs<Boolean>().Value)) },
        }},
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Instance, Instance>)> UnaryOperations = new()
    {
        { UnaryOperation.Not, (Type, (value) => new Instance(Type, !value.GetValueAs<Boolean>().Value)) }
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>> Conversions = new()
    {
    };


    public bool Value { get; }

    public static new Type Type => new(nameof(Boolean));


    public Boolean(bool value, Token? token = null) : base(nameof(Boolean), token) => Value = value;


    public override string ToString() => Value.ToString();
}
