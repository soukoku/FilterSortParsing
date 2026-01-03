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
        var tokens = new List<FilterToken>();

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

            // Determine token type based on word
            var wordLower = word.ToLowerInvariant();

            if (IsLogicalOperator(wordLower))
            {
                tokens.Add(new FilterToken(FilterTokenType.LogicalOperator, wordLower));
            }
            else if (IsOperator(wordLower))
            {
                tokens.Add(new FilterToken(FilterTokenType.Operator, wordLower));
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

            if (char.IsWhiteSpace(c) || c == '(' || c == ')')
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
        return word == "and" || word == "or" || word == "not";
    }

    private static bool IsOperator(string word)
    {
        return word == "eq" || word == "ne" || 
               word == "gt" || word == "ge" || 
               word == "lt" || word == "le" ||
               word == "contains" || word == "startswith" || word == "endswith";
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
