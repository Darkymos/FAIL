using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("FAIL Test")]

namespace FAIL.Language_Integration;
internal class Tokenizer
{
    private readonly CultureInfo culture = new("en-US");

    public string Raw { get; }
    public List<Token?> Tokens { get; } = new();
    public string FileName { get; }
    public Dictionary<string, TokenType> Operators { get; } = new()
    {
        { "(", TokenType.LeftParenthesis },
        { ")", TokenType.RightParenthesis },
        { "+", TokenType.Operator },
        { "-", TokenType.Operator },
        { "*", TokenType.Operator },
        { "/", TokenType.Operator },
        { ";", TokenType.EndOfLine },

        { "WriteLine", TokenType.FunctionKeyWord },
    };

    private uint row = 1;
    private uint column = 0;

    public Tokenizer(string fileName)
    {
        FileName = fileName;

        using (var stream = File.OpenText(fileName))
        {
            Raw = stream.ReadToEnd();
        }

        Deseriaize();
    }

    private void Deseriaize()
    {
        StringBuilder buffer = new();

        foreach (var character in Raw)
        {
            buffer.Append(character);

            if (character == '\n')
            {
                row++;
                column = 0;
            }
            else if (character == ' ')
            {
                var token = CreateToken(buffer);
                if (token is not null) Tokens.Add(token);

                buffer = new();
            }
            else if (Operators.ContainsKey(character.ToString()))
            {
                var token = CreateToken(new(buffer.ToString()[..^1]));
                if (token is not null) Tokens.Add(token);

                token = CreateToken(new(buffer[^1].ToString()));
                if (token is not null) Tokens.Add(token);

                buffer = new();
            }
            else if (character == Raw.Last())
            {
                var token = CreateToken(buffer);
                if (token is not null) Tokens.Add(token);
            }
            else
            {
                column++;
            }
        }

        Tokens.Add(new(TokenType.EndOfText, "", row, column, FileName));
    }

    private Token? CreateToken(StringBuilder buffer)
    {
        var raw = buffer.ToString();
        StringBuilder internalBuffer = new();
        var state = State.Start;

        if (raw == " ") return null;

        foreach (var character in raw)
        {
            internalBuffer.Append(character);

            if (state == State.Start)
            {
                if (character == '"')
                {
                    state = State.String;
                    internalBuffer.Remove(0, 1);
                }
                else if (char.IsNumber(character))
                {
                    state = State.Int;
                }
                else if (Operators.ContainsKey(character.ToString()))
                {
                    return new(Operators[character.ToString()], character.ToString(), row, column, FileName);
                }
            }
            else if (state == State.String)
            {
                if (character == '"') return new(TokenType.String, internalBuffer.Remove(internalBuffer.Length - 1, 1).ToString(), row, column, FileName);
            }
            else if (state == State.Int)
            {
                if (character == '.')
                {
                    state = State.Float;
                    continue;
                }
                if (raw.Last() == character) return new(TokenType.Int, Convert.ToInt32(internalBuffer.ToString(), culture), row, column, FileName);
            }
            else if (state == State.Float)
            {
                if (raw.Last() == character) return new(TokenType.Float, Convert.ToSingle(internalBuffer.ToString(), culture), row, column, FileName);
            }
        }

        return null; 
    }
}
