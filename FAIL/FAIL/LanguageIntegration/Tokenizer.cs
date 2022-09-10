using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("FAIL Test")]
namespace FAIL.LanguageIntegration;

internal record struct Token(TokenType Type, dynamic Value, uint Row, uint Column, string FileName);

internal class Tokenizer : ITokenizer
{
    private string? FileName;
    private char[]? Raw;
    private uint Row = 1;
    private uint Column = 1;
    private int CurrentPosition;

    private StringBuilder Buffer = new();
    private State CurrentState = State.Start;
    private CommentState CommentState = CommentState.None;


    public List<Token> Call(string code, string fileName)
    {
        FileName = fileName;
        Raw = code.ToCharArray();

        var tokenList = new List<Token>();

        while (true)
        {
            var nullableCharacter = Read();
            Column++;

            // check for end of file
            if (nullableCharacter is null)
            {
                CheckForValidToken(tokenList);
                break;
            }

            var character = nullableCharacter.Value;

            // everything in a string is part of it
            if (CurrentState is State.String)
            {
                _ = Buffer.Append(character);

                switch (character)
                {
                    case '\n': // new line
                        Row++;
                        Column = 1;
                        break;
                    case '"': // string end
                        CheckForValidToken(tokenList);
                        break;
                    default:
                        break;
                }

                continue;
            }

            // end of line reached, doesn't mean the current statement's end though
            if (character is '\n')
            {
                EndOfLine();
                continue;
            }

            // contents in comments are skipped
            if (CommentState is CommentState.Block && character is '*' && character + LookAhead(1) is "*/")
            {
                _ = Read(1);
                CommentState = CommentState.None;
                continue;
            }
            if (CommentState is not CommentState.None) continue;

            // check for a comment assignment 
            if (character is '/')
            {
                CommentState = (character + LookAhead(1)) switch
                {
                    "//" => CommentState.Line,
                    "/*" => CommentState.Block,
                    _ => CommentState.None
                };
                if (CommentState is not CommentState.None) continue;
            }

            // check for char
            if (character is '\'')
            {
                var possibleChar = LookAhead(2);

                _ = Buffer.Append(character)
                          .Append(possibleChar);

                CurrentState = State.Char;

                if (possibleChar[1] is '\'')
                {
                    if (ClearBuffer() is Token possibleToken) tokenList.Add(possibleToken);

                    _ = Read(2);
                    CurrentState = State.Start;
                    continue;
                }
                else throw ExceptionCreator.NotAChar(new(TokenType.Char, character.ToString() + possibleChar, Row, Column, FileName));
            }

            // all whitespace are considered as an end of the current token
            if (char.IsWhiteSpace(character))
            {
                if (ClearBuffer() is Token possibleToken) tokenList.Add(possibleToken);
                continue;
            }

            // check for any kind of operator specified in the Operators dictionary
            var tokens = CheckForOperator(character).ToList();
            if (tokens.Count > 0)
            {
                foreach (var token in tokens) if (token is not null) tokenList.Add(token.Value);
                continue;
            }

            // booleans
            if (CurrentState is State.Start)
            {
                if (character is 't' && character + LookAhead(3) is "true")
                {
                    _ = Read(3);
                    Buffer = new();
                    tokenList.Add(new(TokenType.Boolean, true, Row, Column, FileName));
                    continue;
                }
                if (character is 'f' && character + LookAhead(4) is "false")
                {
                    _ = Read(4);
                    Buffer = new();
                    tokenList.Add(new(TokenType.Boolean, false, Row, Column, FileName));
                    continue;
                }
            }

            // state machine for different data types
            while (true)
            {
                if (CurrentState is State.Start)
                {
                    _ = Buffer.Append(character);

                    if (character is '"') CurrentState = State.String;
                    if (char.IsDigit(character))
                    {
                        CurrentState = State.Int;
                        break;
                    }

                    break;
                }
                if (CurrentState is State.Int)
                {
                    if (char.IsDigit(character))
                    {
                        _ = Buffer.Append(character);
                        break;
                    }

                    if (character is '.')
                    {
                        _ = Buffer.Append(character);
                        CurrentState = State.Double;
                        break;
                    }

                    if (ClearBuffer() is Token possibleToken) tokenList.Add(possibleToken);
                    CurrentState = State.Start;
                    continue;
                }
                if (CurrentState is State.Double)
                {
                    if (char.IsDigit(character))
                    {
                        _ = Buffer.Append(character);
                        break;
                    }

                    if (ClearBuffer() is Token possibleToken) tokenList.Add(possibleToken);
                    CurrentState = State.Start;
                    continue;
                }
                if (CurrentState is State.Text)
                {
                    _ = Buffer.Append(character);
                    break;
                }
            }
        }

        return tokenList;
    }

