using FAIL.ElementTree;

namespace FAIL.LanguageIntegration;

internal interface IParser
{
    CommandList Call(List<Token> tokens);
}