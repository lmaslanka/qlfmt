using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteModule(ModuleDefinition module)
    {
        WriteKw(module.ModuleKeyword);
        if (module.Name is { } name)
        {
            WriteIdentifier(name);
        }
        WriteKw(module.NamesKeyword);
        WriteKw(module.AreKeyword);
        if (module.CharacterSet is { } characterSet)
        {
            WriteQualifiedName(characterSet);
        }

        WriteKw(module.LanguageKeyword);
        WriteKw(module.Language);
        WriteKw(module.SchemaKeyword);
        if (module.SchemaName is { } schemaName)
        {
            WriteQualifiedName(schemaName);
        }

        WriteKw(module.AuthorizationKeyword);
        WriteKw(module.Authorization);
        WriteKw(module.PathKeyword);
        if (module.Path.Count > 0)
        {
            WriteNamePath(module.Path);
        }

        foreach (var content in module.Contents)
        {
            AppendLine();
            Write(content);
            if (content is not ModuleProcedure)
            {
                AppendPlain(';');
            }
        }
    }

    private void WriteModuleProcedure(ModuleProcedure procedure)
    {
        WriteKw(procedure.ProcedureKeyword);
        WriteIdentifier(procedure.Name);
        if (procedure.OpenParen is { } open)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            WriteParameters(procedure.Parameters);
            AppendPlain(')');
        }

        if (procedure.Semicolon is not null)
        {
            AppendPlain(';');
        }

        AppendLine();
        Write(procedure.Statement);
        if (procedure.StatementSemicolon is not null)
        {
            AppendPlain(';');
        }
    }

    private void WriteEmbedded(EmbeddedSqlStatement statement)
    {
        WriteKw(statement.ExecKeyword);
        WriteKw(statement.SqlKeyword);
        if (statement.Statement is { } inner)
        {
            AppendPlain(' ');
            Write(inner);
        }

        WriteKw(statement.EndKeyword);
        if (statement.Minus is not null)
        {
            AppendPlain('-');
        }

        WriteKw(statement.EndExec);
        if (statement.Semicolon is not null)
        {
            AppendPlain(';');
        }
    }

    private void WriteWhenever(WheneverStatement statement)
    {
        WriteKw(statement.WheneverKeyword);
        WriteKw(statement.NotKeyword);
        WriteKw(statement.Condition);
        WriteKw(statement.Action);
        WriteKw(statement.GoKeyword);
        WriteKw(statement.ToKeyword);
        WriteKw(statement.Target);
    }
}
