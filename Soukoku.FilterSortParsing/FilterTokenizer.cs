using System;
using System.Collections.Generic;
using System.Text;

namespace Soukoku.FilterSortParsing;

internal enum FilterTokenType
{
    Property,
    Operator,
    Value,
    LogicalOperator,
    LeftParenthesis,
    RightParenthesis,
    Comma,
    End
}

internal class FilterToken
{
    public FilterTokenType Type { get; }
    public string Value { get; }

    public FilterToken(FilterTokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => $"{Type}: {Value}";
}

internal ref struct FilterTokenizer
{
    private ReadOnlySpan<char> _span;
    private int _position;

    public static List<FilterToken> Tokenize(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return new List<FilterToken>();
        }

        var tokenizer = new FilterTokenizer(filter.AsSpan());
        return tokenizer.TokenizeInternal();
    }

    private FilterTokenizer(ReadOnlySpan<char> span)
    {
        _span = span;
        _position = 0;
    }

    private List<FilterToken> TokenizeInternal()
    {
        // Pre-allocate based on estimated tokens per input length
        var tokens = new List<FilterToken>(_span.Length / 4);

        while (_position < _span.Length)
        {
            SkipWhitespace();

            if (_position >= _span.Length)
            {
                break;
            }

            char current = _span[_position];

            // Check for parentheses
            if (current == '(')
            {
                tokens.Add(new FilterToken(FilterTokenType.LeftParenthesis, "("));
                _position++;
                continue;
            }

            if (current == ')')
            {
                tokens.Add(new FilterToken(FilterTokenType.RightParenthesis, ")"));
                _position++;
                continue;
            }

            // Check for comma
            if (current == ',')
            {
                tokens.Add(new FilterToken(FilterTokenType.Comma, ","));
                _position++;
                continue;
            }

            // Check for quoted string value
            if (current == '\'' || current == '"')
            {
                tokens.Add(new FilterToken(FilterTokenType.Value, ReadQuotedString()));
                continue;
            }

            // Read word
            string word = ReadWord();

            if (string.IsNullOrEmpty(word))
            {
                _position++; // Skip unknown character
                continue;
            }

            // Determine token type based on word (use case-insensitive checks)
            if (IsLogicalOperator(word))
            {
                tokens.Add(new FilterToken(FilterTokenType.LogicalOperator, word));
            }
            else if (IsOperator(word))
            {
                tokens.Add(new FilterToken(FilterTokenType.Operator, word));
            }
            else if (IsValue(word))
            {
                tokens.Add(new FilterToken(FilterTokenType.Value, word));
            }
            else
            {
                tokens.Add(new FilterToken(FilterTokenType.Property, word));
            }
        }

        return tokens;
    }

    private void SkipWhitespace()
    {
        while (_position < _span.Length && char.IsWhiteSpace(_span[_position]))
        {
            _position++;
        }
    }

    private string ReadWord()
    {
        int start = _position;

        while (_position < _span.Length)
        {
            char c = _span[_position];

            if (char.IsWhiteSpace(c) || c == '(' || c == ')' || c == ',')
            {
                break;
            }

            _position++;
        }

        if (_position > start)
        {
            return _span.Slice(start, _position - start).ToString();
        }

        return string.Empty;
    }

    private string ReadQuotedString()
    {
        char quote = _span[_position];
        _position++; // Skip opening quote
        int start = _position;
        
        // Fast path: scan for end quote or escape character
        while (_position < _span.Length)
        {
            char c = _span[_position];
            if (c == '\\')
            {
                // Has escapes, use StringBuilder path
                _position = start - 1; // Reset to position after opening quote
                return ReadQuotedStringWithEscapes(quote);
            }
            if (c == quote)
            {
                // Fast path: no escapes, just slice
                var result = _span.Slice(start, _position - start).ToString();
                _position++; // Skip closing quote
                return result;
            }
            _position++;
        }
        
        // Unterminated string, return what we have
        return _span.Slice(start, _position - start).ToString();
    }

    private string ReadQuotedStringWithEscapes(char quote)
    {
        _position++; // Skip opening quote (we're called with position at quote)
        
        var sb = new StringBuilder();
        bool escaped = false;

        while (_position < _span.Length)
        {
            char c = _span[_position];

            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                _position++;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                _position++;
                continue;
            }

            if (c == quote)
            {
                _position++; // Skip closing quote
                break;
            }

            sb.Append(c);
            _position++;
        }

        return sb.ToString();
    }

    private static bool IsLogicalOperator(string word)
    {
        return string.Equals(word, "and", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "or", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "not", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOperator(string word)
    {
        return string.Equals(word, "eq", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "ne", StringComparison.OrdinalIgnoreCase) || 
               string.Equals(word, "gt", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "ge", StringComparison.OrdinalIgnoreCase) || 
               string.Equals(word, "lt", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "le", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "contains", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "startswith", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(word, "endswith", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValue(string word)
    {
        // Check if it's a number, boolean, or null
        if (word.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            word.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            word.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check if it's a number
        return double.TryParse(word, out _);
    }
}
