namespace FAIL.ElementTree.Terms;
internal abstract class BinaryTerm : AST
{
    public AST? FirstValue { get; }
    public AST? SecondValue { get; }

    protected BinaryTerm(AST? firstValue, AST? secondValue)
    {
        FirstValue = firstValue;
        SecondValue = secondValue;
    }

    public override object? Call() => Call(FirstValue, SecondValue);

    protected abstract object? Call(AST? firstValue, AST? secondValue);
}
