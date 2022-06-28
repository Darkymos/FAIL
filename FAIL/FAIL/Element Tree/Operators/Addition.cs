﻿using FAIL.Language_Integration;

namespace FAIL.Element_Tree.Operators;
internal class Addition : BinaryOperator
{
    public Addition(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }


    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter) => firstParameter + secondParameter;
}
