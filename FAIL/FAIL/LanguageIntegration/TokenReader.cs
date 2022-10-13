namespace FAIL.LanguageIntegration;
internal sealed class TokenReader
{
    public List<Token>? Tokens { get; }
    public int CurrentIndex { get; private set; }
    public Token? CurrentToken => CurrentIndex < Tokens!.Count ? Tokens![CurrentIndex] : null;
    public Token? LastToken => CurrentIndex != 0 ? Tokens![CurrentIndex - 1] : null;


    public TokenReader(List<Token>? tokens) => Tokens = tokens;


    public dynamic GetValue() => CurrentToken!.Value.Value;

    public Token? GetNextToken()
    {
        CurrentIndex++;
        return CurrentToken;
    }
    public Token? ConsumeCurrentToken(TokenType? expected = null)
    {
        return expected is not null && !expected!.Value.HasFlag(CurrentToken!.Value.Type)
            ? throw ExceptionCreator.InvalidToken(CurrentToken!.Value, expected!.Value)
            : GetNextToken();
    }

    public bool HasValue(dynamic value) => CurrentToken!.Value.Value == value;

    public bool IsEOT() => CurrentToken is null;
    public bool IsTypeOf(TokenType type) => CurrentToken!.Value.Type == type;
}

internal static class TokenReaderExtensions
{
    public static dynamic GetValue(this TokenReader _, Token? token) => token!.Value.Value;

    public static bool HasValue(this TokenReader _, Token? token, dynamic value) 
        => token!.Value.Value.GetType() == value.GetType() && token!.Value.Value == value;

    public static bool IsTypeOf(this TokenReader _, TokenType type, Token? token) => token!.Value.Type == type;
}
