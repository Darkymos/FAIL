using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.DataTypes;
internal class Object : AST
{
    public dynamic Value { get; }


    public Object(dynamic value, Token? token = null) : base(token) => Value = value;


    public override dynamic? Call() => Value;
    public override string ToString() => $"{nameof(Object)}";
}
