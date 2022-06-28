using FAIL.Element_Tree;
using System.Text;

namespace FAIL.Language_Integration;
internal class Interpreter
{
    private readonly string[] FileNames;
    private readonly AST? AST;
    private readonly dynamic? Result;

    public static Logger? Logger { get; private set; }


    public Interpreter(LogLevel level, params string[] fileNames)
    {
        FileNames = fileNames;
        Logger = new(level, new StreamWriter(File.Create(Directory.GetCurrentDirectory().Split(@"\bin")[0] + @"\log.txt")));

        var code = new StringBuilder();

        foreach (var fileName in FileNames)
        {
            code.Append(File.OpenText(fileName).ReadToEnd());
        }

        AST = new Parser(code.ToString()).Parse();
        Result = AST?.Call();

        Logger.Log(AST!, LogLevel.Info);
    }
}
