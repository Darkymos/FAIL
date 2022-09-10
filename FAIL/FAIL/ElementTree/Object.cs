using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree;
internal abstract class Object : AST
{
    public static readonly Dictionary<BinaryOperation, Dictionary<Type, (Type, Func<Instance, Instance, Instance>)>> BinaryOperations = new()
    {
    };

    public static readonly Dictionary<UnaryOperation, (Type, Func<Instance, Instance>)> UnaryOperations = new()
    {
    };

    public static readonly Dictionary<ConversionType, Dictionary<Type, Func<Instance, Instance>>> Conversions = new()
    {
    };


    public string Name { get; }

    public static Type Type => new(nameof(Object));


    public Object(string name, Token? token = null) : base(token) => Name = name;


    public override Instance? Call() => throw new NotImplementedException();
    public override Type GetType() => new(Name);
}
