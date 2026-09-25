using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteGrantPrivilege(GrantPrivilegeStatement statement)
    {
        WriteKw(statement.GrantKeyword);
        WritePrivilegeActions(statement.Actions);
        WriteKw(statement.OnKeyword);
        WritePrivilegeObject(statement.Object);
        WriteKw(statement.ToKeyword);
        WriteCommaIdentifiers(statement.Grantees);
        WriteGrantOption(statement.HierarchyOption);
        WriteGrantOption(statement.GrantOption);
        WriteKw(statement.GrantedKeyword);
        WriteKw(statement.ByKeyword);
        WriteKw(statement.Grantor);
    }

    private void WriteGrantRole(GrantRoleStatement statement)
    {
        WriteKw(statement.GrantKeyword);
        WriteCommaIdentifiers(statement.Roles);
        WriteKw(statement.ToKeyword);
        WriteCommaIdentifiers(statement.Grantees);
        WriteGrantOption(statement.AdminOption);
        WriteKw(statement.GrantedKeyword);
        WriteKw(statement.ByKeyword);
        WriteKw(statement.Grantor);
    }

    private void WriteRevokePrivilege(RevokePrivilegeStatement statement)
    {
        WriteKw(statement.RevokeKeyword);
        WriteRevokeOption(statement.Option);
        WritePrivilegeActions(statement.Actions);
        WriteKw(statement.OnKeyword);
        WritePrivilegeObject(statement.Object);
        WriteKw(statement.FromKeyword);
        WriteCommaIdentifiers(statement.Grantees);
        WriteKw(statement.GrantedKeyword);
        WriteKw(statement.ByKeyword);
        WriteKw(statement.Grantor);
        WriteKw(statement.Behavior);
    }

    private void WriteRevokeRole(RevokeRoleStatement statement)
    {
        WriteKw(statement.RevokeKeyword);
        WriteRevokeOption(statement.Option);
        WriteCommaIdentifiers(statement.Roles);
        WriteKw(statement.FromKeyword);
        WriteCommaIdentifiers(statement.Grantees);
        WriteKw(statement.GrantedKeyword);
        WriteKw(statement.ByKeyword);
        WriteKw(statement.Grantor);
        WriteKw(statement.Behavior);
    }

    private void WriteGrantOption(GrantOptionClause? option)
    {
        if (option is null)
        {
            return;
        }

        WriteKw(option.WithKeyword);
        WriteKw(option.Kind);
        WriteKw(option.OptionKeyword);
    }

    private void WriteRevokeOption(RevokeOptionClause? option)
    {
        if (option is null)
        {
            return;
        }

        WriteKw(option.Kind);
        WriteKw(option.OptionKeyword);
        WriteKw(option.ForKeyword);
    }

    private void WritePrivilegeActions(IReadOnlyList<PrivilegeAction> actions)
    {
        for (var i = 0; i < actions.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            var action = actions[i];
            WriteKw(action.Name);
            WriteKw(action.PrivilegesKeyword);
            if (action.OpenParen is { } open && action.Columns is { } columns)
            {
                AppendLeadingTrivia(open);
                AppendSpaceIfNeeded();
                AppendPlain('(');
                WriteCommaIdentifiers(columns);
                AppendPlain(')');
            }
        }
    }

    private void WritePrivilegeObject(PrivilegeObject obj)
    {
        WriteKw(obj.Kind);
        WriteKw(obj.SetKeyword);
        if (obj.Name is { } name)
        {
            WriteQualifiedName(name);
        }

        if (obj.Routine is { } routine)
        {
            WriteRoutineDesignator(routine);
        }
    }

    private void WriteCreateRole(CreateRoleStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.RoleKeyword);
        WriteIdentifier(statement.Name);
        WriteKw(statement.WithKeyword);
        WriteKw(statement.AdminKeyword);
        WriteKw(statement.Grantor);
    }

    private void WriteStartTransaction(StartTransactionStatement statement)
    {
        WriteKw(statement.StartKeyword);
        WriteKw(statement.TransactionKeyword);
        WriteTransactionModes(statement.Modes);
    }

    private void WriteSetTransaction(SetTransactionStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.LocalKeyword);
        WriteKw(statement.TransactionKeyword);
        WriteTransactionModes(statement.Modes);
    }

    private void WriteTransactionModes(IReadOnlyList<TransactionMode> modes)
    {
        for (var i = 0; i < modes.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
            }

            var mode = modes[i];
            WriteKw(mode.IsolationKeyword);
            WriteKw(mode.LevelKeyword);
            WriteKw(mode.Access);
            WriteKw(mode.Level);
            WriteKw(mode.ReadKeyword);
            WriteKw(mode.DiagnosticsKeyword);
            WriteKw(mode.SizeKeyword);
            if (mode.Size is { } size)
            {
                AppendPlain(' ');
                AppendExpression(size);
            }
        }
    }

    private void WriteCommit(CommitStatement statement)
    {
        WriteKw(statement.CommitKeyword);
        WriteKw(statement.WorkKeyword);
        WriteKw(statement.AndKeyword);
        WriteKw(statement.NoKeyword);
        WriteKw(statement.ChainKeyword);
    }

    private void WriteRollback(RollbackStatement statement)
    {
        WriteKw(statement.RollbackKeyword);
        WriteKw(statement.WorkKeyword);
        WriteKw(statement.AndKeyword);
        WriteKw(statement.NoKeyword);
        WriteKw(statement.ChainKeyword);
        WriteKw(statement.ToKeyword);
        WriteKw(statement.SavepointKeyword);
        WriteKw(statement.Savepoint);
    }

    private void WriteSetConstraints(SetConstraintsStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.ConstraintsKeyword);
        WriteKw(statement.AllKeyword);
        if (statement.Names is { } names)
        {
            WriteNamePath(names);
        }

        WriteKw(statement.Mode);
    }

    private void WriteSetSessionAuthorization(SetSessionAuthorizationStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.SessionKeyword);
        WriteKw(statement.AuthorizationKeyword);
        WriteLexical(statement.Value);
    }

    private void WriteSetSessionCharacteristics(SetSessionCharacteristicsStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.SessionKeyword);
        WriteKw(statement.CharacteristicsKeyword);
        WriteKw(statement.AsKeyword);
        WriteTransactionModes(statement.Modes);
    }

    private void WriteSetNames(SetNamesStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.NamesKeyword);
        WriteLexical(statement.Value);
        WriteKw(statement.CollateKeyword);
        if (statement.Collation is { } collation)
        {
            WriteQualifiedName(collation);
        }
    }

    private void WriteSetCharacterSet(SetCharacterSetStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.CharacterKeyword);
        WriteKw(statement.CharacterSetKeyword);
        WriteLexical(statement.Value);
    }

    private void WriteSetCollation(SetCollationStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.CollationKeyword);
        WriteKw(statement.Value);
        if (statement.Name is { } name)
        {
            WriteQualifiedName(name);
        }
    }

    private void WriteSetTimeZone(SetTimeZoneStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.TimeKeyword);
        WriteKw(statement.ZoneKeyword);
        WriteKw(statement.LocalKeyword);
        if (statement.Value is { } value)
        {
            AppendPlain(' ');
            AppendExpression(value);
        }
    }

    private void WriteSetCatalog(SetCatalogStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.CatalogKeyword);
        WriteKw(statement.Value);
        if (statement.Name is { } name)
        {
            WriteQualifiedName(name);
        }
    }

    private void WriteSetSchema(SetSchemaStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.SchemaKeyword);
        WriteKw(statement.Value);
        if (statement.Name is { } name)
        {
            WriteQualifiedName(name);
        }
    }

    private void WriteSetPath(SetPathStatement statement)
    {
        WriteKw(statement.SetKeyword);
        WriteKw(statement.PathKeyword);
        WriteKw(statement.Value);
        if (statement.Names is { } names)
        {
            WriteNamePath(names);
        }
    }

    private void WriteConnect(ConnectStatement statement)
    {
        WriteKw(statement.ConnectKeyword);
        WriteKw(statement.ToKeyword);
        WriteLexical(statement.Target);
        WriteKw(statement.AsKeyword);
        WriteKw(statement.ConnectionName);
        WriteKw(statement.UserKeyword);
        WriteKw(statement.User);
    }
}
