using FAIL.ElementTree;

namespace FAIL.LanguageIntegration;
internal interface ICompilerComponent
{
    AST Call(AST ast);
}
