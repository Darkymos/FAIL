using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal abstract class AST
{
    public Token? Token { get; init; }


    public AST(Token? token = null) => Token = token;


    public abstract dynamic? Call();
    public override string ToString() => GetType().Name;
}
