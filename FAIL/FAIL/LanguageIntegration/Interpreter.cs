using FAIL.ElementTree;
using System.Text;

namespace FAIL.LanguageIntegration;
internal class Interpreter
{
    private readonly string[] FileNames;
    private readonly AST? AST;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly dynamic? Result;
#pragma warning restore IDE0052 // Remove unread private members

    public static Logger? Logger { get; private set; }


    public Interpreter(LogLevel level, params string[] fileNames)
    {
        FileNames = fileNames;
        Logger = new(new(File.Create(Directory.GetCurrentDirectory().Split(@"\bin")[0] + @$"\log.txt"), Encoding.Unicode), level);

        var code = new StringBuilder();

        foreach (var fileName in FileNames) _ = code.Append(File.OpenText(fileName).ReadToEnd());

        AST = new Parser(code.ToString(), fileNames[^1]).Parse();
        Result = AST?.Call();

        _ = Logger.Log(AST!, LogLevel.Info);
    }
}
