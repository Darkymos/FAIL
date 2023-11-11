using FAIL.ElementTree;
using FAIL.Metadata;
using static System.FormattableString;
using Instance = FAIL.ElementTree.Instance;
using Type = FAIL.ElementTree.Type;

namespace FAIL.BuiltIn;
internal static class BuiltInFunctions
{
    public static Dictionary<string, (Func<CommandList, Instance?> Function, List<ParameterInfo> Parameters, Type ReturnType)> Functions = new()
    {
        {
            "|>",
            (Log_Call,
            new()
            {
                new(new("var"), false)
            },
            new("Void"))
        },
        {
            "input",
            (Input_Call,
            new()
            {
                new(new("var"), true)
            },
            new(nameof(DataTypes.String)))
        }
    };

    private static Instance? Log_Call(CommandList arguments)
    {
        var result = arguments.Commands.Entries[0].Call()!;

        //_ = Interpreter.Logger!.Log(result.Value, LogLevel.Debug);
        Console.WriteLine(Invariant($"{result.Value}"));

        return null;
    }

    private static Instance Input_Call(CommandList arguments)
    {
        var result = arguments.Commands.Entries.Any() ? arguments.Commands.Entries[0].Call()!.Value : null;

        if (result is not null) Console.Write(result);
        return new Instance(DataTypes.String.Type, Console.ReadLine()!);
    }
}
