using System.Text;
using QlParse;

namespace QlFmt;

internal sealed class Formatter
{
    private const int AsciiCaseBit = 'a' - 'A';
    private const string Indent = "    ";
    private const string DoubleColon = "::";
    private const string Concat = "||";
    private const string JsonArrow = "->";
    private const string JsonTextArrow = "->>";
    private const char IdentifierQuote = '"';

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

    private void Write(Query query)
    {
        switch (query)
        {
            case SelectStatement select:
                WriteSelect(select);
                break;
            case SetOperation setOp:
                WriteSetOperation(setOp);
                break;
            case ParenQuery paren:
                WriteParenQuery(paren);
                break;
            case WithQuery withQuery:
                WriteWithQuery(withQuery);
                break;
            case ValuesQuery values:
                WriteValues(values);
                break;
            case InsertStatement insert:
                WriteInsert(insert);
                break;
            default:
                throw new InvalidOperationException($"Unknown query {query.GetType().Name}");
        }
    }

    private void WriteWithQuery(WithQuery withQuery)
    {
        WriteKeyword(withQuery.WithKeyword, Keyword.WithUpper);
        if (withQuery.RecursiveKeyword is { } recursive)
        {
            AppendPlain(' ');
            WriteKeyword(recursive, Keyword.RecursiveUpper);
        }

        _indent++;
        for (var i = 0; i < withQuery.Ctes.Count; i++)
        {
            AppendLine();
            WriteCte(withQuery.Ctes[i]);
            if (i < withQuery.Ctes.Count - 1)
            {
                AppendPlain(',');
            }
        }

        _indent--;
        AppendLine();
        Write(withQuery.Query);
    }

    private void WriteCte(CommonTableExpression cte)
    {
        WriteIdentifier(cte.Name);
        if (cte.Columns is { } columns)
        {
            AppendPlain('(');
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteIdentifier(columns[i]);
            }

            AppendPlain(')');
        }

