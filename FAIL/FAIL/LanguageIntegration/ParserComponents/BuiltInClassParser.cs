using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class BuiltInClassParser : IParserComponent
{
    private readonly TokenReader Reader;


    public BuiltInClassParser(TokenReader reader) => Reader = reader;


    public Instance Parse(ElementTree.Type type, Token? token)
    {
        var builtInType = System.Type.GetType($"FAIL.BuiltIn.DataTypes.{type.Name}");

        return builtInType is not null
            ? new Instance(type, token!.Value.Value, token)
            : throw new NotImplementedException();
    }
}
