using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.DataTypes;
internal class Object : AST
{
    public dynamic Value { get; }


    public Object(object value, Token? token = null) : base(token) => Value = value;


    public override dynamic? Call() => Value;
    public override string ToString() => GetType().Name;
}
