﻿namespace FAIL.LanguageIntegration;
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

	internal static Calculations Above(this Calculations calculations) => (Calculations)(((int)calculations) - 1);
	internal static Calculations SelfAndAbove(this Calculations calculations) => (Calculations)((((int)calculations) * 2) - 1);
	internal static Calculations Below(this Calculations calculations) => (Calculations)(((int)All) - ((((int)calculations) * 2) - 1));
	internal static Calculations SelfAndBelow(this Calculations calculations) => (Calculations)(((int)All) - (((int)calculations) - 1));

	public static TokenType GetOperationTokenType(this Calculations calculation) => calculation switch
	{
		Calculations.DotCalculations => TokenType.DotCalculation,
		Calculations.StrokeCalculations => TokenType.StrokeCalculation,
		Calculations.TestOperations => TokenType.TestOperator,
		Calculations.LogicalOperations => TokenType.LogicalOperator,
		Calculations.Conversions => TokenType.Conversion,
		_ => throw new NotSupportedException()
	};
}
