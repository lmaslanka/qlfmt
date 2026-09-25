using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void AppendMatchRecognize(MatchRecognizeTable table)
    {
        AppendTable(table.Input);
        AppendPlain(' ');
        WriteTypeName(table.MatchRecognizeKeyword);
        AppendLeadingTrivia(table.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        if (table.PartitionKeyword is { } partitionKeyword && table.PartitionBy is { } partitionBy)
        {
            AppendLine();
            WriteTypeName(partitionKeyword);
            WriteKw(partitionBy);
            AppendPlain(' ');
            AppendCommaExpressions(table.PartitionByList);
        }

        if (table.OrderBy is { } orderBy)
        {
            AppendLine();
            WriteOrderByClause(orderBy, newLine: false);
        }

        if (table.MeasuresKeyword is { } measuresKeyword)
        {
            AppendLine();
            WriteTypeName(measuresKeyword);
            AppendPlain(' ');
            for (var i = 0; i < table.Measures.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                var measure = table.Measures[i];
                WriteKw(measure.Semantics);
                AppendExpression(measure.Expression);
                WriteKw(measure.AsKeyword);
                WriteIdentifier(measure.Name);
            }
        }

        WriteKw(table.RowsKind);
        WriteKw(table.RowOrRows);
        WriteKw(table.PerKeyword);
        WriteKw(table.MatchKeyword);
        WriteKw(table.EmptyHandling);
        WriteKw(table.AfterKeyword);
        WriteKw(table.SkipKeyword);
        WriteKw(table.SkipTo);
        WriteKw(table.SkipPosition);
        WriteKw(table.SkipTarget);
        AppendLine();
        WriteKw(table.PatternKeyword);
        AppendLeadingTrivia(table.PatternOpen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteRowPattern(table.Pattern);
        AppendPlain(')');
        if (table.SubsetKeyword is { } subsetKeyword)
        {
            AppendLine();
            WriteTypeName(subsetKeyword);
            AppendPlain(' ');
            for (var i = 0; i < table.Subsets.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                var subset = table.Subsets[i];
                WriteIdentifier(subset.Name);
                AppendPlain(' ');
                AppendPlain('=');
                AppendPlain(' ');
                AppendPlain('(');
                WriteCommaIdentifiers(subset.Variables);
                AppendPlain(')');
            }
        }

        AppendLine();
        WriteKw(table.DefineKeyword);
        AppendPlain(' ');
        for (var i = 0; i < table.Definitions.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            var definition = table.Definitions[i];
            WriteIdentifier(definition.Name);
            WriteKw(definition.AsKeyword);
            AppendPlain(' ');
            AppendExpression(definition.Condition);
        }

        _indent--;
        AppendLeadingTrivia(table.CloseParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
    }

    private void WriteRowPattern(RowPattern pattern)
    {
        switch (pattern)
        {
            case PatternPrimary primary:
                WriteLexical(primary.Token);
                break;
            case PatternConcatenation concatenation:
                for (var i = 0; i < concatenation.Factors.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(' ');
                    }

                    WriteRowPattern(concatenation.Factors[i]);
                }

                break;
            case PatternAlternation alternation:
                for (var i = 0; i < alternation.Terms.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(' ');
                        AppendPlain('|');
                        AppendPlain(' ');
                    }

                    WriteRowPattern(alternation.Terms[i]);
                }

                break;
            case PatternQuantified quantified:
                WriteRowPattern(quantified.Primary);
                WriteKw(quantified.Quantifier);
                WriteKw(quantified.Low);
                if (quantified.Comma is not null)
                {
                    AppendPlain(',');
                }

                WriteKw(quantified.High);
                if (quantified.CloseBrace is not null)
                {
                    AppendPlain('}');
                }

                WriteKw(quantified.Reluctant);
                break;
            case PatternGroup group:
                AppendLeadingTrivia(group.OpenParen);
                AppendPlain('(');
                WriteRowPattern(group.Pattern);
                AppendPlain(')');
                break;
            case PatternExclusion exclusion:
                AppendLeadingTrivia(exclusion.OpenBrace);
                AppendPlain('{');
                AppendPlain('-');
                WriteRowPattern(exclusion.Pattern);
                AppendPlain('-');
                AppendPlain('}');
                break;
            case PatternPermute permute:
                WriteKw(permute.PermuteKeyword);
                AppendLeadingTrivia(permute.OpenParen);
                AppendPlain('(');
                for (var i = 0; i < permute.Patterns.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    WriteRowPattern(permute.Patterns[i]);
                }

                AppendPlain(')');
                break;
            default:
                throw new InvalidOperationException($"Unknown row pattern {pattern.GetType().Name}");
        }
    }

    private void AppendPtfTable(PtfTable table)
    {
        WriteIdentifier(table.Name);
        AppendLeadingTrivia(table.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        for (var i = 0; i < table.Arguments.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WritePtfArgument(table.Arguments[i]);
        }

        AppendPlain(')');
        AppendTableAlias(table.AsKeyword, table.Alias);
    }

    private void WritePtfArgument(PtfArgument argument)
    {
        WriteKw(argument.KindKeyword);
        if (argument.Query is { } query)
        {
            AppendPlain('(');
            _indent++;
            AppendLine();
            Write(query);
            _indent--;
            AppendLine();
            EnsureContentIndent();
            AppendPlain(')');
        }

        if (argument.Values.Count > 0)
        {
            AppendPlain('(');
            AppendCommaExpressions(argument.Values);
            AppendPlain(')');
        }

        if (argument.Scalar is { } scalar)
        {
            AppendExpression(scalar);
        }

        WriteKw(argument.AsKeyword);
        WriteKw(argument.Alias);
        WriteKw(argument.PartitionKeyword);
        WriteKw(argument.PartitionBy);
        if (argument.PartitionByList.Count > 0)
        {
            AppendPlain(' ');
            AppendCommaExpressions(argument.PartitionByList);
        }

        if (argument.OrderBy is { } orderBy)
        {
            WriteOrderByClause(orderBy, newLine: false);
        }

        WriteKw(argument.PruneKeyword);
        if (argument.Prune is { } prune)
        {
            AppendPlain(' ');
            AppendExpression(prune);
        }

        WriteKw(argument.SemanticsKeyword);
        WriteKw(argument.CopartitionKeyword);
        if (argument.Names.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('(');
            WriteCommaIdentifiers(argument.Names);
            AppendPlain(')');
        }
    }
}
