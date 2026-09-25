using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteCompound(CompoundStatement statement)
    {
        WriteLabelPrefix(statement.Label, statement.Colon);
        WriteKw(statement.BeginKeyword);
        WriteKw(statement.NotKeyword);
        WriteKw(statement.AtomicKeyword);
        WriteStatementBody(statement.Statements);
        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndLabel);
    }

    private void WriteDeclareVariable(DeclareVariableStatement statement)
    {
        WriteKw(statement.DeclareKeyword);
        WriteCommaIdentifiers(statement.Names);
        WriteDataType(statement.Type);
        if (statement.Default is { } defaultClause)
        {
            WriteDefaultClause(defaultClause);
        }
    }

    private void WriteDeclareCondition(DeclareConditionStatement statement)
    {
        WriteKw(statement.DeclareKeyword);
        WriteIdentifier(statement.Name);
        WriteKw(statement.ConditionKeyword);
        WriteKw(statement.ForKeyword);
        WriteKw(statement.SqlStateKeyword);
        WriteKw(statement.ValueKeyword);
        WriteKw(statement.State);
    }

    private void WriteDeclareHandler(DeclareHandlerStatement statement)
    {
        WriteKw(statement.DeclareKeyword);
        WriteKw(statement.HandlerType);
        WriteKw(statement.HandlerKeyword);
        WriteKw(statement.ForKeyword);
        for (var i = 0; i < statement.Conditions.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteConditionValue(statement.Conditions[i]);
        }

        AppendLine();
        Write(statement.Action);
    }

    private void WriteConditionValue(ConditionValue condition)
    {
        WriteKw(condition.SqlStateKeyword);
        WriteKw(condition.ValueKeyword);
        WriteKw(condition.State);
        WriteKw(condition.NotKeyword);
        if (condition.SqlStateKeyword is null)
        {
            WriteKw(condition.Name);
        }
    }

    private void WriteIfStatement(IfStatement statement)
    {
        WriteKw(statement.IfKeyword);
        AppendPlain(' ');
        AppendExpression(statement.Condition);
        WriteKw(statement.ThenKeyword);
        WriteStatementBody(statement.ThenStatements);
        foreach (var elseIf in statement.ElseIfs)
        {
            AppendLine();
            WriteKw(elseIf.ElseIfKeyword);
            AppendPlain(' ');
            AppendExpression(elseIf.Condition);
            WriteKw(elseIf.ThenKeyword);
            WriteStatementBody(elseIf.Statements);
        }

        if (statement.ElseKeyword is { } elseKeyword)
        {
            AppendLine();
            WriteKw(elseKeyword);
            WriteStatementBody(statement.ElseStatements);
        }

        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndIfKeyword);
    }

    private void WriteCaseStatement(CaseStatement statement)
    {
        WriteKw(statement.CaseKeyword);
        if (statement.Operand is { } operand)
        {
            AppendPlain(' ');
            AppendExpression(operand);
        }

        foreach (var when in statement.Whens)
        {
            AppendLine();
            WriteKw(when.WhenKeyword);
            AppendPlain(' ');
            AppendExpression(when.Operand);
            WriteKw(when.ThenKeyword);
            WriteStatementBody(when.Statements);
        }

        if (statement.ElseKeyword is { } elseKeyword)
        {
            AppendLine();
            WriteKw(elseKeyword);
            WriteStatementBody(statement.ElseStatements);
        }

        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndCaseKeyword);
    }

    private void WriteLoop(LoopStatement statement)
    {
        WriteLabelPrefix(statement.Label, statement.Colon);
        WriteKw(statement.LoopKeyword);
        WriteStatementBody(statement.Statements);
        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndLoopKeyword);
        WriteKw(statement.EndLabel);
    }

    private void WriteWhile(WhileStatement statement)
    {
        WriteLabelPrefix(statement.Label, statement.Colon);
        WriteKw(statement.WhileKeyword);
        AppendPlain(' ');
        AppendExpression(statement.Condition);
        WriteKw(statement.DoKeyword);
        WriteStatementBody(statement.Statements);
        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndWhileKeyword);
        WriteKw(statement.EndLabel);
    }

    private void WriteRepeat(RepeatStatement statement)
    {
        WriteLabelPrefix(statement.Label, statement.Colon);
        WriteKw(statement.RepeatKeyword);
        WriteStatementBody(statement.Statements);
        AppendLine();
        WriteKw(statement.UntilKeyword);
        AppendPlain(' ');
        AppendExpression(statement.Condition);
        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndRepeatKeyword);
        WriteKw(statement.EndLabel);
    }

    private void WriteFor(ForStatement statement)
    {
        WriteLabelPrefix(statement.Label, statement.Colon);
        WriteKw(statement.ForKeyword);
        WriteIdentifier(statement.Variable);
        WriteKw(statement.AsKeyword);
        WriteKw(statement.CursorName);
        WriteKw(statement.CursorKeyword);
        WriteKw(statement.CursorForKeyword);
        AppendLine();
        Write(statement.CursorQuery);
        WriteKw(statement.DoKeyword);
        WriteStatementBody(statement.Statements);
        AppendLine();
        WriteKw(statement.EndKeyword);
        WriteKw(statement.EndForKeyword);
        WriteKw(statement.EndLabel);
    }

    private void WriteCreateTrigger(CreateTriggerStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.TriggerKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.ActionTime);
        WriteKw(statement.OfKeyword);
        WriteKw(statement.Event);
        WriteKw(statement.ColumnsOfKeyword);
        if (statement.Columns is { } columns)
        {
            WriteCommaIdentifiers(columns);
        }

        WriteKw(statement.OnKeyword);
        WriteQualifiedName(statement.TableName);
        WriteKw(statement.ReferencingKeyword);
        foreach (var transition in statement.Transitions)
        {
            WriteKw(transition.Kind);
            WriteKw(transition.RowOrTable);
            WriteKw(transition.AsKeyword);
            WriteIdentifier(transition.Name);
        }

        WriteKw(statement.ForKeyword);
        WriteKw(statement.EachKeyword);
        WriteKw(statement.Granularity);
        WriteKw(statement.WhenKeyword);
        if (statement.WhenOpen is { } open && statement.When is { } when)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            AppendExpression(when);
            AppendPlain(')');
        }

        AppendLine();
        Write(statement.Body);
    }

    private void WriteCreateRoutine(CreateRoutineStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.KindPrefix);
        WriteKw(statement.RoutineKeyword);
        WriteQualifiedName(statement.Name);
        AppendLeadingTrivia(statement.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteParameters(statement.Parameters);
        AppendPlain(')');
        WriteKw(statement.ReturnsKeyword);
        if (statement.ReturnsType is { } returnsType)
        {
            WriteDataType(returnsType);
        }

        WriteKw(statement.ReturnsAsKeyword);
        WriteKw(statement.ReturnsLocator);
        WriteKw(statement.ForKeyword);
        if (statement.ForType is { } forType)
        {
            WriteQualifiedName(forType);
        }

        WriteCharacteristics(statement.Characteristics);
        WriteKw(statement.SqlKeyword);
        if (statement.Body is { } body)
        {
            AppendLine();
            Write(body);
        }

        WriteKw(statement.ExternalKeyword);
        WriteKw(statement.NameKeyword);
        WriteKw(statement.ExternalName);
    }

    private void WriteAlterRoutine(AlterRoutineStatement statement)
    {
        WriteKw(statement.AlterKeyword);
        WriteRoutineDesignator(statement.Designator);
        WriteCharacteristics(statement.Characteristics);
        WriteKw(statement.SqlKeyword);
        if (statement.Body is { } body)
        {
            AppendLine();
            Write(body);
        }

        WriteKw(statement.ExternalKeyword);
        WriteKw(statement.NameKeyword);
        WriteKw(statement.ExternalName);
    }

    private void WriteCall(CallStatement statement)
    {
        WriteKw(statement.CallKeyword);
        WriteQualifiedName(statement.Name);
        AppendLeadingTrivia(statement.OpenParen);
        AppendPlain('(');
        AppendCommaExpressions(statement.Arguments);
        AppendPlain(')');
    }

    private void WriteLabelPrefix(SyntaxToken? label, SyntaxToken? colon)
    {
        if (label is not { } name)
        {
            return;
        }

        WriteIdentifier(name);
        if (colon is not null)
        {
            AppendPlain(':');
        }

        AppendPlain(' ');
    }

    private void WriteStatementBody(IReadOnlyList<Query> statements)
    {
        _indent++;
        foreach (var statement in statements)
        {
            AppendLine();
            Write(statement);
            AppendPlain(';');
        }

        _indent--;
    }
}
