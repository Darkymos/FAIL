using FAIL.ElementTree;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class CommandParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly IServiceProvider ServiceProvider;
    private readonly CommandListParser CommandListParser;


    public CommandParser(TokenReader reader, IServiceProvider serviceProvider, CommandListParser commandListParser)
    {
        Reader = reader;
        ServiceProvider = serviceProvider;
        CommandListParser = commandListParser;
    }


    public AST Parse(Scope scope, TokenType? endOfStatementSign = null, TokenType? endOfBlockSign = null)
    {
        var isBlock = false; // kinda redundant, but still there, until a better solution is found

        var result = Reader.CurrentToken!.Value.Type switch
        {
            TokenType.Var or TokenType.Void or TokenType.DataType => ServiceProvider.GetRequiredService<TypeParser>().Parse(scope, out isBlock),
            TokenType.Class => ServiceProvider.GetRequiredService<ClassParser>().Parse(scope, out isBlock),
            TokenType.New => ServiceProvider.GetRequiredService<TypeInitializationParser>().Parse(scope),
            TokenType.If or TokenType.While or TokenType.For => ParseBlockStatement(scope, out isBlock),
            TokenType.Return => ParseSimpleStatement(scope, endOfStatementSign),
            TokenType.Continue or TokenType.Break => ParseSimpleKeyword(),
            _ => ServiceProvider.GetRequiredService<ArithmeticOperationParser>().Parse(scope)
        };

        if (endOfStatementSign is not null // command must have a endOfStatementSign
            && !isBlock
            && (endOfBlockSign is null || !Reader.IsTypeOf(endOfBlockSign.Value))) // is ther an endOfBlockSign (like TokenType.ClosingParenthese, see e.g. 'testCommand' of an if)?
            _ = Reader.ConsumeCurrentToken(endOfStatementSign!.Value);

        return result;
    }

    private AST ParseSimpleKeyword()
    {
        var result = (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{Reader.CurrentToken!.Value.Type}")!,
                                               Reader.CurrentToken) as AST)!;
        _ = Reader.ConsumeCurrentToken();
        return result;
    }
    private AST ParseSimpleStatement(Scope scope, TokenType? endOfStatementSign)
    {
        var token2 = Reader.CurrentToken!.Value;
        _ = Reader.ConsumeCurrentToken();
        return (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{token2.Type}")!,
                                         Parse(scope, endOfStatementSign), token2) as AST)!;
    }
    private AST ParseBlockStatement(Scope scope, out bool isBlock)
    {
        isBlock = true;
        var token3 = Reader.CurrentToken!.Value;
        _ = Reader.ConsumeCurrentToken();
        _ = Reader.ConsumeCurrentToken(TokenType.OpeningParenthese);
        return (GetType().GetMethod($"Parse{token3.Type}", BindingFlags.NonPublic | BindingFlags.Instance)!
                         .Invoke(this, new object[] { scope, token3 }) as AST)!;
    }

    private AST ParseIf(Scope scope, Token token)
    {
        var testCommand = Parse(scope, TokenType.ClosingParenthese);

        var ifBody = CommandListParser.Parse(scope);

        if (!Reader.IsEOT() && Reader.IsTypeOf(TokenType.Else))
        {
            _ = Reader.ConsumeCurrentToken();

            return new If(testCommand!,
                          ifBody,
                          Reader.IsTypeOf(TokenType.If)
                            ? Parse(scope) // else if
                            : CommandListParser.Parse(scope), // else
                          token);
        }

        // no else
        return new If(testCommand!, ifBody, null, token);
    }
    private AST ParseWhile(Scope scope, Token token)
    {
        var testCommand = Parse(scope, TokenType.ClosingParenthese);

        return new While(testCommand!, CommandListParser.Parse(scope), token);
    }
    private AST ParseFor(Scope scope, Token token)
    {
        var internalScope = new Scope(); // special scope for the iterator variable
        var iteratorVariable = Parse(internalScope, TokenType.EndOfStatement); // var i = 0;
        internalScope.Add(iteratorVariable);
        var iteratorTest = Parse(internalScope, TokenType.EndOfStatement); // i < length;
        var iteratorAction = Parse(internalScope, TokenType.ClosingParenthese); // i++

        return new For(iteratorVariable, iteratorTest, iteratorAction, CommandListParser.Parse(internalScope, scope), token);
    }
}
