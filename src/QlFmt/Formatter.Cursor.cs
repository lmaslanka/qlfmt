using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteDeclareCursor(DeclareCursorStatement statement)
    {
        WriteKw(statement.DeclareKeyword);
        WriteIdentifier(statement.Name);
        WriteKw(statement.Sensitivity);
        WriteKw(statement.NoScroll);
        WriteKw(statement.Scroll);
        WriteKw(statement.CursorKeyword);
        WriteKw(statement.HoldWith);
        WriteKw(statement.Hold);
        WriteKw(statement.ReturnWith);
        WriteKw(statement.ReturnKeyword);
        WriteKw(statement.ForKeyword);
        AppendLine();
        Write(statement.Query);
    }

    private void WriteFetchStatement(FetchStatement statement)
    {
        WriteKw(statement.FetchKeyword);
        WriteKw(statement.Orientation);
        if (statement.Offset is { } offset)
        {
            AppendPlain(' ');
            AppendExpression(offset);
        }

        WriteKw(statement.FromKeyword);
        WriteIdentifier(statement.Cursor);
        WriteKw(statement.IntoKeyword);
        WriteIntoList(statement.Targets);
    }

    private void WriteAllocateCursor(AllocateCursorStatement statement)
    {
        WriteKw(statement.AllocateKeyword);
        WriteIdentifier(statement.Name);
        WriteKw(statement.Sensitivity);
        WriteKw(statement.NoScroll);
        WriteKw(statement.Scroll);
        WriteKw(statement.CursorKeyword);
        WriteKw(statement.ForKeyword);
        WriteIdentifier(statement.Statement);
    }

    private void WritePrepare(PrepareStatement statement)
    {
        WriteKw(statement.PrepareKeyword);
        WriteKw(statement.Scope);
        WriteIdentifier(statement.Name);
        WriteKw(statement.FromKeyword);
        WriteLexical(statement.Source);
    }

    private void WriteExecute(ExecuteStatement statement)
    {
        WriteKw(statement.ExecuteKeyword);
        WriteKw(statement.Scope);
        WriteIdentifier(statement.Name);
        WriteKw(statement.IntoKeyword);
        WriteKw(statement.IntoSqlKeyword);
        WriteKw(statement.IntoDescriptorKeyword);
        WriteKw(statement.IntoDescriptorScope);
        WriteKw(statement.IntoDescriptor);
        if (statement.Targets is { } targets)
        {
            WriteIntoList(targets);
        }

        WriteKw(statement.UsingKeyword);
        WriteKw(statement.UsingSqlKeyword);
        WriteKw(statement.UsingDescriptorKeyword);
        WriteKw(statement.UsingDescriptorScope);
        WriteKw(statement.UsingDescriptor);
        if (statement.Arguments is { } arguments)
        {
            WriteIntoList(arguments);
        }
    }

    private void WriteDescribe(DescribeStatement statement)
    {
        WriteKw(statement.DescribeKeyword);
        WriteKw(statement.InputOrOutput);
        WriteKw(statement.Scope);
        WriteIdentifier(statement.Name);
        WriteKw(statement.UsingOrInto);
        WriteKw(statement.SqlKeyword);
        WriteKw(statement.DescriptorKeyword);
        WriteKw(statement.DescriptorScope);
        WriteIdentifier(statement.Descriptor);
    }

    private void WriteDynamicDeclareCursor(DynamicDeclareCursorStatement statement)
    {
        WriteKw(statement.DeclareKeyword);
        WriteIdentifier(statement.Name);
        WriteKw(statement.Sensitivity);
        WriteKw(statement.NoScroll);
        WriteKw(statement.Scroll);
        WriteKw(statement.CursorKeyword);
        WriteKw(statement.HoldWith);
        WriteKw(statement.Hold);
        WriteKw(statement.ReturnWith);
        WriteKw(statement.ReturnKeyword);
        WriteKw(statement.ForKeyword);
        WriteIdentifier(statement.Statement);
    }

    private void WriteAllocateDescriptor(AllocateDescriptorStatement statement)
    {
        WriteKw(statement.AllocateKeyword);
        WriteKw(statement.SqlKeyword);
        WriteKw(statement.DescriptorKeyword);
        WriteKw(statement.Scope);
        WriteIdentifier(statement.Name);
        WriteKw(statement.WithKeyword);
        WriteKw(statement.MaxKeyword);
        WriteKw(statement.Occurrences);
    }

    private void WriteGetDiagnostics(GetDiagnosticsStatement statement)
    {
        WriteKw(statement.GetKeyword);
        WriteKw(statement.DiagnosticsKeyword);
        WriteKw(statement.ConditionKeyword);
        if (statement.ConditionNumber is { } number)
        {
            AppendPlain(' ');
            AppendExpression(number);
        }

        for (var i = 0; i < statement.Items.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            var item = statement.Items[i];
            WriteLexical(item.Target);
            AppendPlain(' ');
            AppendPlain('=');
            AppendPlain(' ');
            WriteKw(item.Name);
        }
    }

    private void WriteSignal(SignalStatement statement)
    {
        WriteKw(statement.SignalKeyword);
        WriteKw(statement.SqlStateKeyword);
        WriteKw(statement.ValueKeyword);
        WriteKw(statement.State);
        WriteKw(statement.Condition);
        WriteKw(statement.SetKeyword);
        WriteSignalItems(statement.Items);
    }

    private void WriteResignal(ResignalStatement statement)
    {
        WriteKw(statement.ResignalKeyword);
        WriteKw(statement.SqlStateKeyword);
        WriteKw(statement.ValueKeyword);
        WriteKw(statement.State);
        WriteKw(statement.Condition);
        WriteKw(statement.SetKeyword);
        WriteSignalItems(statement.Items);
    }

    private void WriteSignalItems(IReadOnlyList<SignalInformation> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            var item = items[i];
            WriteKw(item.Name);
            AppendPlain(' ');
            AppendPlain('=');
            AppendPlain(' ');
            AppendExpression(item.Value);
        }
    }

    private void WriteIntoList(IReadOnlyList<SyntaxToken> targets)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteIntoTarget(targets[i]);
        }
    }
}
