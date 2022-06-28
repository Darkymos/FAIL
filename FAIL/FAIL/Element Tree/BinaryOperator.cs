using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal abstract class BinaryOperator : AST
{
    public AST? FirstParameter { get; init; }
    public AST? SecondParameter { get; init; }


    public BinaryOperator(AST? firstParameter, AST? secondParameter, Token? token = null) : base(token)
    {
        FirstParameter = firstParameter;
        SecondParameter = secondParameter;
    }


    public override dynamic Call() => Calculate(FirstParameter!.Call(), SecondParameter!.Call());
    public override string ToString() => $"{nameof(BinaryOperator)}";

    public abstract dynamic Calculate(dynamic firstParameter, dynamic secondParameter);
}
