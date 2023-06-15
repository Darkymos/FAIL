using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree;
internal class TypeConversion : AST
{
	public AST Value { get; }
	public Type NewType { get; }
	public Func<Instance, Instance> ConversionFunction { get; }

	public TypeConversion(AST value, Type newType, Token? token = null) : base(token)
	{
		Value = value;
		NewType = newType;

		try
		{
			ConversionFunction = ((Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>>)
				Type.GetType(Value.GetType())
					.GetField("Conversions")!
					.GetValue(null)!)[ConversionType.Explicit][NewType];
		}
		catch (KeyNotFoundException)
		{
			throw ExceptionCreator.ExplicitConversionNotSupported(Token!.Value, NewType, Value.GetType());
		}
	}

	public override Instance? Call() => ConversionFunction.Invoke(Value.Call()!);
	public override Type GetType() => NewType;

	public static T? ConvertTo<T>(Instance @object) where T : Instance => @object as T;
}
