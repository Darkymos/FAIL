using FAIL.ElementTree;

namespace FAIL.LanguageIntegration;
internal class TypeChecker : ICompilerComponent
{
    public AST Call(AST ast) => ast!;

    public static AST CheckType(AST given, ElementTree.Type expected, string name, Token token)
        => (expected.GetType().Name == "Var" || given.GetType() == expected) && given.GetType() != new ElementTree.Type("Void")
            ? given
            : throw ExceptionCreator.InvalidType(name, given.GetType(), expected, token);
    public static bool CheckType(AST given, ElementTree.Type expected) => expected.GetType().Name == "var" || given.GetType() == expected;
}
