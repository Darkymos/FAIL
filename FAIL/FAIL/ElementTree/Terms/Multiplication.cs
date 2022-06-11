namespace FAIL.ElementTree.Terms;
internal class Multiplication : BinaryTerm
{
    public Multiplication(AST? firstValue, AST? secondValue) : base(firstValue, secondValue)
    {
    }

    protected override object? Call(AST? firstValue, AST? secondValue)
    {
        return firstValue!.Token!.Value * secondValue!.Token!.Value;
    }
}
