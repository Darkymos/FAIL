﻿using FAIL.Language_Integration;

namespace FAIL.Element_Tree.Operators;
internal class NotEquality : BinaryOperator
{
    public NotEquality(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter) => firstParameter != secondParameter;
}
