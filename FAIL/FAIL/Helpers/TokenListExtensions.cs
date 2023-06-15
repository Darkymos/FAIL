using FAIL.LanguageIntegration;

namespace FAIL.Helpers;
public static class TokenListExtensions
{
    public static List<Token> AcceptExpected(this List<Token> elements, TokenType type)
    {
        if (elements.First().Type != type) throw ExceptionCreator.InvalidToken(elements.First(), type);

        elements.RemoveAt(0);
        return elements;
    }
    public static Token AcceptAndGetExpected(this List<Token> elements, TokenType type)
    {
        var element = elements.First();

        if (element.Type != type) throw ExceptionCreator.InvalidToken(element, type);

        elements.RemoveAt(0);
        return element;
    }
}
