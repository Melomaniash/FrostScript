﻿namespace FrostScript

type Expression =
    { DataType : DataType
      Type : ExpressionType }

and ExpressionType =
| IfExpression of Condition : Expression * True : Expression * False : Expression option
| BinaryExpression of Opporator : TokenType * Left : Expression * Right : Expression
| BlockExpression of Body : Expression list
| LiteralExpression of Value : obj
| IdentifierExpression of Id : string
| ValidationError of Token * Error : string
| BindExpression of Id : string * Value : Expression
| AssignExpression of Id : string * Value : Expression
| FunctionExpression of Paramater : Paramater * Body : Expression
| FrostFunction of Call : (IdentifierMap<Expression> -> obj -> obj * IdentifierMap<Expression>)
| CallExpression of Callee : Expression * Argument : Expression
| NativeFunction of Call : (obj -> obj)