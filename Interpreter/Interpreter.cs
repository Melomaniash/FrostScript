﻿using FrostScript.Expressions;
using FrostScript.Statements;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class Interpreter
    {
        public static Result ExecuteProgram(IEnumerable<Statement> statements, Dictionary<string, object> variables = null)
        {
            if (variables is null)
                variables = new Dictionary<string, object>();

            try
            {
                if (statements.Last() is ExpressionStatement)
                {
                    var result = statements.Last() as ExpressionStatement;

                    foreach (var statement in statements.SkipLast(1))
                        ExecuteStatement(statement);

                    return Result.Pass(ExecuteExpression(result.Expression));
                }
                else
                {
                    foreach (var statement in statements)
                        ExecuteStatement(statement);

                    return Result.Pass();
                }
            }
            catch (InterpretException exception)
            {
                Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
                return Result.Fail();
            }


            void ExecuteStatement(Statement statement)
            {
                switch (statement)
                {
                    case Print print:
                        Console.WriteLine(ExecuteExpression(print.Expression));
                        break;

                    case Bind(var id, var value):
                        variables[id] = ExecuteExpression(value);
                        break;

                    case Assign(var id, var value):
                        variables[id] = ExecuteExpression(value);
                        break;


                    case ExpressionStatement exprStatement: break;
                }
            }

            object ExecuteExpression(Expression expression)
            {
                switch (expression)
                {
                    case Literal literal: return literal.Value;
                    case Identifier identifier: return variables[identifier.Id];

                    case Unary unary:

                        return unary.Expression.Type switch
                        {
                            DataType.Numeral => unary.Operator.Type switch
                            {
                                TokenType.Minus => -(double)ExecuteExpression(unary.Expression),
                                TokenType.Plus => ExecuteExpression(unary.Expression),
                                _ => throw new InterpretException($"Oporator {unary.Operator.Lexeme} not supported for type Numeric")
                            },
                            DataType.Bool => unary.Operator.Type switch
                            {
                                TokenType.Not => !(bool)ExecuteExpression(unary.Expression),
                                _ => throw new InterpretException($"Oporator {unary.Operator.Lexeme} not supported for type Bool")
                            },
                            _ => throw new InterpretException($"Oporator {unary.Operator.Lexeme} not supported for type {unary.Expression.Type}")

                        };

                    case Binary binary:

                        var leftResult = ExecuteExpression(binary.Left);
                        var rightResult = ExecuteExpression(binary.Right);

                        return binary.Operator.Type switch
                        {
                            //strings
                            _ when leftResult is string leftString && rightResult is string rightString =>
                                 binary.Operator.Type switch
                                 {
                                     TokenType.Plus => leftString + rightString,

                                     TokenType.Equal => leftString == rightString,
                                     TokenType.NotEqual => leftString != rightString,

                                     _ => throw new InterpretException(
                                         binary.Operator.Line,
                                         binary.Operator.Character,
                                         $"Oporator \"{binary.Operator.Lexeme}\" is not valid between strings")
                                 },

                            //numerals
                            _ when leftResult is double leftDouble && rightResult is double rightDouble =>
                                 binary.Operator.Type switch
                                 {
                                     TokenType.Plus => leftDouble + rightDouble,
                                     TokenType.Minus => leftDouble - rightDouble,
                                     TokenType.Star => leftDouble * rightDouble,
                                     TokenType.Slash => leftDouble / rightDouble,
                                     TokenType.GreaterThen => leftDouble > rightDouble,
                                     TokenType.GreaterOrEqual => leftDouble >= rightDouble,
                                     TokenType.LessThen => leftDouble < rightDouble,
                                     TokenType.LessOrEqual => leftDouble <= rightDouble,

                                     TokenType.Equal => leftDouble == rightDouble,
                                     TokenType.NotEqual => leftDouble != rightDouble,

                                     _ => throw new InterpretException(
                                        binary.Operator.Line,
                                        binary.Operator.Character,
                                        $"Oporator {binary.Operator.Type} is not valid between numerals")
                                 },

                            // bools
                            _ when leftResult is bool leftBool && rightResult is bool rightBool =>
                            binary.Operator.Type switch
                            {
                                TokenType.Equal => leftBool == rightBool,
                                TokenType.NotEqual => leftBool != rightBool,

                                _ => throw new InterpretException(
                                        binary.Operator.Line,
                                        binary.Operator.Character,
                                        $"Oporator {binary.Operator.Type} is not valid between bools")
                            },

                            TokenType.Equal => ExecuteExpression(binary.Left) == ExecuteExpression(binary.Right),
                            TokenType.NotEqual => ExecuteExpression(binary.Left) != ExecuteExpression(binary.Right),

                            _ => throw new InterpretException(
                                        binary.Operator.Line,
                                        binary.Operator.Character,
                                        $"Oporator {binary.Operator.Type} is not valid between types {leftResult.GetType()} and {rightResult.GetType()}")
                        };

                    case When whenExpr:

                        return ExecuteWhen(whenExpr);

                        object ExecuteWhen(When when)
                        {
                            var ifExpression = when.IfExpresion != null ? ExecuteExpression(when.IfExpresion) : null;

                            return ifExpression switch
                            {
                                null => ExecuteExpression(when.ResultExpression),
                                bool boolResult => boolResult ? ExecuteExpression(when.ResultExpression) : ExecuteWhen(when.ElseWhen),
                            };
                        }

                    case ExpressionBlock expressionBlock:
                        return (ExecuteProgram(expressionBlock.Statements, new(variables)) as Pass<object>).Value;

                    case null: throw new InterpretException("Errors must be addressed before interpretation can commence");

                    default: throw new NotImplementedException();
                };
            }
        }
    }
}
