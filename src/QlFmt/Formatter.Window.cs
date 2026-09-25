using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteWindowClause(WindowClause window)
    {
        AppendLeadingTrivia(window.WindowKeyword);
        AppendLine();
        WriteTypeName(window.WindowKeyword);
        AppendPlain(' ');
        for (var i = 0; i < window.Windows.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteWindowDefinition(window.Windows[i]);
        }
    }

    private void WriteWindowDefinition(WindowDefinition definition)
    {
        WriteIdentifier(definition.Name);
        AppendPlain(' ');
        WriteKeyword(definition.AsKeyword, Keyword.AsUpper);
        WriteWindowSpecification(definition.Specification);
    }

    private void WriteWindowSpecification(WindowSpecification specification)
    {
        if (specification.OverKeyword is { } overKeyword)
        {
            AppendPlain(' ');
            WriteTypeName(overKeyword);
        }

        if (specification.OpenParen is not { } openParen || specification.CloseParen is null)
        {
            if (specification.Name is { } name)
            {
                AppendPlain(' ');
                WriteIdentifier(name);
            }

            return;
        }

        AppendLeadingTrivia(openParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        var wrote = false;
        if (specification.Name is { } windowName)
        {
            WriteIdentifier(windowName);
            wrote = true;
        }

        if (specification.PartitionKeyword is { } partitionKeyword
            && specification.PartitionByKeyword is { } partitionByKeyword)
        {
            if (wrote)
            {
                AppendPlain(' ');
            }

            WriteTypeName(partitionKeyword);
            AppendPlain(' ');
            WriteKeyword(partitionByKeyword, Keyword.ByUpper);
            AppendPlain(' ');
            AppendCommaExpressions(specification.PartitionBy);
            wrote = true;
        }

        if (specification.OrderBy is { } orderBy)
        {
            WriteOrderByClause(orderBy, newLine: false);
            wrote = true;
        }

        if (specification.Frame is { } frame)
        {
            if (wrote)
            {
                AppendPlain(' ');
            }

            WriteWindowFrame(frame);
        }

        AppendPlain(')');
    }

    private void WriteWindowFrame(WindowFrame frame)
    {
        WriteTypeName(frame.Units);
        AppendPlain(' ');
        if (frame.BetweenKeyword is { } betweenKeyword && frame.AndKeyword is { } andKeyword && frame.End is { } end)
        {
            WriteKeyword(betweenKeyword, Keyword.BetweenUpper);
            AppendPlain(' ');
            WriteWindowFrameBound(frame.Start);
            AppendPlain(' ');
            WriteKeyword(andKeyword, Keyword.AndUpper);
            AppendPlain(' ');
            WriteWindowFrameBound(end);
        }
        else
        {
            WriteWindowFrameBound(frame.Start);
        }

        if (frame.ExcludeKeyword is { } excludeKeyword && frame.Exclusion is { } exclusion)
        {
            AppendPlain(' ');
            WriteTypeName(excludeKeyword);
            AppendPlain(' ');
            WriteTypeName(exclusion);
            if (frame.ExclusionTail is { } tail)
            {
                AppendPlain(' ');
                WriteTypeName(tail);
            }
        }
    }

    private void WriteWindowFrameBound(WindowFrameBound bound)
    {
        if (bound.UnboundedKeyword is { } unbounded)
        {
            WriteTypeName(unbounded);
            AppendPlain(' ');
            WriteTypeName(bound.Endpoint);
            return;
        }

        if (bound.CurrentKeyword is { } current)
        {
            WriteTypeName(current);
            AppendPlain(' ');
            WriteTypeName(bound.Endpoint);
            return;
        }

        if (bound.Offset is { } offset)
        {
            AppendExpression(offset);
            AppendPlain(' ');
        }

        WriteTypeName(bound.Endpoint);
    }
}
