namespace FAIL.LanguageIntegration;

public enum TokenType
{
	Integer,

	StrokeCalculation,
	DotCalculation,

	EndOfStatement,
	String,
	OpeningParenthesis,
	ClosingParenthesis,
	Identifier,
	Assignment,
	OpeningBracket,
	ClosingBracket,
	Separator,
	Boolean,
	TestOperator,
	SelfAssignment,
	IncrementalOperator,

	Var,
	Void,
	Return,
	If,
	Else,
	While,
	Break,
	Continue,
	For,
	DataType,
	Conversion,
	LogicalOperator,
	Char,
	Accessor,
	Class,
	New,
	WriteLine,
	Double,
}

internal static class TokenTypeTranslator
{
	public const int LONGEST_OPERATOR = 2; // just to avoid unnecessary looping in Tokenizer.CheckForOperator()

	public static TokenType? GetOperator(string raw) => raw switch
	{
		// IO
		"|>" => TokenType.WriteLine,

		// Operators
		"==" or "!=" or ">=" or "<=" or ">" or "<" => TokenType.TestOperator,
		"||" or "&&" or "!" => TokenType.LogicalOperator,
		"+=" or "-=" or "*=" or "/=" => TokenType.SelfAssignment,
		"++" or "--" => TokenType.IncrementalOperator,
		"+" or "-" => TokenType.StrokeCalculation,
		"*" or "/" => TokenType.DotCalculation,

		// Annotation Signs
		"," => TokenType.Separator,
		";" => TokenType.EndOfStatement,
		"=" => TokenType.Assignment,
		"." => TokenType.Accessor,

		// Delimiters
		"(" => TokenType.OpeningParenthesis,
		")" => TokenType.ClosingParenthesis,
		"{" => TokenType.OpeningBracket,
		"}" => TokenType.ClosingBracket,

		// Not found
		_ => null,
	};
	public static TokenType? GetKeyword(string raw) => raw switch
	{
		// Special types
		"var" => TokenType.Var,
		"void" => TokenType.Void,

		// Data types
		"object" or "int" or "double" or "string" or "char" or "bool" => TokenType.DataType,

		// OOP
		"class" => TokenType.Class,
		"new" => TokenType.New,

		// Decisions
		"return" => TokenType.Return,
		"if" => TokenType.If,
		"else" => TokenType.Else,

		// Loops
		"while" => TokenType.While,
		"break" => TokenType.Break,
		"continue" => TokenType.Continue,
		"for" => TokenType.For,

		// Conversions
		"as" => TokenType.Conversion,

		// Logical operators
		"or" or "and" or "not" => TokenType.LogicalOperator,

		// Not found
		_ => null,
	};
}
