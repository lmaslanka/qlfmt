using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteCreatePropertyGraph(CreatePropertyGraphStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.PropertyKeyword);
        WriteKw(statement.GraphKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.VertexKeyword);
        WriteInsertedKeyword(Keyword.TablesUpper);
        AppendPlain(' ');
        AppendPlain('(');
        WriteGraphTableElements(statement.Vertices);
        AppendPlain(')');
        WriteKw(statement.EdgeKeyword);
        if (statement.EdgeKeyword is not null)
        {
            WriteInsertedKeyword(Keyword.TablesUpper);
            AppendPlain(' ');
            AppendPlain('(');
            WriteGraphTableElements(statement.Edges);
            AppendPlain(')');
        }
    }

    private void WriteDropPropertyGraph(DropPropertyGraphStatement statement)
    {
        WriteKw(statement.DropKeyword);
        WriteKw(statement.PropertyKeyword);
        WriteKw(statement.GraphKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.Behavior);
    }

    private void WriteGraphTableElements(IReadOnlyList<GraphTableElement> elements)
    {
        for (var i = 0; i < elements.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteGraphTableElement(elements[i]);
        }
    }

    private void WriteGraphTableElement(GraphTableElement element)
    {
        WriteQualifiedName(element.Name);
        WriteKw(element.KeyKeyword);
        if (element.KeyColumns.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('(');
            WriteCommaIdentifiers(element.KeyColumns);
            AppendPlain(')');
        }

        WriteKw(element.SourceKeyword);
        if (element.SourceColumns.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('(');
            WriteCommaIdentifiers(element.SourceColumns);
            AppendPlain(')');
        }

        if (element.SourceReference.Count > 0)
        {
            AppendPlain(' ');
            WriteQualifiedName(element.SourceReference);
        }

        WriteKw(element.DestinationKeyword);
        if (element.DestinationColumns.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('(');
            WriteCommaIdentifiers(element.DestinationColumns);
            AppendPlain(')');
        }

        if (element.DestinationReference.Count > 0)
        {
            AppendPlain(' ');
            WriteQualifiedName(element.DestinationReference);
        }

        if (element.Labels.Count > 0)
        {
            AppendPlain(' ');
            WriteCommaIdentifiers(element.Labels);
        }

        if (element.Properties.Count > 0)
        {
            AppendPlain(' ');
            WriteCommaIdentifiers(element.Properties);
        }
    }

    private void AppendGraphTable(GraphTable table)
    {
        WriteTypeName(table.GraphTableKeyword);
        AppendLeadingTrivia(table.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteQualifiedName(table.GraphName);
        WriteKw(table.MatchKeyword);
        for (var i = 0; i < table.Pattern.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteGraphElement(table.Pattern[i]);
        }

        WriteKw(table.WhereKeyword);
        if (table.Where is { } where)
        {
            AppendPlain(' ');
            AppendExpression(where);
        }

        WriteKw(table.ColumnsKeyword);
        if (table.Columns.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('(');
            AppendCommaExpressions(table.Columns);
            AppendPlain(')');
        }

        AppendPlain(')');
        AppendTableAlias(table.AsKeyword, table.Alias);
    }

    private void WriteGraphElement(GraphElement element)
    {
        if (element.Group.Count > 0)
        {
            AppendPlain('(');
            for (var i = 0; i < element.Group.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(' ');
                }

                WriteGraphElement(element.Group[i]);
            }

            AppendPlain(')');
            WriteKw(element.Quantifier);
            return;
        }

        if (element.Edge)
        {
            AppendPlain('-');
            AppendPlain('[');
        }
        else
        {
            AppendPlain('(');
        }

        WriteKw(element.Variable);
        foreach (var label in element.Labels)
        {
            AppendPlain(' ');
            WriteLexical(label);
        }

        if (element.Properties.Count > 0)
        {
            AppendPlain(' ');
            AppendPlain('{');
            for (var i = 0; i < element.Properties.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                var property = element.Properties[i];
                WriteIdentifier(property.Name);
                AppendPlain(':');
                AppendExpression(property.Value);
            }

            AppendPlain('}');
        }

        if (element.Where is { } where)
        {
            AppendPlain(' ');
            AppendExpression(where);
        }

        AppendPlain(element.Edge ? ']' : ')');
        if (element.Edge)
        {
            AppendPlain('-');
            AppendPlain('>');
        }

        WriteKw(element.Quantifier);
    }
}
