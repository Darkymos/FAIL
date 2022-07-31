using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("FAIL Test")]
namespace FAIL.LanguageIntegration;

internal record struct Token(TokenType Type, dynamic Value, uint Row, uint Column, string FileName);

internal class Tokenizer : IEnumerable<Token>
{
    public string File { get; }
    public string FileName { get; }
    public static Dictionary<string, TokenType> Operators { get; } = new()
    {
        // Testing
        { "==", TokenType.TestOperator },
        { "!=", TokenType.TestOperator },
        { ">=", TokenType.TestOperator },
        { "<=", TokenType.TestOperator },
        { ">", TokenType.TestOperator },
        { "<", TokenType.TestOperator },

        // Self Assignments
        { "+=", TokenType.SelfAssignment },
        { "-=", TokenType.SelfAssignment },
        { "*=", TokenType.SelfAssignment },
        { "/=", TokenType.SelfAssignment },

        // Incremental Operators
        { "++", TokenType.IncrementalOperator },
        { "--", TokenType.IncrementalOperator },

        // Annotation Signs
        { ",", TokenType.Separator },
        { ";", TokenType.EndOfStatement },
        { "=", TokenType.Assignment },

        // Delimiters
        { "(", TokenType.OpeningParenthese },
        { ")", TokenType.ClosingParenthese },
        { "{", TokenType.OpeningBracket },
        { "}", TokenType.ClosingBracket },

        // Mathematical Operators
        { "+", TokenType.StrokeCalculation },
        { "-", TokenType.StrokeCalculation },
        { "*", TokenType.DotCalculation },
        { "/", TokenType.DotCalculation },
    };
    public static Dictionary<string, TokenType> KeyWords { get; } = new()
    {
        // Peripherals
        { "log", TokenType.Log },
        { "input", TokenType.Input },

        // Special types
        { "var", TokenType.Var },
        { "void", TokenType.Void },

        // Types
        { "object", TokenType.Object },
        { "int", TokenType.DataType },
        { "double", TokenType.DataType },
        { "string", TokenType.DataType },
        { "bool", TokenType.DataType },

        // Decissions
        { "return", TokenType.Return },
        { "if", TokenType.If },
        { "else", TokenType.Else },

        // Loops
        { "while", TokenType.While },
        { "break", TokenType.Break },
        { "continue", TokenType.Continue },
        { "for", TokenType.For },

        // Conversions
        { "as", TokenType.As },
    };

    private uint Row = 1;
    private uint Column = 0;

    private StringBuilder Buffer = new();
    private State CurrentState = State.Start;
    private bool isCommented = false;

    private readonly char[] Raw;
    private int CurrentPosition = 0;


    public Tokenizer(string file, string fileName)
    {
        File = file;
        FileName = fileName;

        Raw = file.ToCharArray();
    }


