using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteInsert(InsertStatement insert)
    {
        WriteKeyword(insert.InsertKeyword, Keyword.InsertUpper);
        AppendPlain(' ');
        WriteKeyword(insert.IntoKeyword, Keyword.IntoUpper);
        AppendPlain(' ');
        WriteQualifiedName(insert.TableName);
        if (insert.Columns is { } columns
            && insert.ColumnOpenParen is { } open
            && insert.ColumnCloseParen is { } close)
        {
            WriteInsertColumns(open, columns, close);
        }

        if (insert.Override is { } overrideClause)
        {
            WriteOverride(overrideClause);
        }

        AppendLine();
        if (insert.DefaultKeyword is { } defaultKeyword && insert.ValuesKeyword is { } valuesKeyword)
        {
            WriteTypeName(defaultKeyword);
            AppendPlain(' ');
            WriteKeyword(valuesKeyword, Keyword.ValuesUpper);
            return;
        }

        Write(insert.Query!);
    }

    private void WriteInsertColumns(
        SyntaxToken open,
        IReadOnlyList<SyntaxToken> columns,
        SyntaxToken close)
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

    private void WriteOverride(OverrideClause overrideClause)
    {
        AppendPlain(' ');
        WriteTypeName(overrideClause.OverridingKeyword);
        AppendPlain(' ');
        WriteTypeName(overrideClause.Kind);
        AppendPlain(' ');
        WriteTypeName(overrideClause.ValueKeyword);
    }

    private void WriteUpdate(UpdateStatement update)
    {
        WriteKeyword(update.UpdateKeyword, Keyword.UpdateUpper);
        AppendPlain(' ');
        WriteTargetTable(update.Target);
        if (update.Portion is { } portion)
        {
            WritePortion(portion);
        }

        AppendPlain(' ');
        WriteTypeName(update.SetKeyword);
        AppendPlain(' ');
        WriteSetClauses(update.Assignments);
        WriteWhereOrPositioned(update.Where, update.Positioned);
    }

    private void WriteDelete(DeleteStatement delete)
    {
        WriteTypeName(delete.DeleteKeyword);
        AppendPlain(' ');
        WriteKeyword(delete.FromKeyword, Keyword.FromUpper);
        AppendPlain(' ');
        WriteTargetTable(delete.Target);
        if (delete.Portion is { } portion)
        {
            WritePortion(portion);
        }

        WriteWhereOrPositioned(delete.Where, delete.Positioned);
    }

    private void WriteTruncate(TruncateStatement truncate)
    {
        WriteTypeName(truncate.TruncateKeyword);
        AppendPlain(' ');
        WriteTypeName(truncate.TableKeyword);
        AppendPlain(' ');
        WriteQualifiedName(truncate.TableName);
        if (truncate.RestartKeyword is { } restart && truncate.IdentityKeyword is { } identity)
        {
            AppendPlain(' ');
            WriteTypeName(restart);
            AppendPlain(' ');
            WriteTypeName(identity);
        }
    }

    private void WriteSetAssignment(SetAssignmentStatement assignment)
    {
        WriteTypeName(assignment.SetKeyword);
        AppendPlain(' ');
        if (assignment.OpenParen is { } open)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            for (var i = 0; i < assignment.Targets.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteQualifiedName(assignment.Targets[i]);
            }

            AppendPlain(')');
        }
        else
        {
            WriteQualifiedName(assignment.Targets[0]);
        }

        AppendPlain(' ');
        AppendPlain('=');
        AppendPlain(' ');
        AppendExpression(assignment.Value);
    }

    private void WriteMerge(MergeStatement merge)
    {
        WriteTypeName(merge.MergeKeyword);
        AppendPlain(' ');
        WriteKeyword(merge.IntoKeyword, Keyword.IntoUpper);
        AppendPlain(' ');
        WriteTargetTable(merge.Target);
        AppendLine();
        WriteKeyword(merge.UsingKeyword, Keyword.UsingUpper);
        AppendPlain(' ');
        AppendTable(merge.Source);
        foreach (var join in merge.SourceJoins)
        {
            AppendLine();
            AppendJoin(join);
        }

        AppendLine();
        WriteKeyword(merge.OnKeyword, Keyword.OnUpper);
        AppendPlain(' ');
        AppendExpression(merge.Condition);
        foreach (var when in merge.Whens)
        {
            WriteMergeWhen(when);
        }
    }

    private void WriteMergeWhen(MergeWhenClause when)
    {
        AppendLeadingTrivia(when.WhenKeyword);
        AppendLine();
        WriteKeyword(when.WhenKeyword, Keyword.WhenUpper);
        AppendPlain(' ');
        if (when.NotKeyword is { } notKeyword)
        {
            WriteKeyword(notKeyword, Keyword.NotUpper);
            AppendPlain(' ');
        }

        WriteTypeName(when.MatchedKeyword);
        if (when.ByKeyword is { } byKeyword && when.ByKind is { } byKind)
        {
            AppendPlain(' ');
            WriteKeyword(byKeyword, Keyword.ByUpper);
            AppendPlain(' ');
            WriteTypeName(byKind);
        }

        if (when.AndKeyword is { } andKeyword && when.Condition is { } condition)
        {
            AppendPlain(' ');
            WriteKeyword(andKeyword, Keyword.AndUpper);
            AppendPlain(' ');
            AppendExpression(condition);
        }

        AppendPlain(' ');
        WriteKeyword(when.ThenKeyword, Keyword.ThenUpper);
        AppendPlain(' ');
        WriteMergeAction(when.Action);
    }

    private void WriteMergeAction(MergeAction action)
    {
        switch (action)
        {
            case MergeUpdateAction update:
                WriteKeyword(update.UpdateKeyword, Keyword.UpdateUpper);
                AppendPlain(' ');
                WriteTypeName(update.SetKeyword);
                AppendPlain(' ');
                WriteSetClauses(update.Assignments);
                break;
            case MergeDeleteAction delete:
                WriteTypeName(delete.DeleteKeyword);
                break;
            case MergeInsertAction insert:
                WriteMergeInsert(insert);
                break;
            default:
                throw new InvalidOperationException($"Unknown merge action {action.GetType().Name}");
        }
    }

    private void WriteMergeInsert(MergeInsertAction insert)
    {
        WriteKeyword(insert.InsertKeyword, Keyword.InsertUpper);
        if (insert.Columns is { } columns)
        {
            AppendPlain(' ');
            AppendPlain('(');
            WriteCommaIdentifiers(columns);
            AppendPlain(')');
        }

        if (insert.Override is { } overrideClause)
        {
            WriteOverride(overrideClause);
        }

        AppendPlain(' ');
        WriteKeyword(insert.ValuesKeyword, Keyword.ValuesUpper);
        AppendLeadingTrivia(insert.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        AppendCommaExpressions(insert.Values);
        AppendPlain(')');
    }

    private void WriteTargetTable(TargetTable target)
    {
        if (target.OnlyKeyword is { } onlyKeyword)
        {
            WriteKeyword(onlyKeyword, Keyword.OnlyUpper);
            if (target.OpenParen is { } open)
            {
                AppendLeadingTrivia(open);
                AppendSpaceIfNeeded();
                AppendPlain('(');
            }

            WriteQualifiedName(target.NameParts);
            if (target.CloseParen is not null)
            {
                AppendPlain(')');
            }
        }
        else
        {
            WriteQualifiedName(target.NameParts);
        }

        AppendTableAlias(target.AsKeyword, target.Alias);
    }

    private void WritePortion(PortionClause portion)
    {
        AppendPlain(' ');
        WriteKeyword(portion.ForKeyword, Keyword.ForUpper);
        AppendPlain(' ');
        WriteTypeName(portion.PortionKeyword);
        AppendPlain(' ');
        WriteKeyword(portion.OfKeyword, Keyword.OfUpper);
        AppendPlain(' ');
        WriteIdentifier(portion.Name);
        AppendPlain(' ');
        WriteKeyword(portion.FromKeyword, Keyword.FromUpper);
        AppendPlain(' ');
        AppendExpression(portion.Start);
        AppendPlain(' ');
        WriteTypeName(portion.ToKeyword);
        AppendPlain(' ');
        AppendExpression(portion.End);
    }

    private void WriteWhereOrPositioned(WhereClause? where, PositionedClause? positioned)
    {
        if (where is not null)
        {
            AppendLeadingTrivia(where.WhereKeyword);
            AppendLine();
            WriteKeyword(where.WhereKeyword, Keyword.WhereUpper);
            AppendPlain(' ');
            AppendWhereExpression(where.Expression);
            return;
        }

        if (positioned is not { } clause)
        {
            return;
        }

        AppendLeadingTrivia(clause.WhereKeyword);
        AppendLine();
        WriteKeyword(clause.WhereKeyword, Keyword.WhereUpper);
        AppendPlain(' ');
        WriteTypeName(clause.CurrentKeyword);
        AppendPlain(' ');
        WriteKeyword(clause.OfKeyword, Keyword.OfUpper);
        AppendPlain(' ');
        WriteIdentifier(clause.Cursor);
    }

    private void WriteSetClauses(IReadOnlyList<SetClause> assignments)
    {
        for (var i = 0; i < assignments.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteSetClause(assignments[i]);
        }
    }

    private void WriteSetClause(SetClause clause)
    {
        if (clause.OpenParen is { } open)
        {
            AppendLeadingTrivia(open);
            EnsureContentIndent();
            AppendSpaceIfNeeded();
            AppendPlain('(');
            for (var i = 0; i < clause.Targets.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteSetTarget(clause.Targets[i]);
            }

            AppendPlain(')');
        }
        else
        {
            WriteSetTarget(clause.Targets[0]);
        }

        AppendPlain(' ');
        AppendPlain('=');
        AppendPlain(' ');
        if (clause.DefaultKeyword is { } defaultKeyword)
        {
            WriteTypeName(defaultKeyword);
            return;
        }

        AppendExpression(clause.Value!);
    }

    private void WriteSetTarget(SetTarget target)
    {
        WriteIdentifier(target.Name);
        if (target.OpenBracket is not { } open || target.Index is not { } index)
        {
            return;
        }

        AppendLeadingTrivia(open);
        AppendPlain('[');
        AppendExpression(index);
        AppendPlain(']');
    }
}
