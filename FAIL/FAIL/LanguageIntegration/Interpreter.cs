using System.Text;

namespace FAIL.LanguageIntegration;
internal class Interpreter
{
    private CompilerPipeline? Pipeline;

    public static Logger? Logger { get; private set; }


    public Interpreter(LogLevel level, string fileName)
    {
        Logger = new(new(File.Create(Directory.GetCurrentDirectory().Split(@"\bin")[0] + @$"\log.txt"), Encoding.Unicode), level);

        var code = File.OpenText(fileName).ReadToEnd();

        BuildPipeline();
        _ = Pipeline!.Call(code, fileName);

        //_ = Logger.Log(AST!, LogLevel.Info);
    }

    private void BuildPipeline()
    {
        Pipeline = new(new Tokenizer(), new Parser());

        _ = Pipeline.AddComponent<TypeChecker>();
    }
}
