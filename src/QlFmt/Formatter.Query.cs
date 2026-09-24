using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
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
            case DirectSqlScript script:
                WriteDirectSqlScript(script);
                break;
            default:
                throw new InvalidOperationException($"Unknown query {query.GetType().Name}");
        }
    }

    private void WriteDirectSqlScript(DirectSqlScript script)
    {
        for (var i = 0; i < script.Statements.Count; i++)
        {
            Write(script.Statements[i]);
            if (i >= script.Semicolons.Count)
            {
                continue;
            }

            AppendLeadingTrivia(script.Semicolons[i]);
            AppendPlain(';');
            if (i < script.Statements.Count - 1)
            {
                AppendLine();
            }
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
        WriteSubqueryBody(query, closeParen);
    }

    private void AppendSubqueryCall(SyntaxToken openParen, Query query, SyntaxToken closeParen)
    {
        AppendLeadingTrivia(openParen);
        AppendPlain('(');
        WriteSubqueryBody(query, closeParen);
    }

    private void WriteSubqueryBody(Query query, SyntaxToken closeParen)
    {
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

}
