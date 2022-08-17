namespace FAIL.LanguageIntegration;
[Flags]
internal enum Calculations : int
{
    Term = 1,
    DotCalculations = 2,
    StrokeCalculations = 4,
    TestOperations = 8,
    LogicalOperations = 16,
    Conversions = 32,
}

internal static class CalculationsExtensions
{
    private static readonly int[] Values = (int[])Enum.GetValues(typeof(Calculations));
    internal static Calculations All => (Calculations)Values.Sum();

    internal static Calculations GetAbove(this Calculations calculations) => (Calculations)(((int)calculations) - 1);
    internal static Calculations GetSelfAndAbove(this Calculations calculations) => (Calculations)((((int)calculations) * 2) - 1);
    internal static Calculations GetBelow(this Calculations calculations) => (Calculations)(((int)All) - ((((int)calculations) * 2) - 1));
    internal static Calculations GetSelfAndBelow(this Calculations calculations) => (Calculations)(((int)All) - (((int)calculations) - 1));
}