        AppendPlain(' ');
        WriteKeyword(cte.AsKeyword, Keyword.AsUpper);
        AppendPlain(' ');
        AppendSubquery(cte.OpenQuery, cte.Query, cte.CloseQuery);
    }

    private void WriteParenQuery(ParenQuery paren)
    {
        AppendLeadingTrivia(paren.OpenParen);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        AppendLine();
        Write(paren.Inner);
        _indent--;
        AppendLeadingTrivia(paren.CloseParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
    }

    private void WriteSetOperation(SetOperation setOp)
    {
        Write(setOp.Left);
        AppendLeadingTrivia(setOp.Operator);
        AppendLine();
        WriteKeyword(setOp.Operator, SetOperatorText(setOp.Operator.Kind));
        if (setOp.AllKeyword is { } all)
        {
            AppendPlain(' ');
            WriteKeyword(all, Keyword.AllUpper);
        }

        if (setOp.CorrespondingKeyword is { } corresponding)
        {
            AppendPlain(' ');
            WriteKeyword(corresponding, Keyword.CorrespondingUpper);
            if (setOp.ByKeyword is { } by && setOp.Columns is { } columns)
            {
                AppendPlain(' ');
                WriteKeyword(by, Keyword.ByUpper);
                AppendPlain(' ');
                AppendPlain('(');
                for (var i = 0; i < columns.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    WriteIdentifier(columns[i]);
                }

                AppendPlain(')');
            }
        }

        AppendLine();
        Write(setOp.Right);
    }

    private void WriteInsert(InsertStatement insert)
    {
        WriteKeyword(insert.InsertKeyword, Keyword.InsertUpper);
        AppendPlain(' ');
        WriteKeyword(insert.IntoKeyword, Keyword.IntoUpper);
        for (var i = 0; i < insert.TableName.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain('.');
            }

            WriteIdentifier(insert.TableName[i]);
        }

        if (insert.Columns is { } columns
            && insert.ColumnOpenParen is { } open
            && insert.ColumnCloseParen is { } close)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            _indent++;
            for (var i = 0; i < columns.Count; i++)
            {
                AppendLine();
                WriteIdentifier(columns[i]);
                if (i < columns.Count - 1)
                {
                    AppendPlain(',');
                }
            }

            _indent--;
            AppendLeadingTrivia(close);
            AppendLine();
            EnsureContentIndent();
            AppendPlain(')');
        }

        AppendLine();
        Write(insert.Query);
    }

    private void WriteValues(ValuesQuery values)
    {
        WriteKeyword(values.ValuesKeyword, Keyword.ValuesUpper);
        _indent++;
        for (var i = 0; i < values.Rows.Count; i++)
        {
            var row = values.Rows[i];
            AppendLeadingTrivia(row.OpenParen);
            AppendLine();
            EnsureContentIndent();
            AppendPlain('(');
            AppendCommaExpressions(row.Values);
            AppendPlain(')');
            if (i < values.Rows.Count - 1)
            {
                AppendPlain(',');
            }
        }

        _indent--;
    }

    private void WriteSelect(SelectStatement statement)
    {
        WriteKeyword(statement.SelectKeyword, Keyword.SelectUpper);
        if (statement.DistinctKeyword is { } distinct)
        {
            AppendPlain(' ');
            WriteKeyword(distinct, Keyword.DistinctUpper);
        }
        else if (statement.AllKeyword is { } all)
        {
            AppendPlain(' ');
            WriteKeyword(all, Keyword.AllUpper);
        }

        _indent++;
        for (var i = 0; i < statement.SelectList.Count; i++)
        {
            if (i > 0)
            {
                AppendLeadingTrivia(StartToken(statement.SelectList[i].Expression));
            }

            AppendLine();
            AppendSelectItem(statement.SelectList[i]);
            if (i < statement.SelectList.Count - 1)
            {
                AppendPlain(',');
            }
        }

        _indent--;
        if (statement.FromKeyword is { } fromKeyword && statement.From is { } from)
        {
            AppendLeadingTrivia(fromKeyword);
            AppendLine();
            WriteKeyword(fromKeyword, Keyword.FromUpper);
            AppendPlain(' ');
            AppendTable(from);
            foreach (var join in statement.Joins)
            {
                AppendLine();
                AppendJoin(join);
            }

            foreach (var extra in statement.ExtraFrom)
            {
                AppendPlain(',');
                AppendPlain(' ');
                AppendTable(extra.Table);
                foreach (var join in extra.Joins)
                {
                    AppendLine();
                    AppendJoin(join);
                }
            }
        }

        if (statement.Where is not null)
        {
            AppendLeadingTrivia(statement.Where.WhereKeyword);
            AppendLine();
            WriteKeyword(statement.Where.WhereKeyword, Keyword.WhereUpper);
            AppendPlain(' ');
            AppendWhereExpression(statement.Where.Expression);
        }

        if (statement.GroupBy is not null)
        {
            AppendLeadingTrivia(statement.GroupBy.GroupKeyword);
            AppendLine();
            WriteKeyword(statement.GroupBy.GroupKeyword, Keyword.GroupUpper);
            AppendPlain(' ');
            WriteKeyword(statement.GroupBy.ByKeyword, Keyword.ByUpper);
            AppendPlain(' ');
            AppendCommaExpressions(statement.GroupBy.Keys);
        }

        if (statement.Having is not null)
        {
            AppendLeadingTrivia(statement.Having.HavingKeyword);
            AppendLine();
            WriteKeyword(statement.Having.HavingKeyword, Keyword.HavingUpper);
            AppendPlain(' ');
            AppendWhereExpression(statement.Having.Expression);
        }

        if (statement.OrderBy is not null)
        {
            AppendLeadingTrivia(statement.OrderBy.OrderKeyword);
            AppendLine();
            WriteKeyword(statement.OrderBy.OrderKeyword, Keyword.OrderUpper);
            AppendPlain(' ');
            WriteKeyword(statement.OrderBy.ByKeyword, Keyword.ByUpper);
            AppendPlain(' ');
            for (var i = 0; i < statement.OrderBy.Items.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                var item = statement.OrderBy.Items[i];
                AppendExpression(item.Expression);
                if (item.Direction is { } direction)
                {
                    AppendPlain(' ');
                    WriteKeyword(
                        direction,
                        direction.Kind == SyntaxKind.DescKeyword ? Keyword.DescUpper : Keyword.AscUpper);
                }
            }
        }

        if (statement.Limit is not null)
        {
            AppendLeadingTrivia(statement.Limit.LimitKeyword);
            AppendLine();
            WriteKeyword(statement.Limit.LimitKeyword, Keyword.LimitUpper);
            AppendPlain(' ');
            AppendExpression(statement.Limit.Count);
        }

        if (statement.Offset is not null)
        {
            AppendLeadingTrivia(statement.Offset.OffsetKeyword);
            AppendLine();
            WriteKeyword(statement.Offset.OffsetKeyword, Keyword.OffsetUpper);
            AppendPlain(' ');
            AppendExpression(statement.Offset.Count);
        }

        if (statement.Lock is { } lockClause)
        {
            WriteLockClause(lockClause);
        }
    }

    private void WriteLockClause(LockClause lockClause)
    {
        AppendLeadingTrivia(lockClause.ForKeyword);
        AppendLine();
        WriteKeyword(lockClause.ForKeyword, Keyword.ForUpper);
        if (lockClause.ReadKeyword is { } readKeyword && lockClause.OnlyKeyword is { } onlyKeyword)
        {
            AppendPlain(' ');
            WriteKeyword(readKeyword, Keyword.ReadUpper);
            AppendPlain(' ');
            WriteKeyword(onlyKeyword, Keyword.OnlyUpper);
            return;
        }

        if (lockClause.UpdateKeyword is { } updateKeyword)
        {
            AppendPlain(' ');
            WriteKeyword(updateKeyword, Keyword.UpdateUpper);
        }

        if (lockClause.OfKeyword is { } ofKeyword && lockClause.Columns is { } columns)
        {
            AppendPlain(' ');
            WriteKeyword(ofKeyword, Keyword.OfUpper);
            AppendPlain(' ');
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteIdentifier(columns[i]);
            }
        }
    }

    private void AppendSelectItem(SelectItem item)
    {
        AppendExpression(item.Expression);
        if (item.Alias is { } alias)
        {
            AppendPlain(' ');
            if (item.AsKeyword is { } asKeyword)
            {
                WriteKeyword(asKeyword, Keyword.AsUpper);
            }
            else
            {
                AppendColored(Ansi.Keyword, Keyword.AsUpper);
            }

            AppendPlain(' ');
            WriteIdentifier(alias);
        }
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

    private void AppendJoin(JoinClause join)
    {
        if (join.NaturalKeyword is { } natural)
        {
            WriteKeyword(natural, Keyword.NaturalUpper);
            AppendPlain(' ');
        }

        if (join.JoinType is { } joinType)
        {
            WriteKeyword(joinType, JoinTypeText(joinType.Kind));
            AppendPlain(' ');
        }

        if (join.OuterKeyword is { } outer)
        {
            WriteKeyword(outer, Keyword.OuterUpper);
            AppendPlain(' ');
        }

        WriteKeyword(join.JoinKeyword, Keyword.JoinUpper);
                    AppendPlain(' ');
        AppendTable(join.Table);
        switch (join.Constraint)
        {
            case OnConstraint on:
                _indent++;
                AppendLeadingTrivia(on.OnKeyword);
                AppendLine();
                WriteKeyword(on.OnKeyword, Keyword.OnUpper);
                AppendPlain(' ');
                AppendExpression(on.Condition);
                _indent--;
                break;
            case UsingConstraint usingConstraint:
                _indent++;
                AppendLeadingTrivia(usingConstraint.UsingKeyword);
                AppendLine();
                WriteKeyword(usingConstraint.UsingKeyword, Keyword.UsingUpper);
                AppendPlain(' ');
                AppendPlain('(');
                for (var i = 0; i < usingConstraint.Columns.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    WriteIdentifier(usingConstraint.Columns[i]);
                }

                AppendPlain(')');
                _indent--;
                break;
        }
    }

    private static string SetOperatorText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.UnionKeyword => Keyword.UnionUpper,
        SyntaxKind.ExceptKeyword => Keyword.ExceptUpper,
        SyntaxKind.IntersectKeyword => Keyword.IntersectUpper,
        _ => throw new InvalidOperationException($"Unknown set operator {kind}"),
    };

    private static string QuantifierText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.AllKeyword => Keyword.AllUpper,
        SyntaxKind.AnyKeyword => Keyword.AnyUpper,
        SyntaxKind.SomeKeyword => Keyword.SomeUpper,
        _ => throw new InvalidOperationException($"Unknown quantifier {kind}"),
    };

    private static string IsValueText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.NullKeyword => Keyword.NullUpper,
        SyntaxKind.TrueKeyword => Keyword.TrueUpper,
        SyntaxKind.FalseKeyword => Keyword.FalseUpper,
        SyntaxKind.UnknownKeyword => Keyword.UnknownUpper,
        _ => throw new InvalidOperationException($"Unknown IS value {kind}"),
    };

    private static string MatchTypeText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.PartialKeyword => Keyword.PartialUpper,
        SyntaxKind.FullKeyword => Keyword.FullUpper,
        _ => throw new InvalidOperationException($"Unknown match type {kind}"),
    };

    private static string JoinTypeText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.InnerKeyword => Keyword.InnerUpper,
        SyntaxKind.LeftKeyword => Keyword.LeftUpper,
        SyntaxKind.RightKeyword => Keyword.RightUpper,
        SyntaxKind.FullKeyword => Keyword.FullUpper,
        SyntaxKind.CrossKeyword => Keyword.CrossUpper,
        SyntaxKind.UnionKeyword => Keyword.UnionUpper,
        _ => throw new InvalidOperationException($"Unknown join type {kind}"),
    };

    private void AppendTable(TableSource table)
    {
        switch (table)
        {
            case TableReference named:
                AppendNamedTable(named);
                break;
            case DerivedTable derived:
                AppendDerivedTable(derived);
                break;
            case JoinedTable joined:
                AppendJoinedTable(joined);
                break;
            default:
                throw new InvalidOperationException($"Unknown table {table.GetType().Name}");
        }
    }

    private void AppendNamedTable(TableReference table)
    {
        for (var i = 0; i < table.NameParts.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain('.');
            }

            WriteIdentifier(table.NameParts[i]);
        }

        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
    }

    private void AppendDerivedTable(DerivedTable table)
    {
        AppendSubquery(table.OpenParen, table.Query, table.CloseParen);
        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
    }

    private void AppendJoinedTable(JoinedTable table)
    {
        AppendLeadingTrivia(table.OpenParen);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        AppendLine();
        AppendTable(table.Table);
        foreach (var join in table.Joins)
        {
            AppendLine();
            AppendJoin(join);
        }

        _indent--;
        AppendLeadingTrivia(table.CloseParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
    }

    private void AppendSubquery(SyntaxToken openParen, Query query, SyntaxToken closeParen)
    {
        AppendLeadingTrivia(openParen);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        AppendLine();
        Write(query);
        _indent--;
        AppendLeadingTrivia(closeParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
    }

    private void AppendTableAlias(SyntaxToken? asKeyword, SyntaxToken? alias)
    {
        if (alias is not { } name)
        {
            return;
        }

        AppendPlain(' ');
        if (asKeyword is { } keyword)
        {
            WriteKeyword(keyword, Keyword.AsUpper);
        }
        else
        {
            AppendColored(Ansi.Keyword, Keyword.AsUpper);
        }

        AppendPlain(' ');
        WriteIdentifier(name);
    }

    private void AppendCorrelationColumns(
        SyntaxToken? openParen,
        IReadOnlyList<SyntaxToken>? columns,
        SyntaxToken? closeParen)
    {
        if (openParen is not { } open || columns is null || closeParen is null)
        {
            return;
        }

        var last = _lastChar;
        AppendLeadingTrivia(open);
        if (_lastChar != last)
        {
            AppendSpaceIfNeeded();
        }

        AppendPlain('(');
        for (var i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteIdentifier(columns[i]);
        }

        AppendPlain(')');
    }

    private void AppendWhereExpression(Expression expression)
    {
        if (expression is BinaryExpression { OperatorToken.Kind: SyntaxKind.AndKeyword } and)
        {
            AppendWhereExpression(and.Left);
            _indent++;
            AppendLeadingTrivia(and.OperatorToken);
            AppendLine();
            WriteKeyword(and.OperatorToken, Keyword.AndUpper);
            AppendPlain(' ');
            AppendExpression(and.Right);
            _indent--;
            return;
        }

        AppendExpression(expression);
    }

    private void AppendExpression(Expression expression)
    {
        switch (expression)
        {
            case ArrayExpression array:
                WriteKeyword(array.ArrayKeyword, Keyword.ArrayUpper);
                AppendLeadingTrivia(array.OpenBracket);
                AppendPlain('[');
                for (var i = 0; i < array.Elements.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    AppendExpression(array.Elements[i]);
                }

                AppendPlain(']');
                break;
            case CastExpression cast:
                WriteKeyword(cast.CastKeyword, Keyword.CastUpper);
                AppendPlain('(');
                AppendExpression(cast.Expression);
                AppendPlain(' ');
                WriteKeyword(cast.AsKeyword, Keyword.AsUpper);
                AppendPlain(' ');
                WriteDataType(cast.Type);
                AppendPlain(')');
                break;
            case CoalesceExpression coalesce:
                WriteIdentifier(coalesce.CoalesceKeyword);
                AppendPlain('(');
                AppendCommaExpressions(coalesce.Arguments);
                AppendPlain(')');
                break;
            case NullIfExpression nullIf:
                WriteIdentifier(nullIf.NullIfKeyword);
                AppendPlain('(');
                AppendExpression(nullIf.First);
                AppendPlain(',');
                AppendPlain(' ');
                AppendExpression(nullIf.Second);
                AppendPlain(')');
                break;
            case ColonCastExpression colonCast:
                AppendExpression(colonCast.Expression);
                AppendLeadingTrivia(colonCast.DoubleColon);
                AppendPlain(DoubleColon);
                WriteDataType(colonCast.Type);
                break;
            case CollateExpression collate:
                AppendExpression(collate.Expression);
                AppendPlain(' ');
                WriteKeyword(collate.CollateKeyword, Keyword.CollateUpper);
                AppendPlain(' ');
                WriteIdentifier(collate.Name);
                break;
            case IdentifierExpression identifier:
                WriteIdentifier(identifier.Identifier);
                break;
            case EmbeddedHostExpression host:
                AppendLeadingTrivia(host.Name);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain(host.Name.TextOf(_source));
                break;
            case HostParameterExpression parameter:
                AppendLeadingTrivia(parameter.QuestionMark);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain('?');
                break;
            case NiladicFunctionExpression niladic:
                WriteNiladic(niladic);
                break;
            case LiteralExpression literal:
                if (literal.Literal.Kind == SyntaxKind.TrueKeyword)
                {
                    WriteKeyword(literal.Literal, Keyword.TrueUpper);
                    break;
                }

                if (literal.Literal.Kind == SyntaxKind.FalseKeyword)
                {
                    WriteKeyword(literal.Literal, Keyword.FalseUpper);
                    break;
                }

                if (literal.Literal.Kind == SyntaxKind.NullKeyword)
                {
                    WriteKeyword(literal.Literal, Keyword.NullUpper);
                    break;
                }

                AppendLeadingTrivia(literal.Literal);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendLiteral(literal.Literal);
                break;
            case DatetimeLiteralExpression datetime:
                WriteKeyword(datetime.KindKeyword, DatetimeKeywordText(datetime.KindKeyword.Kind));
                AppendLeadingTrivia(datetime.Literal);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendLiteral(datetime.Literal);
                break;
            case IntervalLiteralExpression interval:
                WriteInterval(interval);
                break;
            case TrimExpression trim:
                WriteTrim(trim);
                break;
            case ExtractExpression extract:
                WriteExtract(extract);
                break;
            case SubstringExpression substring:
                WriteSubstring(substring);
                break;
            case PositionExpression position:
                WritePosition(position);
                break;
            case UsingTransformExpression transform:
                WriteUsingTransform(transform);
                break;
            case BinaryExpression binary:
                AppendExpression(binary.Left);
                if (binary.OperatorToken.Kind is not (SyntaxKind.JsonArrowToken or SyntaxKind.JsonTextArrowToken))
                {
                    AppendPlain(' ');
                }

                AppendOperator(binary.OperatorToken);
                if (binary.OperatorToken.Kind is not (SyntaxKind.JsonArrowToken or SyntaxKind.JsonTextArrowToken))
                {
                    AppendPlain(' ');
                }

                AppendExpression(binary.Right);
                break;
            case MemberAccessExpression member:
                AppendExpression(member.Target);
                AppendPlain('.');
                WriteIdentifier(member.Member);
                break;
            case BetweenExpression between:
                AppendExpression(between.Target);
                AppendPlain(' ');
                if (between.NotKeyword is { } notBetween)
                {
                    WriteKeyword(notBetween, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(between.BetweenKeyword, Keyword.BetweenUpper);
                AppendPlain(' ');
                AppendExpression(between.Lower);
                AppendPlain(' ');
                WriteKeyword(between.AndKeyword, Keyword.AndUpper);
                AppendPlain(' ');
                AppendExpression(between.Upper);
                break;
            case InExpression inExpression:
                AppendExpression(inExpression.Target);
                AppendPlain(' ');
                if (inExpression.NotKeyword is { } notIn)
                {
                    WriteKeyword(notIn, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(inExpression.InKeyword, Keyword.InUpper);
                AppendPlain(' ');
                if (inExpression.Query is { } inQuery)
                {
                    AppendSubquery(inExpression.OpenParen, inQuery, inExpression.CloseParen);
                    break;
                }

                AppendPlain('(');
                for (var i = 0; i < inExpression.Values.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    AppendExpression(inExpression.Values[i]);
                }

                AppendPlain(')');
                break;
            case LikeExpression like:
                AppendExpression(like.Target);
                AppendPlain(' ');
                if (like.NotKeyword is { } notLike)
                {
                    WriteKeyword(notLike, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(like.LikeKeyword, Keyword.LikeUpper);
                AppendPlain(' ');
                AppendExpression(like.Pattern);
                if (like.EscapeKeyword is { } escapeKeyword && like.Escape is { } escape)
                {
                    AppendPlain(' ');
                    WriteKeyword(escapeKeyword, Keyword.EscapeUpper);
                    AppendPlain(' ');
                    AppendExpression(escape);
                }

                break;
            case IsExpression isExpression:
                AppendExpression(isExpression.Target);
                AppendPlain(' ');
                WriteKeyword(isExpression.IsKeyword, Keyword.IsUpper);
                AppendPlain(' ');
                if (isExpression.NotKeyword is { } notKeyword)
                {
                    WriteKeyword(notKeyword, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(isExpression.Value, IsValueText(isExpression.Value.Kind));
                break;
            case NotExpression not:
                WriteKeyword(not.NotKeyword, Keyword.NotUpper);
                AppendPlain(' ');
                AppendExpression(not.Expression);
                break;
            case FunctionCallExpression call:
                WriteIdentifier(call.Name);
                AppendPlain('(');
                AppendCommaExpressions(call.Arguments);
                AppendPlain(')');
                if (call.Filter is { } filter)
                {
                    AppendPlain(' ');
                    WriteKeyword(filter.FilterKeyword, Keyword.FilterUpper);
                    AppendLeadingTrivia(filter.OpenParen);
                    AppendSpaceIfNeeded();
                    AppendPlain('(');
                    WriteKeyword(filter.WhereKeyword, Keyword.WhereUpper);
                    AppendPlain(' ');
                    AppendExpression(filter.Expression);
                    AppendPlain(')');
                }

                break;
            case RowConstructorExpression row:
                AppendLeadingTrivia(row.OpenParen);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain('(');
                AppendCommaExpressions(row.Elements);
                AppendPlain(')');
                break;
            case OverlapsExpression overlaps:
                AppendExpression(overlaps.Left);
                AppendPlain(' ');
                WriteKeyword(overlaps.OverlapsKeyword, Keyword.OverlapsUpper);
                AppendPlain(' ');
                AppendExpression(overlaps.Right);
                break;
            case ParenExpression paren:
                AppendLeadingTrivia(paren.OpenParen);
                EnsureContentIndent();
                AppendPlain('(');
                AppendExpression(paren.Inner);
                AppendPlain(')');
                break;
            case ScalarSubqueryExpression subquery:
                AppendSubquery(subquery.OpenParen, subquery.Query, subquery.CloseParen);
                break;
            case ExistsExpression exists:
                WriteKeyword(exists.ExistsKeyword, Keyword.ExistsUpper);
                AppendPlain(' ');
                AppendSubquery(exists.OpenParen, exists.Query, exists.CloseParen);
                break;
            case UniqueExpression unique:
                WriteKeyword(unique.UniqueKeyword, Keyword.UniqueUpper);
                AppendPlain(' ');
                AppendSubquery(unique.OpenParen, unique.Query, unique.CloseParen);
                break;
            case MatchExpression match:
                AppendExpression(match.Left);
                AppendPlain(' ');
                WriteKeyword(match.MatchKeyword, Keyword.MatchUpper);
                if (match.UniqueKeyword is { } uniqueKeyword)
                {
                    AppendPlain(' ');
                    WriteKeyword(uniqueKeyword, Keyword.UniqueUpper);
                }

                if (match.MatchType is { } matchType)
                {
                    AppendPlain(' ');
                    WriteKeyword(matchType, MatchTypeText(matchType.Kind));
                }

                AppendPlain(' ');
                AppendSubquery(match.OpenParen, match.Query, match.CloseParen);
                break;
            case QuantifiedSubqueryExpression quantified:
                AppendExpression(quantified.Left);
                AppendPlain(' ');
                AppendOperator(quantified.OperatorToken);
                AppendPlain(' ');
                WriteKeyword(quantified.Quantifier, QuantifierText(quantified.Quantifier.Kind));
                AppendPlain(' ');
                AppendSubquery(quantified.OpenParen, quantified.Query, quantified.CloseParen);
                break;
            case StarExpression star:
                AppendLeadingTrivia(star.Star);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain('*');
                break;
            case QualifiedStarExpression qualifiedStar:
                AppendExpression(qualifiedStar.Target);
                AppendPlain('.');
                AppendPlain('*');
                break;
            case CaseExpression caseExpression:
                AppendCase(caseExpression);
                break;
            case UnaryExpression unary:
                AppendLeadingTrivia(unary.OperatorToken);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain(unary.OperatorToken.Kind == SyntaxKind.MinusToken ? '-' : '+');
                AppendExpression(unary.Expression);
                break;
            default:
                throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}");
        }
    }

    private void AppendCase(CaseExpression expression)
    {
        WriteKeyword(expression.CaseKeyword, Keyword.CaseUpper);
        if (expression.Operand is { } operand)
        {
            AppendPlain(' ');
            AppendExpression(operand);
        }

        _indent++;
        foreach (var arm in expression.Arms)
        {
            AppendLeadingTrivia(arm.WhenKeyword);
            AppendLine();
            WriteKeyword(arm.WhenKeyword, Keyword.WhenUpper);
            AppendPlain(' ');
            AppendExpression(arm.Condition);
            AppendPlain(' ');
            WriteKeyword(arm.ThenKeyword, Keyword.ThenUpper);
            AppendPlain(' ');
            AppendExpression(arm.Result);
        }

        if (expression.ElseKeyword is { } elseKeyword && expression.ElseResult is { } elseResult)
        {
            AppendLeadingTrivia(elseKeyword);
            AppendLine();
            WriteKeyword(elseKeyword, Keyword.ElseUpper);
            AppendPlain(' ');
            AppendExpression(elseResult);
        }

        _indent--;
        AppendLeadingTrivia(expression.EndKeyword);
        AppendLine();
        WriteKeyword(expression.EndKeyword, Keyword.EndUpper);
    }

    private void WriteNiladic(NiladicFunctionExpression niladic)
    {
        WriteKeyword(niladic.Name, NiladicKeywordText(niladic.Name.Kind));
        if (niladic.OpenParen is not { } openParen
            || niladic.Precision is not { } precision
            || niladic.CloseParen is null)
        {
            return;
        }

        var last = _lastChar;
        AppendLeadingTrivia(openParen);
        if (_lastChar != last)
        {
            AppendSpaceIfNeeded();
        }

        AppendPlain('(');
        AppendPlain(precision.TextOf(_source));
        AppendPlain(')');
    }

    private static string NiladicKeywordText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.UserKeyword => Keyword.UserUpper,
        SyntaxKind.CurrentDateKeyword => Keyword.CurrentDateUpper,
        SyntaxKind.CurrentTimeKeyword => Keyword.CurrentTimeUpper,
        SyntaxKind.CurrentTimestampKeyword => Keyword.CurrentTimestampUpper,
        SyntaxKind.CurrentUserKeyword => Keyword.CurrentUserUpper,
        SyntaxKind.SessionUserKeyword => Keyword.SessionUserUpper,
        SyntaxKind.SystemUserKeyword => Keyword.SystemUserUpper,
        _ => throw new InvalidOperationException($"Unknown niladic function {kind}"),
    };

    private void WriteTrim(TrimExpression trim)
    {
        WriteKeyword(trim.TrimKeyword, Keyword.TrimUpper);
        AppendPlain('(');
        if (trim.Specification is { } specification)
        {
            WriteTypeName(specification);
        }

        if (trim.Characters is { } characters)
        {
            AppendExpression(characters);
        }

        if (trim.FromKeyword is { } fromKeyword)
        {
            AppendPlain(' ');
            WriteKeyword(fromKeyword, Keyword.FromUpper);
        }

        AppendExpression(trim.Source);
        AppendPlain(')');
    }

    private void WriteExtract(ExtractExpression extract)
    {
        WriteKeyword(extract.ExtractKeyword, Keyword.ExtractUpper);
        AppendPlain('(');
        WriteTypeName(extract.Field);
        AppendPlain(' ');
        WriteKeyword(extract.FromKeyword, Keyword.FromUpper);
        AppendExpression(extract.Source);
        AppendPlain(')');
    }

    private void WriteSubstring(SubstringExpression substring)
    {
        WriteKeyword(substring.SubstringKeyword, Keyword.SubstringUpper);
        AppendPlain('(');
        AppendExpression(substring.Source);
        AppendPlain(' ');
        WriteKeyword(substring.FromKeyword, Keyword.FromUpper);
        AppendExpression(substring.Start);
        if (substring.ForKeyword is { } forKeyword && substring.Length is { } length)
        {
            AppendPlain(' ');
            WriteKeyword(forKeyword, Keyword.ForUpper);
            AppendExpression(length);
        }

        AppendPlain(')');
    }

    private void WriteUsingTransform(UsingTransformExpression transform)
    {
        WriteKeyword(
            transform.FunctionKeyword,
            transform.FunctionKeyword.Kind == SyntaxKind.ConvertKeyword
                ? Keyword.ConvertUpper
                : Keyword.TranslateUpper);
        AppendPlain('(');
        AppendExpression(transform.Expression);
        AppendPlain(' ');
        WriteKeyword(transform.UsingKeyword, Keyword.UsingUpper);
        AppendPlain(' ');
        WriteIdentifier(transform.Name);
        AppendPlain(')');
    }

    private void WritePosition(PositionExpression position)
    {
        WriteKeyword(position.PositionKeyword, Keyword.PositionUpper);
        AppendPlain('(');
        AppendExpression(position.Needle);
        AppendPlain(' ');
        WriteKeyword(position.InKeyword, Keyword.InUpper);
        AppendExpression(position.Haystack);
        AppendPlain(')');
    }

    private void WriteInterval(IntervalLiteralExpression interval)
    {
        WriteKeyword(interval.IntervalKeyword, Keyword.IntervalUpper);
        if (interval.Sign is { } sign)
        {
            AppendLeadingTrivia(sign);
            EnsureContentIndent();
            AppendSpaceIfNeeded();
            AppendPlain(sign.Kind == SyntaxKind.MinusToken ? '-' : '+');
        }

        AppendLeadingTrivia(interval.Literal);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendLiteral(interval.Literal);
        AppendPlain(' ');
        WriteIntervalField(interval.Qualifier.Start);
        if (interval.Qualifier.ToKeyword is { } to && interval.Qualifier.End is { } end)
        {
            AppendPlain(' ');
            WriteKeyword(to, Keyword.ToUpper);
            AppendPlain(' ');
            WriteIntervalField(end);
        }
    }

    private void WriteIntervalField(IntervalField field)
    {
        WriteTypeName(field.Name);
        if (field.OpenParen is null || field.Precision is not { } precision || field.CloseParen is null)
        {
            return;
        }

        AppendPlain('(');
        AppendPlain(precision.TextOf(_source));
        if (field.Scale is { } scale)
        {
            AppendPlain(',');
            AppendPlain(' ');
            AppendPlain(scale.TextOf(_source));
        }

        AppendPlain(')');
    }

    private static string DatetimeKeywordText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.DateKeyword => Keyword.DateUpper,
        SyntaxKind.TimeKeyword => Keyword.TimeUpper,
        SyntaxKind.TimestampKeyword => Keyword.TimestampUpper,
        _ => throw new InvalidOperationException($"Unknown datetime keyword {kind}"),
    };

    private void WriteDataType(DataType type)
    {
        WriteTypeName(type.Name);
        foreach (var part in type.NameTail)
        {
            WriteTypeName(part);
        }

        if (type.OpenParen is not null && type.Precision is { } precision && type.CloseParen is not null)
        {
            AppendPlain('(');
            AppendPlain(precision.TextOf(_source));
            if (type.Scale is { } scale)
            {
                AppendPlain(',');
                AppendPlain(' ');
                AppendPlain(scale.TextOf(_source));
            }

            AppendPlain(')');
        }

        if (type.WithKeyword is { } withKeyword
            && type.TimeKeyword is { } timeKeyword
            && type.Zone is { } zone)
        {
            AppendPlain(' ');
            WriteKeyword(withKeyword, Keyword.WithUpper);
            AppendPlain(' ');
            WriteKeyword(timeKeyword, Keyword.TimeUpper);
            AppendPlain(' ');
            WriteKeyword(zone, Keyword.ZoneUpper);
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
        _ => throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}"),
    };

    private void AppendIdentifier(SyntaxToken token)
    {
        var span = token.TextOf(_source);
        if (span.Length > 0 && span[0] == IdentifierQuote)
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