    public IEnumerator<Token> GetEnumerator()
    {
        while (true)
        {
            var nullableCharacter = Read();
            Column++;

            // check for end of file
            if (nullableCharacter is null)
            {
                var possibleToken = CheckForValidToken(true);
                if (possibleToken is not null) yield return possibleToken.Value;
                yield break;
            }
            var character = nullableCharacter.Value;

            // everything in a string is part of it
            if (CurrentState == State.String)
            {
                Buffer.Append(character);

                if (character == '\n')
                {
                    Row++;
                    Column = 0;
                }
                else if (character == '"')
                {
                    var possibleToken = CheckForValidToken();
                    if (possibleToken is not null) yield return possibleToken.Value;
                }

                continue;
            }

            // check for char (currently halted because of some issues)
            //if (character == '\'')
            //{
            //    var possibleChar = LookAhead(2);
            //    CurrentState = State.Char;

            //    if (possibleChar[1] == '\'')
            //    {
            //        var possibleToken = CheckForValidToken(false);
            //        if (possibleToken is not null) yield return possibleToken.Value;

            //        CurrentState = State.Start;
            //        continue;
            //    }
            //    else throw ExceptionCreator.NotAChar(character.ToString() + possibleChar, FileName);
            //}

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

            // booleans
            if (CurrentState == State.Start)
            {
                if (character == 't' && character + LookAhead(3) == "true")
                {
                    Read(3);
                    Buffer = new();
                    yield return new(TokenType.Boolean, true, Row, Column, FileName);
                    continue;
                }
                if (character == 'f' && character + LookAhead(4) == "false")
                {
                    Read(4);
                    Buffer = new();
                    yield return new(TokenType.Boolean, false, Row, Column, FileName);
                    continue;
                } 
            }

            // state maschine for different data types
            while (true)
            {
                if (CurrentState == State.Start)
                {
                    Buffer.Append(character);

                    if (character == '"')
                    {
                        CurrentState = State.String;
                    }

                    if (char.IsDigit(character))
                    {
                        CurrentState = State.Int;
                        break;
                    }

                    break;
                }
                if (CurrentState == State.Int)
                {
                    if (char.IsDigit(character))
                    {
                        Buffer.Append(character);
                        break;
                    }

                    if (character == '.')
                    {
                        Buffer.Append(character);
                        CurrentState = State.Double;
                        break;
                    }

                    var possibleToken = CheckForValidToken(false);
                    if (possibleToken is not null) yield return possibleToken.Value;
                    CurrentState = State.Start;
                    continue;
                }
                if (CurrentState == State.Double)
                {
                    if (char.IsDigit(character))
                    {
                        Buffer.Append(character);
                        break;
                    }

                    var possibleToken = CheckForValidToken(false);
                    if (possibleToken is not null) yield return possibleToken.Value;
                    CurrentState = State.Start;
                    continue;
                }
                if (CurrentState == State.Text)
                {
                    Buffer.Append(character);
                    break;
                }
            }
        }
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private char? Read() => CurrentPosition >= Raw.Length ? null : Raw[CurrentPosition++];
    private char[]? Read(int length)
    {
        if (CurrentPosition + length >= Raw.Length) return null;

        var val = Raw[new Range(CurrentPosition, CurrentPosition + length)];
        CurrentPosition += length;
        return val;
    }

    private Token? ClearBuffer()
    {
        Token? token;

        if (Buffer.Length == 0) token = null;
        else if (CurrentState == State.Int) token = new(TokenType.Number, Convert.ToInt32(Buffer.ToString()), Row, Column - (uint)Buffer.Length, FileName);
        else if (CurrentState == State.Double) token = new(TokenType.Number, Convert.ToDouble(Buffer.ToString(), new CultureInfo("en-US")), Row, Column - (uint)Buffer.Length, FileName);
        else if (CurrentState == State.String) token = new(TokenType.String, Buffer.ToString()[1..^1], Row, Column - (uint)Buffer.Length, FileName);
        else if (CurrentState == State.Char) token = new(TokenType.String, Buffer[1], Row, Column - (uint)Buffer.Length, FileName);
        else
        {
            if (KeyWords.ContainsKey(Buffer.ToString()))
            {
                if (Buffer.ToString() == "bool") token = new(TokenType.DataType, "Boolean", Row, Column - (uint)Buffer.Length, FileName);
                else if (Buffer.ToString() == "string") token = new(TokenType.DataType, "String", Row, Column - (uint)Buffer.Length, FileName);
                else if (Buffer.ToString() == "int") token = new(TokenType.DataType, "Integer", Row, Column - (uint)Buffer.Length, FileName);
                else if (Buffer.ToString() == "double") token = new(TokenType.DataType, "Double", Row, Column - (uint)Buffer.Length, FileName);
                else token = new(KeyWords[Buffer.ToString()], Buffer.ToString(), Row, Column - (uint)Buffer.Length, FileName); 
            }
            else token = new(TokenType.Identifier, Buffer.ToString(), Row, Column - (uint)Buffer.Length, FileName);
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
                yield return new(Operators[op], op, Row, Column, FileName);
                yield break;
            }

            if (character == op[0])
            {
                var operatorString = character + LookAhead(op.Length - 1);
                if (operatorString == op)
                {
                    yield return ClearBuffer();
                    Read(op.Length - 1);
                    yield return new(Operators[op], op, Row, Column, FileName);
                    yield break;
                }

            }
        }
    }

    private void EndOfLine()
    {
        Row++;
        Column = 0;
        isCommented = false;
    }
}
