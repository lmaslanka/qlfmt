using System.Text;
using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private const int AsciiCaseBit = 'a' - 'A';
    private const string Indent = "    ";
    private const string DoubleColon = "::";
    private const string Concat = "||";
    private const string JsonArrow = "->";
    private const string JsonTextArrow = "->>";
    private const char IdentifierQuote = '"';
    private const char Ampersand = '&';
    private const char CharsetIntroducer = '_';
    private const int UnicodeDelimitedPrefixLength = 3;
    private const int UnicodeDelimitedAmpersandOffset = 1;
    private const int UnicodeDelimitedQuoteOffset = 2;

    private readonly string _source;
    private readonly IReadOnlyList<SyntaxTrivia> _trivia;
    private readonly StringBuilder _sql;
    private readonly bool[] _emitted;
    private readonly bool _color;
    private int _indent;
    private char _lastChar;

    private Formatter(string source, IReadOnlyList<SyntaxTrivia> trivia, bool color)
    {
        _source = source;
        _trivia = trivia;
        _sql = new StringBuilder(source.Length);
        _emitted = new bool[trivia.Count];
        _color = color;
    }

    public static string Format(SqlParseResult tree, bool color = false)
    {
        var formatter = new Formatter(tree.Source, tree.Trivia, color);
        formatter.Write(tree.Root!);
        formatter.AppendLeadingTrivia(tree.Tokens[^1]);
        return formatter._sql.ToString();
    }

    private void AppendCommaExpressions(IReadOnlyList<Expression> expressions)
    {
        for (var i = 0; i < expressions.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            AppendExpression(expressions[i]);
        }
    }

    private void WriteTypeName(SyntaxToken token)
    {
        AppendLeadingTrivia(token);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        if (_color)
        {
            _sql.Append(Ansi.Keyword);
        }

        var span = token.TextOf(_source);
        foreach (var ch in span)
        {
            AppendPlain(ch is >= 'a' and <= 'z' ? (char)(ch - AsciiCaseBit) : ch);
        }

        if (_color)
        {
            _sql.Append(Ansi.Reset);
        }
    }

    private void WriteKeyword(SyntaxToken token, string text)
    {
        AppendLeadingTrivia(token);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendColored(Ansi.Keyword, text);
    }

    private void WriteIdentifier(SyntaxToken token)
    {
        AppendLeadingTrivia(token);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendIdentifier(token);
    }

    private void AppendLeadingTrivia(SyntaxToken token)
    {
        var end = token.LeadingTriviaStart + token.LeadingTriviaCount;
        for (var i = token.LeadingTriviaStart; i < end; i++)
        {
            if (_emitted[i])
            {
                continue;
            }

            AppendComment(_trivia[i]);
            _emitted[i] = true;
        }
    }

    private void AppendComment(SyntaxTrivia comment)
    {
        var text = _source.AsSpan(comment.Position, comment.Length);
        if (comment.Kind == SyntaxKind.LineCommentTrivia)
        {
            if (!AtLineStart())
            {
                AppendSpaceIfNeeded();
            }
            else
            {
                AppendIndent();
            }

            AppendColored(Ansi.Comment, text);
            AppendLine();
            return;
        }

        if (AtLineStart())
        {
            AppendIndent();
            AppendColored(Ansi.Comment, text);
            AppendLine();
            return;
        }

        AppendSpaceIfNeeded();
        AppendColored(Ansi.Comment, text);
    }

    private void AppendSpaceIfNeeded()
    {
        if (_sql.Length == 0)
        {
            return;
        }

        if (_lastChar is ' ' or '\n' or '(' or '[' or '.' or '+' or '-' or ':' or '>')
        {
            return;
        }

        AppendPlain(' ');
    }

    private void AppendLine()
    {
        if (!AtLineStart())
        {
            _sql.AppendLine();
            _lastChar = '\n';
        }
    }

    private bool AtLineStart() => _sql.Length == 0 || _lastChar == '\n';

    private void AppendIndent()
    {
        for (var i = 0; i < _indent; i++)
        {
            AppendPlain(Indent);
        }
    }

    private void AppendPlain(char ch)
    {
        _sql.Append(ch);
        _lastChar = ch;
    }

    private void AppendPlain(string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        _sql.Append(text);
        _lastChar = text[^1];
    }

    private void AppendPlain(ReadOnlySpan<char> text)
    {
        if (text.Length == 0)
        {
            return;
        }

        _sql.Append(text);
        _lastChar = text[^1];
    }

    private void AppendColored(string color, string text)
    {
        if (_color)
        {
            _sql.Append(color);
        }

        AppendPlain(text);
        if (_color)
        {
            _sql.Append(Ansi.Reset);
        }
    }

    private void AppendColored(string color, ReadOnlySpan<char> text)
    {
        if (_color)
        {
            _sql.Append(color);
        }

        AppendPlain(text);
        if (_color)
        {
            _sql.Append(Ansi.Reset);
        }
    }

    private void AppendLiteral(SyntaxToken literal)
    {
        var color = literal.Kind == SyntaxKind.String ? Ansi.String : Ansi.Number;
        AppendColored(color, literal.TextOf(_source));
    }

    private void EnsureContentIndent()
    {
        if (AtLineStart())
        {
            AppendIndent();
        }
    }

    private static SyntaxToken StartToken(Expression expression) => expression switch
    {
        ArrayExpression array => array.ArrayKeyword,
        CastExpression cast => cast.CastKeyword,
        CoalesceExpression coalesce => coalesce.CoalesceKeyword,
        NullIfExpression nullIf => nullIf.NullIfKeyword,
        ColonCastExpression colonCast => StartToken(colonCast.Expression),
        CollateExpression collate => StartToken(collate.Expression),
        IdentifierExpression identifier => identifier.Identifier,
        EmbeddedHostExpression host => host.Name,
        HostParameterExpression parameter => parameter.QuestionMark,
        LiteralExpression literal => literal.Literal,
        NiladicFunctionExpression niladic => niladic.Name,
        DatetimeLiteralExpression datetime => datetime.KindKeyword,
        IntervalLiteralExpression interval => interval.IntervalKeyword,
        TrimExpression trim => trim.TrimKeyword,
        ExtractExpression extract => extract.ExtractKeyword,
        SubstringExpression substring => substring.SubstringKeyword,
        PositionExpression position => position.PositionKeyword,
        UsingTransformExpression transform => transform.FunctionKeyword,
        BinaryExpression binary => StartToken(binary.Left),
        MemberAccessExpression member => StartToken(member.Target),
        BetweenExpression between => StartToken(between.Target),
        InExpression inExpression => StartToken(inExpression.Target),
        LikeExpression like => StartToken(like.Target),
        IsExpression isExpression => StartToken(isExpression.Target),
        NotExpression not => not.NotKeyword,
        FunctionCallExpression call => call.Name,
        RowConstructorExpression row => row.OpenParen,
        OverlapsExpression overlaps => StartToken(overlaps.Left),
        ParenExpression paren => paren.OpenParen,
        ScalarSubqueryExpression subquery => subquery.OpenParen,
        ExistsExpression exists => exists.ExistsKeyword,
        UniqueExpression unique => unique.UniqueKeyword,
        MatchExpression match => StartToken(match.Left),
        QuantifiedSubqueryExpression quantified => StartToken(quantified.Left),
        StarExpression star => star.Star,
        QualifiedStarExpression qualifiedStar => StartToken(qualifiedStar.Target),
        CaseExpression caseExpression => caseExpression.CaseKeyword,
        UnaryExpression unary => unary.OperatorToken,
        TreatExpression treat => treat.TreatKeyword,
        NextValueExpression next => next.NextKeyword,
        SpecialFormExpression special => special.Name,
        OverlayExpression overlay => overlay.OverlayKeyword,
        ArrayQueryExpression arrayQuery => arrayQuery.ArrayKeyword,
        MultisetExpression multiset => multiset.MultisetKeyword,
        MultisetQueryExpression multisetQuery => multisetQuery.Keyword,
        MultisetOperationExpression multisetOp => StartToken(multisetOp.Left),
        MultisetSetExpression multisetSet => multisetSet.SetKeyword,
        AbsentOnNullExpression absent => absent.AbsentKeyword,
        DerefExpression deref => deref.DerefKeyword,
        RefValueExpression refValue => refValue.RefKeyword,
        DereferenceExpression dereference => StartToken(dereference.Reference),
        SpecifictypeExpression specifictype => specifictype.SpecifictypeKeyword,
        MethodInvocationExpression method => method.OpenParen ?? StartToken(method.Target),
        StaticMethodInvocationExpression staticMethod => StartToken(staticMethod.Type),
        NewSpecificationExpression created => created.NewKeyword,
        _ => throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}"),
    };

    private void WriteTypeNameTail(SyntaxToken name, IReadOnlyList<SyntaxToken> nameTail)
    {
        var previous = name;
        foreach (var part in nameTail)
        {
            if (HasDotBetween(previous, part))
            {
                AppendPlain('.');
            }

            WriteTypeName(part);
            previous = part;
        }
    }

    private bool HasDotBetween(SyntaxToken left, SyntaxToken right)
    {
        var start = left.Position + left.Length;
        var end = right.Position;
        if (end <= start)
        {
            return false;
        }

        return _source.AsSpan(start, end - start).Contains('.');
    }

    private void WriteQualifiedName(IReadOnlyList<SyntaxToken> parts)
    {
        for (var i = 0; i < parts.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain('.');
            }

            WriteIdentifier(parts[i]);
        }
    }

    private static bool IsPreservedSpelling(ReadOnlySpan<char> span)
    {
        if (span.Length == 0)
        {
            return false;
        }

        if (span[0] == IdentifierQuote)
        {
            return true;
        }

        if (span.Length >= UnicodeDelimitedPrefixLength
            && span[0] is 'U' or 'u'
            && span[UnicodeDelimitedAmpersandOffset] == Ampersand
            && span[UnicodeDelimitedQuoteOffset] == IdentifierQuote)
        {
            return true;
        }

        if (span[0] != CharsetIntroducer)
        {
            return false;
        }

        for (var i = 1; i < span.Length; i++)
        {
            if (span[i] == IdentifierQuote)
            {
                return true;
            }
        }

        return false;
    }

    private void AppendIdentifier(SyntaxToken token)
    {
        var span = token.TextOf(_source);
        if (IsPreservedSpelling(span))
        {
            AppendPlain(span);
            return;
        }

        for (var i = 0; i < span.Length; i++)
        {
            var ch = span[i];
            if (ch is >= 'A' and <= 'Z' || !char.IsAscii(ch))
            {
                AppendPlain(span[..i]);
                AppendLowerRest(span[i..]);
                return;
            }
        }

        AppendPlain(span);
    }

    private void AppendLowerRest(ReadOnlySpan<char> span)
    {
        foreach (var ch in span)
        {
            if (ch is >= 'A' and <= 'Z')
            {
                AppendPlain((char)(ch + AsciiCaseBit));
            }
            else if (char.IsAscii(ch))
            {
                AppendPlain(ch);
            }
            else
            {
                AppendPlain(char.ToLowerInvariant(ch));
            }
        }
    }

    private void AppendOperator(SyntaxToken token)
    {
        AppendLeadingTrivia(token);
        EnsureContentIndent();
        if (token.Kind is SyntaxKind.JsonArrowToken or SyntaxKind.JsonTextArrowToken)
        {
            AppendPlain(token.Kind == SyntaxKind.JsonArrowToken ? JsonArrow : JsonTextArrow);
            return;
        }

        AppendSpaceIfNeeded();
        switch (token.Kind)
        {
            case SyntaxKind.ConcatToken:
                AppendPlain(Concat);
                break;
            case SyntaxKind.PlusToken:
                AppendPlain('+');
                break;
            case SyntaxKind.MinusToken:
                AppendPlain('-');
                break;
            case SyntaxKind.Star:
                AppendPlain('*');
                break;
            case SyntaxKind.SlashToken:
                AppendPlain('/');
                break;
            case SyntaxKind.EqualsToken:
                AppendPlain('=');
                break;
            case SyntaxKind.NotEqualsToken:
                AppendPlain('!');
                AppendPlain('=');
                break;
            case SyntaxKind.GreaterThan:
                AppendPlain('>');
                break;
            case SyntaxKind.GreaterOrEqual:
                AppendPlain('>');
                AppendPlain('=');
                break;
            case SyntaxKind.LessThan:
                AppendPlain('<');
                break;
            case SyntaxKind.LessOrEqual:
                AppendPlain('<');
                AppendPlain('=');
                break;
            case SyntaxKind.AndKeyword:
                AppendColored(Ansi.Keyword, Keyword.AndUpper);
                break;
            case SyntaxKind.OrKeyword:
                AppendColored(Ansi.Keyword, Keyword.OrUpper);
                break;
            default:
                throw new InvalidOperationException($"Unknown operator {token.Kind}");
        }
    }
}
