using System.Globalization;
using System.Text;

namespace FAIL.LanguageIntegration;
public record struct Token(TokenType Type, dynamic Value, uint Row, uint Column, string FileName);

public sealed class Tokenizer : ITokenizer
{
    private readonly string FileName;
    private readonly List<char> Raw;
    private uint Row = 1;
    private uint Column = 0;
    private int CurrentPosition = -1;

    private StringBuilder Buffer = new();
    private State CurrentState = State.Start;
    private CommentState CommentState = CommentState.None;


    public Tokenizer(List<string> files)
    {
        FileName = files.First();
        Raw = File.ReadAllText(FileName).ToCharArray().ToList();
    }


    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        var index = 0;
        Raw.ToDictionary(x => index++).ToList().ForEach(x =>
        {
            var item = x.Value;

            if (CurrentPosition >= x.Key) return;

            CurrentPosition++;
            Column++;

            if (item is '\n')
            {
                EndOfLine();
                return;
            }

            if (CommentState is CommentState.Block && item is '*' && Raw.GetRange(CurrentPosition + 1, 1).First() == '/')
            {
                CurrentPosition++;
                CommentState = CommentState.None;
                return;
            }
            if (CommentState is not CommentState.None) return;
            if (item is '/')
            {
                CommentState = (item + Raw[CurrentPosition + 1].ToString()) switch
                {
                    "//" => CommentState.Line,
                    "/*" => CommentState.Block,
                    _ => CommentState.None
                };
                if (CommentState is not CommentState.None) return;
            }

            switch (CurrentState)
            {
                case State.Int:
                    if (char.IsDigit(item))
                    {
                        _ = Buffer.Append(item);
                        return;
                    }

                    if (item is '.')
                    {
                        _ = Buffer.Append(item);
                        CurrentState = State.Double;
                        return;
                    }

                    if (ClearBuffer() is Token possibleToken) tokens.Add(possibleToken);
                    CurrentState = State.Start;
                    break;
                case State.Text:
                    _ = Buffer.Append(item);
                    return;
                case State.String:
                    _ = Buffer.Append(item);

                    switch (item)
                    {
                        case '\n': // new line
                            Row++;
                            Column = 0;
                            break;
                        case '"': // string end
                            CheckForValidToken(tokens);
                            CurrentState = State.Start;
                            break;
                        default:
                            break;
                    }

                    return;
                case State.Double:
                    if (char.IsDigit(item))
                    {
                        _ = Buffer.Append(item);
                        return;
                    }

                    if (ClearBuffer() is Token possibleNextToken) tokens.Add(possibleNextToken);
                    CurrentState = State.Start;
                    break;
                case State.Char:
                    break;
                case State.Start:
                    break;
                default:
                    break;
            }

            if (item is '\'')
            {
                var possibleChar = new string(Raw.GetRange(CurrentPosition + 1, 2).ToArray());

                _ = Buffer.Append(item).Append(possibleChar);

                CurrentState = State.Char;

                if (possibleChar[1] is not '\'') throw ExceptionCreator.NotAChar(new(TokenType.Char, item.ToString() + possibleChar, Row, Column, FileName));

                if (ClearBuffer() is Token possibleToken) tokens.Add(possibleToken);

                CurrentPosition += 2;
                CurrentState = State.Start;
                return;
            }

            if (char.IsWhiteSpace(item))
            {
                if (ClearBuffer() is Token possibleToken) tokens.Add(possibleToken);
                return;
            }

            var tokenList = CheckForOperator(item).ToList();
            if (tokenList.Count > 0)
            {
                foreach (var token in tokenList) if (token is not null) tokens.Add(token.Value);
                return;
            }

            if (item is 't' && item + new string(Raw.GetRange(CurrentPosition + 1, 3).ToArray()) is "true")
            {
                CurrentPosition += 3;
                Buffer = new();
                tokens.Add(new(TokenType.Boolean, true, Row, Column, FileName));
                return;
            }
            if (item is 'f' && item + new string(Raw.GetRange(CurrentPosition + 1, 4).ToArray()) is "false")
            {
                CurrentPosition += 4;
                Buffer = new();
                tokens.Add(new(TokenType.Boolean, false, Row, Column, FileName));
                return;
            }

            if (item is '"') CurrentState = State.String;
            else if (char.IsDigit(item)) CurrentState = State.Int;

            _ = Buffer.Append(item);
        });

        CheckForValidToken(tokens);

        return tokens;
    }

    private void EndOfLine()
    {
        Row++;
        Column = 0;
        if (CommentState is CommentState.Line) CommentState = CommentState.None;
    }

    private void CheckForValidToken(List<Token> tokenList)
    {
        if (ClearBuffer() is Token possibleToken) tokenList.Add(possibleToken);
    }
    private Token? ClearBuffer()
    {
        if (Buffer.Length is 0)
        {
            Buffer = new();
            CurrentState = State.Start;
            return null;
        }

        var token = new Token(TokenType.New, "", Row, Column - (uint)Buffer.Length, FileName);

        token = CurrentState switch
        {
            State.Int => token with { Type = TokenType.Integer, Value = Convert.ToInt32(Buffer.ToString()) },
            State.Double => token with { Type = TokenType.Double, Value = Convert.ToDouble(Buffer.ToString(), new CultureInfo("en-US")) },
            State.String => token with { Type = TokenType.String, Value = Buffer.ToString()[1..^1] },
            State.Char => token with { Type = TokenType.Char, Value = Buffer[1] },
            _ => TokenTypeTranslator.GetKeyword(Buffer.ToString()) is not null and TokenType type
                ? token with
                {
                    Type = type,
                    Value = Buffer.ToString() switch
                    {
                        "bool" => "Boolean",
                        "string" => "String",
                        "char" => "Char",
                        "int" => "Integer",
                        "double" => "Double",
                        _ => Buffer.ToString(),
                    }
                }
                : token with
                {
                    Type = TokenType.Identifier,
                    Value = Buffer.ToString()
                }
        };

        Buffer = new();
        CurrentState = State.Start;

        return token;
    }

    private IEnumerable<Token?> CheckForOperator(char character)
    {
        for (var i = TokenTypeTranslator.LONGEST_OPERATOR; i >= 0; i--)
        {
            var operatorString = character + new string(Raw.GetRange(CurrentPosition + 1, i).ToArray());
            var possibleToken = TokenTypeTranslator.GetOperator(operatorString);

            if (possibleToken is not null && !(possibleToken == TokenType.Accessor && CurrentState == State.Int))
            {
                yield return ClearBuffer();
                CurrentPosition += i;
                yield return new(possibleToken!.Value, operatorString, Row, Column, FileName!);
                yield break;
            }
        }
    }
}