    private char? Read() => CurrentPosition >= Raw!.Length ? null : Raw[CurrentPosition++];
    private char[]? Read(int length)
    {
        if (CurrentPosition + length >= Raw!.Length) return null;
        
        CurrentPosition += length;
        return Raw[(CurrentPosition - length)..CurrentPosition];
    }

    private void CheckForValidToken(List<Token> tokenList)
    {
        if (ClearBuffer() is Token possibleToken) tokenList.Add(possibleToken);
    }
    private Token? ClearBuffer()
    {
        Token? token;

        if (Buffer.Length == 0) token = null;
        else if (CurrentState == State.Int) token = new(TokenType.Number, Convert.ToInt32(Buffer.ToString()), Row, Column - (uint)Buffer.Length, FileName!);
        else if (CurrentState == State.Double) token = new(TokenType.Number, Convert.ToDouble(Buffer.ToString(), new CultureInfo("en-US")), Row, Column - (uint)Buffer.Length, FileName!);
        else if (CurrentState == State.String) token = new(TokenType.String, Buffer.ToString()[1..^1], Row, Column - (uint)Buffer.Length, FileName!);
        else if (CurrentState == State.Char) token = new(TokenType.Char, Buffer[1], Row, Column - (uint)Buffer.Length, FileName!);
        else
        {
            var possibleKeyWord = TokenTypeTranslator.GetKeyWord(Buffer.ToString());

            if (possibleKeyWord is not null) token = Buffer.ToString() switch
            {
                "bool" => new(TokenType.DataType, "Boolean", Row, Column - (uint)Buffer.Length, FileName!),
                "string" => new(TokenType.DataType, "String", Row, Column - (uint)Buffer.Length, FileName!),
                "char" => new(TokenType.DataType, "Char", Row, Column - (uint)Buffer.Length, FileName!),
                "int" => new(TokenType.DataType, "Integer", Row, Column - (uint)Buffer.Length, FileName!),
                "double" => new(TokenType.DataType, "Double", Row, Column - (uint)Buffer.Length, FileName!),
                _ => new(possibleKeyWord!.Value, Buffer.ToString(), Row, Column - (uint)Buffer.Length, FileName!),
            };
            else token = new(TokenType.Identifier, Buffer.ToString(), Row, Column - (uint)Buffer.Length, FileName!);
        }

        Buffer = new();
        CurrentState = State.Start;

        return token;
    }
    private string LookAhead(int length)
    {
        var val = Read(length);
        CurrentPosition -= length;

        return val is null ? "" : new string(val);
    }

    private IEnumerable<Token?> CheckForOperator(char character)
    {
        for (var i = TokenTypeTranslator.LONGEST_OPERATOR; i >= 0; i--)
        {
            var operatorString = character + LookAhead(i);
            var possibleToken = TokenTypeTranslator.GetOperator(operatorString);

            if (possibleToken is not null && !(possibleToken == TokenType.Accessor && CurrentState == State.Int))
            {
                yield return ClearBuffer();
                _ = Read(i);
                yield return new(possibleToken!.Value, operatorString, Row, Column, FileName!);
                yield break;
            }
        }
    }

    private void EndOfLine()
    {
        Row++;
        Column = 0;
        if (CommentState is CommentState.Line) CommentState = CommentState.None;
    }
}
