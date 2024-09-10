using FAIL.ElementTree;
using FAIL.LanguageIntegration;
using FAIL.Metadata;
using Type = FAIL.ElementTree.Type;

namespace FAIL.BuiltIn.DataTypes;
internal class String : ElementTree.Object
{
	public static new readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>> BinaryOperations = new()
	{
		{ BinaryOperation.Addition, new() {
			{ Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<String>().Value + second.GetValueAs<String>().Value)) },
			{ Char.Type, (Type, (first, second) => new Instance(Type, first.GetValueAs<String>().Value + second.GetValueAs<Char>().Value.ToString())) },
		}},
		{ BinaryOperation.Equal, new() {
			{ Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<String>().Value == second.GetValueAs<String>().Value)) },
			{ Char.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<String>().Value == second.GetValueAs<Char>().Value.ToString())) },
		}},
		{ BinaryOperation.NotEqual, new() {
			{ Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<String>().Value != second.GetValueAs<String>().Value)) },
			{ Char.Type, (Boolean.Type, (first, second) => new Instance(Boolean.Type, first.GetValueAs<String>().Value != second.GetValueAs<Char>().Value.ToString())) },
		}},
	};

	public static new readonly Dictionary<UnaryOperation, (Type, Func<Instance, Instance>)> UnaryOperations = new()
	{
	};

	public static new readonly Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>> Conversions = new()
	{
		{ ConversionType.Explicit, new() {
			{ Integer.Type, (value) => new Instance(Integer.Type, Convert.ToInt32(value.GetValueAs<String>().Value), value.Token) }
		}},
	};


	public string Value { get; }

	public static new Type Type => new(nameof(String));


	public String(string value, Token? token = null) : base(nameof(String), token) => Value = value;


	public override string ToString() => Value;
}
