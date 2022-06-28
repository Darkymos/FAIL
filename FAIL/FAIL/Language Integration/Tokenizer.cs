using FAIL.Exceptions;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("FAIL Test")]
namespace FAIL.Language_Integration;

internal record struct Token(TokenType Type, dynamic Value, uint Row, uint Column, string FileName);

internal class Tokenizer : IEnumerable<Token>
{
    public string File { get; }
    public static Dictionary<string, TokenType> Operators { get; } = new()
    {
        { "+", TokenType.StrokeCalculation },
        { "-", TokenType.StrokeCalculation },
        { "*", TokenType.DotCalculation },
        { "/", TokenType.DotCalculation },
        { ";", TokenType.EndOfStatement },
    };

    private uint row = 1;
    private uint column = 0;

    private StringBuilder buffer = new();
    private State currentState = State.Start;
    private bool isCommented = false;

    private readonly char[] Raw;
    private int currentPosition = 0;


    public Tokenizer(string file)
    {
        File = file;
        Raw = file.ToCharArray();
    }


    public IEnumerator<Token> GetEnumerator()
    {
        while (true)
        {
            var nullableCharacter = Read();
            column++;

            // check for end of file
            if (nullableCharacter is null)
            {
                var possibleToken = CheckForValidToken();
                if (possibleToken is not null) yield return possibleToken.Value;
                yield break;
            }
            var character = nullableCharacter.Value;

            // end of line reached, doesn't mean the current statement's end though
            if (character == '\n')
            {
                EndOfLine();
                continue;
            }

            // contents in comments are skipped
            if (isCommented) continue;

            // check for a comment assignment 
            if (character == '/' && character + LookAhead(1) == "//")
            {
                isCommented = true;
                continue;
            }

            // all whitespace are considered as an end of the current token
            if (char.IsWhiteSpace(character))
            {
                var possibleToken = CheckForValidToken(false);
                if (possibleToken is not null) yield return possibleToken.Value;
                continue;
            }

            // check for any kind of operator specified in the Operators dictionary
            var tokens = CheckForOperator(character).ToList();
            if (tokens.Count > 0)
            {
                foreach (var token in tokens) if (token is not null) yield return token.Value;
                continue;
            }

            while (true)
            {
                if (currentState == State.Start)
                {
                    buffer.Append(character);

                    if (char.IsDigit(character))
                    {
                        currentState = State.Int;
                        break;
                    }

                    break;
                }
                if (currentState == State.Int)
                {
                    if (char.IsDigit(character))
                    {
                        buffer.Append(character);
                        break;
                    }

                    var possibleToken = CheckForValidToken(false);
                    if (possibleToken is not null) yield return possibleToken.Value;
                    currentState = State.Start;
                    continue;
                }
                if (currentState == State.Text)
                {
                    buffer.Append(character);
                    break;
                }
            }
        }
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private char? Read() => currentPosition >= Raw.Length ? null : Raw[currentPosition++];
    private char[]? Read(int length)
    {
        if (currentPosition + length >= Raw.Length) return null;

        var val = Raw[currentPosition..(currentPosition + length)];
        currentPosition += length;
        return val;
    }

    private Token? ClearBuffer()
    {
        Token? token;

        if (buffer.Length == 0) token = null;
        else if (currentState == State.Int) token = new(TokenType.Number, Convert.ToInt32(buffer.ToString()), row, column, File);
        else token = null;

        buffer = new();
        currentState = State.Start;

        return token;
    }
    private string LookAhead(int length)
    {
        var val = Read(length);
        currentPosition -= length;

        return val is null ? "" : val.ToString()!;
    }

    private Token? CheckForValidToken(bool throwException = true)
    {
        var possibleToken = ClearBuffer();
        if (possibleToken is not null) return possibleToken;
        else if (throwException) throw ExceptionCreator.IterationEnded();
        return null;
    }
    private IEnumerable<Token?> CheckForOperator(char character)
    {
        foreach (var op in Operators.Keys)
        {
            if (character.ToString() == op)
            {
                yield return ClearBuffer();
                yield return new(Operators[op], op, row, column, File);
                yield break;
            }

            if (character == op[0])
            {
                var operatorString = character + LookAhead(op.Length - 1);
                if (operatorString == op)
                {
                    yield return ClearBuffer();
                    Read(op.Length - 1);
                    yield return new(Operators[op], operatorString, row, column, File);
                    yield break;
                }

            }
        }
    }

    private void EndOfLine()
    {
        row++;
        column = 0;
        isCommented = false;
    }
}
