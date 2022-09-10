namespace FAIL.LanguageIntegration;

internal interface ITokenizer
{
    List<Token> Call(string code, string fileName);
}