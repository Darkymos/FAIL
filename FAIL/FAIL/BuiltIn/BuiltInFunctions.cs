using FAIL.ElementTree;
using FAIL.LanguageIntegration;
using Object = FAIL.ElementTree.DataTypes.Object;
using static System.FormattableString;
using Type = FAIL.ElementTree.Type;
using FAIL.Metadata;

namespace FAIL.BuiltIn;
internal static class BuiltInFunctions
{
    public static Dictionary<string, (Func<CommandList, Object?> Function, List<ParameterInfo> Parameters, Type ReturnType)> Functions = new()
    {
        {
            "log", 
            (Log_Call,
            new()
            {
                new(new("Var"), false)
            },
            new("Void")) 
        },
        { 
            "input", 
            (Input_Call, 
            new() 
            {
                new(new("var"), false)
            },
            new(nameof(ElementTree.DataTypes.String))) 
        }
    };

    private static Object? Log_Call(CommandList arguments)
    {
        var result = arguments.Commands.Entries[0].Call()!.Value;

        Interpreter.Logger!.Log(result, LogLevel.Debug);
        Console.WriteLine(Invariant($"{result}"));

        return null;
    }

    private static ElementTree.DataTypes.String Input_Call(CommandList arguments)
    {
        var result = arguments.Commands.Entries[0].Call()!.Value;

        if (result is not null) Console.Write(result);
        return new ElementTree.DataTypes.String(Console.ReadLine()!);
    }
}
