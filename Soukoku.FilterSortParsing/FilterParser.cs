using System;
using System.Collections.Generic;

namespace Soukoku.FilterSortParsing;

internal enum FilterExpressionType
{
    Comparison,
    Logical,
    Not,
    Function
}

internal abstract class FilterExpression
{
    public FilterExpressionType Type { get; protected set; }
}

internal class ComparisonExpression : FilterExpression
{
    public string PropertyName { get; }
    public string Operator { get; }
    public string Value { get; }

    public ComparisonExpression(string propertyName, string operatorName, string value)
    {
        Type = FilterExpressionType.Comparison;
        PropertyName = propertyName;
        Operator = operatorName;
        Value = value;
    }

    public override string ToString() => $"{PropertyName} {Operator} {Value}";
}

internal class LogicalExpression : FilterExpression
{
    public FilterExpression Left { get; }
    public string Operator { get; }
    public FilterExpression Right { get; }

    public LogicalExpression(FilterExpression left, string operatorName, FilterExpression right)
    {
        Type = FilterExpressionType.Logical;
        Left = left;
        Operator = operatorName;
        Right = right;
    }

    public override string ToString() => $"({Left} {Operator} {Right})";
}

internal class NotExpression : FilterExpression
{
    public FilterExpression Inner { get; }

    public NotExpression(FilterExpression inner)
    {
        Type = FilterExpressionType.Not;
        Inner = inner;
    }

    public override string ToString() => $"not ({Inner})";
}

internal class FunctionExpression : FilterExpression
{
    public string FunctionName { get; }
    public List<string> Arguments { get; }

    public FunctionExpression(string functionName, List<string> arguments)
    {
        Type = FilterExpressionType.Function;
        FunctionName = functionName;
        Arguments = arguments;
    }

    public override string ToString() => $"{FunctionName}({string.Join(", ", Arguments)})";
}

internal class FilterParser
{
    private List<FilterToken> _tokens;
    private int _position;

    public static FilterExpression? Parse(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        var tokens = FilterTokenizer.Tokenize(filter!);
        if (tokens.Count == 0)
        {
            return null;
        }

        var parser = new FilterParser(tokens);
        return parser.ParseExpression();
    }

    private FilterParser(List<FilterToken> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    private FilterToken? CurrentToken => _position < _tokens.Count ? _tokens[_position] : null;

    private FilterToken Consume()
    {
        if (_position >= _tokens.Count)
        {
            throw new InvalidOperationException("Unexpected end of filter expression.");
        }
        return _tokens[_position++];
    }

    private FilterExpression ParseExpression()
    {
        return ParseLogicalOr();
    }

    private FilterExpression ParseLogicalOr()
    {
        var left = ParseLogicalAnd();

        while (CurrentToken?.Type == FilterTokenType.LogicalOperator &&
               string.Equals(CurrentToken.Value, "or", StringComparison.OrdinalIgnoreCase))
        {
            Consume(); // Consume 'or'
            var right = ParseLogicalAnd();
            left = new LogicalExpression(left, "or", right);
        }

        return left;
    }

    private FilterExpression ParseLogicalAnd()
    {
        var left = ParseUnary();

        while (CurrentToken?.Type == FilterTokenType.LogicalOperator &&
               string.Equals(CurrentToken.Value, "and", StringComparison.OrdinalIgnoreCase))
        {
            Consume(); // Consume 'and'
            var right = ParseUnary();
            left = new LogicalExpression(left, "and", right);
        }

        return left;
    }

    private FilterExpression ParseUnary()
    {
        if (CurrentToken?.Type == FilterTokenType.LogicalOperator &&
            string.Equals(CurrentToken.Value, "not", StringComparison.OrdinalIgnoreCase))
        {
            Consume(); // Consume 'not'
            var inner = ParsePrimary();
            return new NotExpression(inner);
        }

        return ParsePrimary();
    }

    private FilterExpression ParsePrimary()
    {
        // Handle parentheses for grouping
        if (CurrentToken?.Type == FilterTokenType.LeftParenthesis)
        {
            Consume(); // Consume '('
            var expr = ParseExpression();

            if (CurrentToken?.Type != FilterTokenType.RightParenthesis)
            {
                throw new InvalidOperationException("Expected closing parenthesis ')'.");
            }
            Consume(); // Consume ')'
            return expr;
        }

        // Check if this is a function call: functionName(arg1, arg2, ...)
        // Function names can be either Operator tokens (contains, startswith, endswith) or Property tokens
        if ((CurrentToken?.Type == FilterTokenType.Operator || CurrentToken?.Type == FilterTokenType.Property) && 
            _position + 1 < _tokens.Count && 
            _tokens[_position + 1].Type == FilterTokenType.LeftParenthesis)
        {
            return ParseFunctionCall();
        }

        // Parse comparison: property operator value
        return ParseComparison();
    }

    private FilterExpression ParseComparison()
    {
        if (CurrentToken?.Type != FilterTokenType.Property)
        {
            throw new InvalidOperationException($"Expected property name, but got: {CurrentToken?.Value ?? "end of expression"}");
        }

        string property = Consume().Value;

        if (CurrentToken?.Type != FilterTokenType.Operator)
        {
            throw new InvalidOperationException($"Expected operator after property '{property}', but got: {CurrentToken?.Value ?? "end of expression"}");
        }

        string operatorName = Consume().Value;
        // normalize operator to lower-case so downstream switch comparisons are case-sensitive
        operatorName = operatorName.ToLowerInvariant();

        if (CurrentToken?.Type != FilterTokenType.Value && CurrentToken?.Type != FilterTokenType.Property)
        {
            throw new InvalidOperationException($"Expected value after operator '{operatorName}', but got: {CurrentToken?.Value ?? "end of expression"}");
        }

        string value = Consume().Value;

        return new ComparisonExpression(property, operatorName, value);
    }

    private FilterExpression ParseFunctionCall()
    {
        // Consume function name
        string functionName = Consume().Value.ToLowerInvariant();

        // Consume '('
        if (CurrentToken?.Type != FilterTokenType.LeftParenthesis)
        {
            throw new InvalidOperationException($"Expected '(' after function name '{functionName}'.");
        }
        Consume();

        // Parse arguments
        var arguments = new List<string>();

        // Check for empty parameter list
        if (CurrentToken?.Type == FilterTokenType.RightParenthesis)
        {
            Consume(); // Consume ')'
            return new FunctionExpression(functionName, arguments);
        }

        // Parse first argument
        if (CurrentToken?.Type == FilterTokenType.Property || CurrentToken?.Type == FilterTokenType.Value)
        {
            arguments.Add(Consume().Value);
        }
        else
        {
            throw new InvalidOperationException($"Expected argument in function '{functionName}', but got: {CurrentToken?.Value ?? "end of expression"}");
        }

        // Parse remaining arguments
        while (CurrentToken?.Type == FilterTokenType.Comma)
        {
            Consume(); // Consume ','

            if (CurrentToken?.Type == FilterTokenType.Property || CurrentToken?.Type == FilterTokenType.Value)
            {
                arguments.Add(Consume().Value);
            }
            else
            {
                throw new InvalidOperationException($"Expected argument after comma in function '{functionName}', but got: {CurrentToken?.Value ?? "end of expression"}");
            }
        }

        // Consume ')'
        if (CurrentToken?.Type != FilterTokenType.RightParenthesis)
        {
            throw new InvalidOperationException($"Expected ')' after function arguments in '{functionName}'.");
        }
        Consume();

        return new FunctionExpression(functionName, arguments);
    }
}
