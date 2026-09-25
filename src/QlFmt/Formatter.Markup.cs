using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteMarkupCall(MarkupCallExpression call)
    {
        WriteTypeName(call.Name);
        AppendLeadingTrivia(call.OpenParen);
        AppendPlain('(');
        WriteMarkupBody(call.Arguments, call.Clauses, call.Query, call.OrderBy, call.Columns, call.Returning);
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

        if (call.Over is { } over)
        {
            WriteWindowSpecification(over);
        }
    }

    private void AppendMarkupTable(MarkupTable table)
    {
        WriteTypeName(table.Name);
        AppendLeadingTrivia(table.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteMarkupBody(table.Arguments, table.Clauses, query: null, orderBy: null, table.Columns, returning: null);
        AppendPlain(')');
        AppendTableAlias(table.AsKeyword, table.Alias);
    }

    private void WriteMarkupBody(
        IReadOnlyList<Expression> arguments,
        IReadOnlyList<SyntaxToken> clauses,
        Query? query,
        OrderByClause? orderBy,
        IReadOnlyList<MarkupColumn> columns,
        DataType? returning)
    {
        var wrote = false;
        if (query is { } nested)
        {
            Write(nested);
            wrote = true;
        }

        for (var i = 0; i < arguments.Count; i++)
        {
            if (wrote)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            AppendExpression(arguments[i]);
            wrote = true;
        }

        foreach (var clause in clauses)
        {
            WriteLexical(clause);
            wrote = true;
        }

        if (orderBy is { } order)
        {
            WriteOrderByClause(order, newLine: false);
        }

        if (columns.Count > 0)
        {
            if (wrote)
            {
                AppendPlain(' ');
            }

            WriteMarkupColumns(columns);
        }

        if (returning is { } type)
        {
            AppendPlain(' ');
            WriteDataType(type);
        }
    }

    private void WriteMarkupColumns(IReadOnlyList<MarkupColumn> columns)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteMarkupColumn(columns[i]);
        }
    }

    private void WriteMarkupColumn(MarkupColumn column)
    {
        WriteIdentifier(column.Name);
        if (column.Type is { } type)
        {
            WriteDataType(type);
        }

        if (column.Path is { } path)
        {
            AppendPlain(' ');
            AppendExpression(path);
        }

        if (column.Default is { } defaultValue)
        {
            AppendPlain(' ');
            AppendExpression(defaultValue);
        }

        WriteKw(column.ForKeyword);
        WriteKw(column.OrdinalityKeyword);
        if (column.Nested.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('(');
            WriteMarkupColumns(column.Nested);
            AppendPlain(')');
        }
    }

    private void WriteJsonAccessor(JsonAccessorExpression accessor)
    {
        AppendExpression(accessor.Target);
        AppendLeadingTrivia(accessor.OpenBracket);
        AppendPlain('[');
        AppendExpression(accessor.Index);
        AppendPlain(']');
    }

    private void WriteMdarrayConstructor(MdarrayConstructorExpression expression)
    {
        WriteTypeName(expression.MdarrayKeyword);
        if (expression.OpenBracket is { } openBracket)
        {
            AppendLeadingTrivia(openBracket);
            AppendPlain('[');
            for (var i = 0; i < expression.Dimensions.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteMdarrayDimension(expression.Dimensions[i]);
            }

            AppendPlain(']');
        }

        if (expression.Query is { } query)
        {
            AppendSubqueryCall(expression.OpenParen, query, expression.CloseParen);
            return;
        }

        AppendLeadingTrivia(expression.OpenParen);
        AppendPlain('(');
        AppendCommaExpressions(expression.Elements);
        AppendPlain(')');
    }

    private void WriteMdarraySlice(MdarraySliceExpression expression)
    {
        AppendExpression(expression.Target);
        AppendLeadingTrivia(expression.OpenBracket);
        AppendPlain('[');
        for (var i = 0; i < expression.Axes.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteMdarrayAxis(expression.Axes[i]);
        }

        AppendPlain(']');
    }

    private void WriteMdarrayAxis(MdarrayAxis axis)
    {
        if (axis.Star is { } star)
        {
            AppendLeadingTrivia(star);
            AppendPlain('*');
            return;
        }

        if (axis.Lower is { } lower)
        {
            AppendExpression(lower);
        }

        if (axis.Colon is not null)
        {
            AppendPlain(':');
        }

        if (axis.Upper is { } upper)
        {
            AppendExpression(upper);
        }
    }

    private void WriteMdarrayAggregate(MdarrayAggregateExpression expression)
    {
        WriteTypeName(expression.Name);
        AppendLeadingTrivia(expression.OpenParen);
        AppendPlain('(');
        AppendExpression(expression.Argument);
        AppendPlain(')');
    }
}
