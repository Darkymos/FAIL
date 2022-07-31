using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.DataTypes;
internal class Object : AST
{
    public dynamic Value { get; } // has to be dynamic, so it hasn't to be overridden in every type


    public Object(object value, Token? token = null) : base(token) => Value = value; // here, the actual type of the data is defined


    public override Object? Call() => this;
    public override Type GetType() => new(nameof(Object));
}
