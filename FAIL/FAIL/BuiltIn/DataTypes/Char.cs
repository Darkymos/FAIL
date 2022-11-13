using FAIL.ElementTree;
using FAIL.LanguageIntegration;
using FAIL.Metadata;
using Object = FAIL.ElementTree.Object;
using Type = FAIL.ElementTree.Type;

namespace FAIL.BuiltIn.DataTypes;
internal class Char : Object
{
    public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>> BinaryOperations = new()
    {
        { BinaryOperation.Addition, new() {
            { Type, (String.Type, (first, second) => new Instance(String.Type, first.GetValueAs<Char>().Value.ToString() + second.GetValueAs<Char>().Value)) },
            { String.Type, (String.Type, (first, second) => new Instance(String.Type, first.GetValueAs<Char>().Value + second.GetValueAs<String>().Value)) },
        }},
        { BinaryOperation.Equal, new() {
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Char>().Value == second.GetValueAs<Char>().Value)) },
            { String.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Char>().Value.ToString() == second.GetValueAs<String>().Value)) },
        }},
        { BinaryOperation.NotEqual, new() {
            { Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Char>().Value != second.GetValueAs<Char>().Value)) },
            { String.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<Char>().Value.ToString() != second.GetValueAs<String>().Value)) },
        }},
    };

    public static new readonly Dictionary<UnaryOperation, (Type, Func<Instance, Instance>)> UnaryOperations = new()
    {
    };

    public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>> Conversions = new()
    {
    };


    public char Value { get; }

    public static new Type Type => new(nameof(Char));


    public Char(char value, Token? token = null) : base(nameof(Char), token) => Value = value;


    public override string ToString() => Value.ToString();
}
