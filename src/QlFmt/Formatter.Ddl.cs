using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void WriteDropNamed(
        SyntaxToken drop,
        SyntaxToken kind,
        IReadOnlyList<SyntaxToken> name,
        SyntaxToken? behavior)
    {
        WriteKw(drop);
        WriteKw(kind);
        WriteQualifiedName(name);
        WriteKw(behavior);
    }

    private void WriteSchemaDefinition(SchemaDefinition schema)
    {
        WriteKw(schema.CreateKeyword);
        WriteKw(schema.SchemaKeyword);
        if (schema.Name is { } name)
        {
            WriteQualifiedName(name);
        }

        WriteKw(schema.AuthorizationKeyword);
        WriteKw(schema.Authorization);
        WriteKw(schema.DefaultKeyword);
        WriteKw(schema.CharacterKeyword);
        WriteKw(schema.SetKeyword);
        if (schema.CharacterSet is { } characterSet)
        {
            WriteQualifiedName(characterSet);
        }

        WriteKw(schema.PathKeyword);
        if (schema.Path is { } path)
        {
            WriteNamePath(path);
        }

        foreach (var table in schema.Elements)
        {
            AppendLine();
            WriteCreateTable(table);
        }
    }

    private void WriteAlterSchema(AlterSchemaStatement statement)
    {
        WriteKw(statement.AlterKeyword);
        WriteKw(statement.SchemaKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.RenameKeyword);
        WriteKw(statement.ToKeyword);
        WriteQualifiedName(statement.NewName);
    }

    private void WriteDropSchema(DropSchemaStatement statement) =>
        WriteDropNamed(statement.DropKeyword, statement.SchemaKeyword, statement.Name, statement.Behavior);

    private void WriteCreateTable(CreateTableStatement table)
    {
        WriteKw(table.CreateKeyword);
        WriteKw(table.Scope);
        WriteKw(table.TemporaryKeyword);
        WriteKw(table.TableKeyword);
        WriteQualifiedName(table.Name);
        WriteKw(table.OfKeyword);
        if (table.TypeName is { } typeName)
        {
            WriteQualifiedName(typeName);
        }

        WriteKw(table.UnderKeyword);
        if (table.Supertable is { } supertable)
        {
            WriteQualifiedName(supertable);
        }

        if (table.OpenParen is { } open && table.Elements is { } elements && table.CloseParen is { } close)
        {
            WriteParenthesizedElements(open, close, elements, WriteTableElement);
        }

        if (table.AsColumnsOpen is { } asOpen && table.AsColumns is { } asColumns && table.AsColumnsClose is { } asClose)
        {
            AppendLeadingTrivia(asOpen);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            WriteCommaIdentifiers(asColumns);
            AppendPlain(')');
        }

        WriteKw(table.AsKeyword);
        if (table.Query is { } query)
        {
            AppendLine();
            Write(query);
        }

        WriteKw(table.WithKeyword);
        WriteKw(table.NoKeyword);
        WriteKw(table.DataKeyword);
        WriteKw(table.OnKeyword);
        WriteKw(table.CommitKeyword);
        WriteKw(table.CommitAction);
        WriteKw(table.RowsKeyword);
        WriteKw(table.SystemKeyword);
        WriteKw(table.VersioningKeyword);
    }

    private void WriteParenthesizedElements<T>(
        SyntaxToken open,
        SyntaxToken close,
        IReadOnlyList<T> elements,
        Action<T> write)
    {
        AppendLeadingTrivia(open);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        for (var i = 0; i < elements.Count; i++)
        {
            AppendLine();
            write(elements[i]);
            if (i < elements.Count - 1)
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

    private void WriteTableElement(TableElement element)
    {
        switch (element)
        {
            case ColumnDefinition column:
                WriteColumnDefinition(column);
                break;
            case TableConstraint constraint:
                WriteTableConstraint(constraint);
                break;
            case LikeClause like:
                WriteLikeClause(like);
                break;
            case RefIsClause refIs:
                WriteKw(refIs.RefKeyword);
                WriteKw(refIs.IsKeyword);
                WriteIdentifier(refIs.Name);
                WriteKw(refIs.Generation);
                WriteKw(refIs.GeneratedKeyword);
                break;
            case PeriodDefinition period:
                WritePeriodDefinition(period);
                break;
            default:
                throw new InvalidOperationException($"Unknown table element {element.GetType().Name}");
        }
    }

    private void WriteColumnDefinition(ColumnDefinition column)
    {
        WriteIdentifier(column.Name);
        if (column.Type is { } type)
        {
            WriteDataType(type);
        }

        WriteKw(column.WithKeyword);
        WriteKw(column.OptionsKeyword);
        if (column.Default is { } defaultClause)
        {
            WriteDefaultClause(defaultClause);
        }

        if (column.Identity is { } identity)
        {
            WriteIdentityColumn(identity);
        }

        if (column.Generated is { } generated)
        {
            WriteGeneratedColumn(generated);
        }

        WriteKw(column.CollateKeyword);
        if (column.Collation is { } collation)
        {
            WriteQualifiedName(collation);
        }

        WriteKw(column.ScopeKeyword);
        if (column.ScopeName is { } scopeName)
        {
            WriteQualifiedName(scopeName);
        }

        foreach (var constraint in column.Constraints)
        {
            AppendPlain(' ');
            WriteTableConstraint(constraint);
        }
    }

    private void WriteDefaultClause(DefaultClause clause)
    {
        WriteKw(clause.DefaultKeyword);
        AppendPlain(' ');
        AppendExpression(clause.Value);
    }

    private void WriteIdentityColumn(IdentityColumn identity)
    {
        WriteKw(identity.GeneratedKeyword);
        WriteKw(identity.AlwaysKeyword);
        WriteKw(identity.ByKeyword);
        WriteKw(identity.DefaultKeyword);
        WriteKw(identity.AsKeyword);
        WriteKw(identity.IdentityKeyword);
        if (identity.OpenParen is { } open)
        {
            AppendLeadingTrivia(open);
            AppendPlain('(');
            WriteSequenceOptions(identity.Options);
            AppendPlain(')');
        }
    }

    private void WriteGeneratedColumn(GeneratedColumn generated)
    {
        WriteKw(generated.GeneratedKeyword);
        WriteKw(generated.AlwaysKeyword);
        WriteKw(generated.ByKeyword);
        WriteKw(generated.DefaultKeyword);
        WriteKw(generated.AsKeyword);
        if (generated.OpenParen is { } open && generated.Expression is { } expression)
        {
            AppendLeadingTrivia(open);
            AppendPlain('(');
            AppendExpression(expression);
            AppendPlain(')');
        }

        WriteKw(generated.RowKeyword);
        WriteKw(generated.Bound);
    }

    private void WriteSequenceOptions(IReadOnlyList<SequenceOption> options)
    {
        for (var i = 0; i < options.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(' ');
            }

            WriteSequenceOption(options[i]);
        }
    }

    private void WriteSequenceOption(SequenceOption option)
    {
        WriteKw(option.NoKeyword);
        WriteKw(option.Name);
        WriteKw(option.WithKeyword);
        WriteKw(option.ByKeyword);
        if (option.Value is { } value)
        {
            AppendPlain(' ');
            AppendExpression(value);
        }
    }

    private void WriteTableConstraint(TableConstraint constraint)
    {
        WriteKw(constraint.ConstraintKeyword);
        WriteKw(constraint.ConstraintName);
        WriteKw(constraint.NotKeyword);
        WriteKw(constraint.NullKeyword);
        WriteKw(constraint.UniqueKeyword);
        WriteKw(constraint.NullsKeyword);
        WriteKw(constraint.NullsNotKeyword);
        WriteKw(constraint.DistinctKeyword);
        WriteKw(constraint.PrimaryKeyword);
        WriteKw(constraint.KeyKeyword);
        WriteKw(constraint.ForeignKeyword);
        if (constraint.ColumnsOpen is { } open && constraint.Columns is { } columns)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            WriteCommaIdentifiers(columns);
            AppendPlain(')');
        }

        WriteKw(constraint.ReferencesKeyword);
        if (constraint.ReferenceTable is { } referenceTable)
        {
            WriteQualifiedName(referenceTable);
        }

        if (constraint.ReferenceOpen is { } refOpen && constraint.ReferenceColumns is { } refColumns)
        {
            AppendLeadingTrivia(refOpen);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            WriteCommaIdentifiers(refColumns);
            AppendPlain(')');
        }

        WriteKw(constraint.MatchKeyword);
        WriteKw(constraint.MatchType);
        foreach (var action in constraint.Actions)
        {
            WriteReferentialAction(action);
        }

        WriteKw(constraint.CheckKeyword);
        if (constraint.CheckOpen is { } checkOpen && constraint.Check is { } check)
        {
            AppendLeadingTrivia(checkOpen);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            AppendExpression(check);
            AppendPlain(')');
        }

        WriteKw(constraint.DeferrableNotKeyword);
        WriteKw(constraint.DeferrableKeyword);
        WriteKw(constraint.InitiallyKeyword);
        WriteKw(constraint.InitiallyWhen);
    }

    private void WriteReferentialAction(ReferentialAction action)
    {
        WriteKw(action.OnKeyword);
        WriteKw(action.Event);
        WriteKw(action.NoKeyword);
        WriteKw(action.SetKeyword);
        WriteKw(action.Action);
    }

    private void WriteLikeClause(LikeClause like)
    {
        WriteKw(like.LikeKeyword);
        WriteQualifiedName(like.TableName);
        foreach (var option in like.Options)
        {
            WriteKw(option.KindKeyword);
            WriteKw(option.Option);
        }
    }

    private void WritePeriodDefinition(PeriodDefinition period)
    {
        WriteKw(period.PeriodKeyword);
        WriteKw(period.ForKeyword);
        WriteIdentifier(period.Name);
        AppendLeadingTrivia(period.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteIdentifier(period.StartColumn);
        AppendPlain(',');
        AppendPlain(' ');
        WriteIdentifier(period.EndColumn);
        AppendPlain(')');
    }

    private void WriteAlterTable(AlterTableStatement statement)
    {
        WriteKw(statement.AlterKeyword);
        WriteKw(statement.TableKeyword);
        WriteQualifiedName(statement.Name);
        WriteAlterTableAction(statement.Action);
    }

    private void WriteAlterTableAction(AlterTableAction action)
    {
        switch (action)
        {
            case AddColumnAction addColumn:
                WriteKw(addColumn.AddKeyword);
                WriteKw(addColumn.ColumnKeyword);
                WriteColumnDefinition(addColumn.Column);
                break;
            case DropColumnAction dropColumn:
                WriteKw(dropColumn.DropKeyword);
                WriteKw(dropColumn.ColumnKeyword);
                WriteIdentifier(dropColumn.Name);
                WriteKw(dropColumn.Behavior);
                break;
            case AddConstraintAction addConstraint:
                WriteKw(addConstraint.AddKeyword);
                WriteTableConstraint(addConstraint.Constraint);
                break;
            case DropConstraintAction dropConstraint:
                WriteKw(dropConstraint.DropKeyword);
                WriteKw(dropConstraint.ConstraintKeyword);
                WriteIdentifier(dropConstraint.Name);
                WriteKw(dropConstraint.Behavior);
                break;
            case AlterColumnAction alterColumn:
                WriteAlterColumn(alterColumn);
                break;
            case AddPeriodAction addPeriod:
                WriteKw(addPeriod.AddKeyword);
                WritePeriodDefinition(addPeriod.Period);
                break;
            case DropPeriodAction dropPeriod:
                WriteKw(dropPeriod.DropKeyword);
                WriteKw(dropPeriod.PeriodKeyword);
                WriteKw(dropPeriod.ForKeyword);
                WriteIdentifier(dropPeriod.Name);
                WriteKw(dropPeriod.Behavior);
                break;
            case SystemVersioningAction versioning:
                WriteKw(versioning.VerbKeyword);
                WriteKw(versioning.SystemKeyword);
                WriteKw(versioning.VersioningKeyword);
                break;
            default:
                throw new InvalidOperationException($"Unknown alter table action {action.GetType().Name}");
        }
    }

    private void WriteAlterColumn(AlterColumnAction action)
    {
        WriteKw(action.AlterKeyword);
        WriteKw(action.ColumnKeyword);
        WriteIdentifier(action.Name);
        WriteKw(action.SetKeyword);
        WriteKw(action.DropKeyword);
        WriteKw(action.AddKeyword);
        WriteKw(action.DataKeyword);
        WriteKw(action.TypeKeyword);
        if (action.DataType is { } dataType)
        {
            WriteDataType(dataType);
        }

        if (action.Default is { } defaultClause)
        {
            WriteDefaultClause(defaultClause);
        }

        WriteKw(action.NotKeyword);
        WriteKw(action.NullKeyword);
        WriteKw(action.ScopeKeyword);
        if (action.ScopeName is { } scopeName)
        {
            WriteQualifiedName(scopeName);
        }

        WriteKw(action.Behavior);
        WriteKw(action.RestartKeyword);
        WriteKw(action.WithKeyword);
        if (action.RestartValue is { } restart)
        {
            AppendPlain(' ');
            AppendExpression(restart);
        }

        WriteKw(action.GeneratedKeyword);
        WriteKw(action.AlwaysKeyword);
        WriteKw(action.ByKeyword);
        WriteKw(action.DefaultKeyword);
        WriteKw(action.IdentityKeyword);
        if (action.IdentityOption is { } option)
        {
            WriteSequenceOption(option);
        }
    }

    private void WriteCreateView(CreateViewStatement view)
    {
        WriteKw(view.CreateKeyword);
        WriteKw(view.RecursiveKeyword);
        WriteKw(view.ViewKeyword);
        WriteQualifiedName(view.Name);
        WriteKw(view.OfKeyword);
        if (view.TypeName is { } typeName)
        {
            WriteQualifiedName(typeName);
        }

        WriteKw(view.UnderKeyword);
        if (view.Superview is { } superview)
        {
            WriteQualifiedName(superview);
        }

        if (view.ColumnsOpen is { } open && view.Columns is { } columns)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            WriteCommaIdentifiers(columns);
            AppendPlain(')');
        }

        if (view.Elements is { } elements)
        {
            AppendPlain(' ');
            AppendPlain('(');
            _indent++;
            for (var i = 0; i < elements.Count; i++)
            {
                AppendLine();
                WriteTableElement(elements[i]);
                if (i < elements.Count - 1)
                {
                    AppendPlain(',');
                }
            }

            _indent--;
            AppendLine();
            EnsureContentIndent();
            AppendPlain(')');
        }

        WriteKw(view.AsKeyword);
        AppendLine();
        Write(view.Query);
        WriteKw(view.WithKeyword);
        WriteKw(view.Levels);
        WriteKw(view.CheckKeyword);
        WriteKw(view.OptionKeyword);
    }

    private void WriteAlterView(AlterViewStatement statement)
    {
        WriteKw(statement.AlterKeyword);
        WriteKw(statement.ViewKeyword);
        WriteQualifiedName(statement.Name);
        switch (statement.Action)
        {
            case ReplaceViewAction replace:
                WriteKw(replace.AsKeyword);
                AppendLine();
                Write(replace.Query);
                break;
            case AlterViewColumnAction column:
                WriteAlterColumn(column.Column);
                break;
            default:
                throw new InvalidOperationException($"Unknown alter view action {statement.Action.GetType().Name}");
        }
    }

    private void WriteCreateDomain(CreateDomainStatement domain)
    {
        WriteKw(domain.CreateKeyword);
        WriteKw(domain.DomainKeyword);
        WriteQualifiedName(domain.Name);
        WriteKw(domain.AsKeyword);
        WriteDataType(domain.Type);
        if (domain.Default is { } defaultClause)
        {
            WriteDefaultClause(defaultClause);
        }

        foreach (var constraint in domain.Constraints)
        {
            WriteDomainConstraint(constraint);
        }

        WriteKw(domain.CollateKeyword);
        if (domain.Collation is { } collation)
        {
            WriteQualifiedName(collation);
        }
    }

    private void WriteDomainConstraint(DomainConstraint constraint)
    {
        WriteKw(constraint.ConstraintKeyword);
        WriteKw(constraint.ConstraintName);
        WriteKw(constraint.CheckKeyword);
        AppendLeadingTrivia(constraint.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        AppendExpression(constraint.Check);
        AppendPlain(')');
        WriteKw(constraint.DeferrableNotKeyword);
        WriteKw(constraint.DeferrableKeyword);
        WriteKw(constraint.InitiallyKeyword);
        WriteKw(constraint.InitiallyWhen);
    }

    private void WriteAlterDomain(AlterDomainStatement statement)
    {
        WriteKw(statement.AlterKeyword);
        WriteKw(statement.DomainKeyword);
        WriteQualifiedName(statement.Name);
        switch (statement.Action)
        {
            case SetDomainDefaultAction setDefault:
                WriteKw(setDefault.SetKeyword);
                WriteDefaultClause(setDefault.Default);
                break;
            case DropDomainDefaultAction dropDefault:
                WriteKw(dropDefault.DropKeyword);
                WriteKw(dropDefault.DefaultKeyword);
                break;
            case AddDomainConstraintAction add:
                WriteKw(add.AddKeyword);
                WriteDomainConstraint(add.Constraint);
                break;
            case DropDomainConstraintAction drop:
                WriteKw(drop.DropKeyword);
                WriteKw(drop.ConstraintKeyword);
                WriteIdentifier(drop.Name);
                WriteKw(drop.Behavior);
                break;
            default:
                throw new InvalidOperationException($"Unknown alter domain action {statement.Action.GetType().Name}");
        }
    }

    private void WriteCreateType(CreateTypeStatement type)
    {
        WriteKw(type.CreateKeyword);
        WriteKw(type.TypeKeyword);
        WriteQualifiedName(type.Name);
        WriteKw(type.UnderKeyword);
        if (type.Supertype is { } supertype)
        {
            WriteQualifiedName(supertype);
        }

        WriteKw(type.AsKeyword);
        if (type.Representation is { } representation)
        {
            WriteDataType(representation);
        }

        if (type.MembersOpen is { } open && type.Attributes is { } attributes && type.MembersClose is { } close)
        {
            WriteParenthesizedElements(open, close, attributes, WriteAttribute);
        }

        WriteKw(type.NotInstantiableKeyword);
        WriteKw(type.InstantiableKeyword);
        WriteKw(type.NotFinalKeyword);
        WriteKw(type.FinalKeyword);
        if (type.Reference is { } reference)
        {
            WriteReferenceGeneration(reference);
        }

        foreach (var cast in type.Casts)
        {
            WriteTypeCastOption(cast);
        }

        foreach (var method in type.Methods)
        {
            AppendLine();
            WriteMethodSpecification(method);
        }
    }

    private void WriteAttribute(AttributeDefinition attribute)
    {
        WriteIdentifier(attribute.Name);
        WriteDataType(attribute.Type);
        WriteKw(attribute.ReferencesKeyword);
        WriteKw(attribute.AreKeyword);
        WriteKw(attribute.NotKeyword);
        WriteKw(attribute.CheckedKeyword);
        if (attribute.OnDelete is { } onDelete)
        {
            WriteReferentialAction(onDelete);
        }

        if (attribute.Default is { } defaultClause)
        {
            WriteDefaultClause(defaultClause);
        }

        WriteKw(attribute.CollateKeyword);
        if (attribute.Collation is { } collation)
        {
            WriteQualifiedName(collation);
        }
    }

    private void WriteReferenceGeneration(ReferenceGeneration reference)
    {
        WriteKw(reference.RefKeyword);
        WriteKw(reference.IsKeyword);
        WriteKw(reference.SystemKeyword);
        WriteKw(reference.GeneratedKeyword);
        WriteKw(reference.UsingKeyword);
        if (reference.PredefinedType is { } predefined)
        {
            WriteDataType(predefined);
        }

        WriteKw(reference.FromKeyword);
        if (reference.OpenParen is { } open && reference.Attributes is { } attributes)
        {
            AppendLeadingTrivia(open);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            WriteCommaIdentifiers(attributes);
            AppendPlain(')');
        }
    }

    private void WriteTypeCastOption(TypeCastOption option)
    {
        WriteKw(option.CastKeyword);
        AppendLeadingTrivia(option.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteIdentifier(option.Source);
        WriteKw(option.AsKeyword);
        WriteIdentifier(option.Target);
        AppendPlain(')');
        WriteKw(option.WithKeyword);
        WriteIdentifier(option.Function);
    }

    private void WriteMethodSpecification(MethodSpecification method)
    {
        WriteKw(method.OverridingKeyword);
        WriteKw(method.Kind);
        WriteKw(method.MethodKeyword);
        WriteIdentifier(method.Name);
        AppendLeadingTrivia(method.OpenParen);
        AppendPlain('(');
        WriteParameters(method.Parameters);
        AppendPlain(')');
        WriteKw(method.ReturnsKeyword);
        WriteDataType(method.ReturnsType);
        WriteKw(method.ReturnsAsKeyword);
        WriteKw(method.ReturnsLocator);
        WriteKw(method.SpecificKeyword);
        if (method.SpecificName is { } specific)
        {
            WriteQualifiedName(specific);
        }

        WriteKw(method.SelfResultKeyword);
        WriteKw(method.SelfResultAsKeyword);
        WriteKw(method.ResultKeyword);
        WriteKw(method.SelfLocatorKeyword);
        WriteKw(method.SelfLocatorAsKeyword);
        WriteKw(method.LocatorKeyword);
        WriteCharacteristics(method.Characteristics);
    }

    private void WriteParameters(IReadOnlyList<RoutineParameter> parameters)
    {
        for (var i = 0; i < parameters.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteParameter(parameters[i]);
        }
    }

    private void WriteParameter(RoutineParameter parameter)
    {
        WriteKw(parameter.Mode);
        WriteKw(parameter.Name);
        WriteDataType(parameter.Type);
        WriteKw(parameter.AsKeyword);
        WriteKw(parameter.LocatorKeyword);
        WriteKw(parameter.ResultKeyword);
    }

    private void WriteCharacteristics(IReadOnlyList<RoutineCharacteristic> characteristics)
    {
        foreach (var characteristic in characteristics)
        {
            WriteKw(characteristic.NotKeyword);
            WriteKw(characteristic.Name);
            foreach (var part in characteristic.Tail)
            {
                WriteKw(part);
            }
        }
    }

    private void WriteRoutineDesignator(RoutineDesignator designator)
    {
        WriteKw(designator.SpecificKeyword);
        WriteKw(designator.Kind);
        WriteKw(designator.RoutineType);
        WriteQualifiedName(designator.Name);
        WriteKw(designator.ForKeyword);
        if (designator.ForType is { } forType)
        {
            WriteQualifiedName(forType);
        }
    }

    private void WriteCreateOrdering(CreateOrderingStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.OrderingKeyword);
        WriteKw(statement.ForKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.EqualsKeyword);
        WriteKw(statement.OrderKeyword);
        WriteKw(statement.Form);
        WriteKw(statement.ByKeyword);
        WriteKw(statement.Category);
        WriteKw(statement.WithKeyword);
        if (statement.Routine is { } routine)
        {
            WriteRoutineDesignator(routine);
        }
    }

    private void WriteCreateCast(CreateCastStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.CastKeyword);
        AppendLeadingTrivia(statement.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteDataType(statement.Source);
        WriteKw(statement.AsKeyword);
        WriteDataType(statement.Target);
        AppendPlain(')');
        WriteKw(statement.WithKeyword);
        WriteRoutineDesignator(statement.Routine);
        WriteKw(statement.AssignmentAsKeyword);
        WriteKw(statement.AssignmentKeyword);
    }

    private void WriteCreateTransform(CreateTransformStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.TransformKeyword);
        WriteKw(statement.ForKeyword);
        WriteQualifiedName(statement.Name);
        foreach (var group in statement.Groups)
        {
            WriteIdentifier(group.Name);
            AppendLeadingTrivia(group.OpenParen);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            for (var i = 0; i < group.Elements.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                var element = group.Elements[i];
                WriteKw(element.Direction);
                WriteKw(element.SqlKeyword);
                WriteKw(element.WithKeyword);
                WriteRoutineDesignator(element.Routine);
            }

            AppendPlain(')');
        }
    }

    private void WriteCreateAssertion(CreateAssertionStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.AssertionKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.CheckKeyword);
        AppendLeadingTrivia(statement.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        AppendExpression(statement.Check);
        AppendPlain(')');
        WriteKw(statement.DeferrableNotKeyword);
        WriteKw(statement.DeferrableKeyword);
        WriteKw(statement.InitiallyKeyword);
        WriteKw(statement.InitiallyWhen);
    }

    private void WriteCreateCharacterSet(CreateCharacterSetStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.CharacterKeyword);
        WriteKw(statement.SetKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.AsKeyword);
        WriteKw(statement.GetKeyword);
        WriteQualifiedName(statement.Source);
        WriteKw(statement.CollateKeyword);
        if (statement.Collation is { } collation)
        {
            WriteQualifiedName(collation);
        }
    }

    private void WriteDropCharacterSet(DropCharacterSetStatement statement)
    {
        WriteKw(statement.DropKeyword);
        WriteKw(statement.CharacterKeyword);
        WriteKw(statement.SetKeyword);
        WriteQualifiedName(statement.Name);
    }

    private void WriteCreateCollation(CreateCollationStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.CollationKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.ForKeyword);
        WriteQualifiedName(statement.CharacterSet);
        WriteKw(statement.FromKeyword);
        WriteQualifiedName(statement.Source);
        WriteKw(statement.NoKeyword);
        WriteKw(statement.PadKeyword);
        WriteKw(statement.SpaceKeyword);
    }

    private void WriteCreateTranslation(CreateTranslationStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.TranslationKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.ForKeyword);
        WriteQualifiedName(statement.SourceCharacterSet);
        WriteKw(statement.ToKeyword);
        WriteQualifiedName(statement.TargetCharacterSet);
        WriteKw(statement.FromKeyword);
        if (statement.Existing is { } existing)
        {
            WriteQualifiedName(existing);
        }

        if (statement.Routine is { } routine)
        {
            WriteRoutineDesignator(routine);
        }
    }

    private void WriteCreateSequence(CreateSequenceStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.SequenceKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.AsKeyword);
        if (statement.DataType is { } dataType)
        {
            WriteDataType(dataType);
        }

        if (statement.Options.Count > 0)
        {
            AppendPlain(' ');
            WriteSequenceOptions(statement.Options);
        }
    }

    private void WriteCreateIndex(CreateIndexStatement statement)
    {
        WriteKw(statement.CreateKeyword);
        WriteKw(statement.UniqueKeyword);
        WriteKw(statement.ClusteredKeyword);
        WriteKw(statement.IndexKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.OnKeyword);
        WriteQualifiedName(statement.TableName);
        AppendLeadingTrivia(statement.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        for (var i = 0; i < statement.Columns.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            var column = statement.Columns[i];
            AppendExpression(column.Expression);
            WriteKw(column.Direction);
            WriteKw(column.NullsKeyword);
            WriteKw(column.NullsOrder);
        }

        AppendPlain(')');
        WriteKw(statement.WhereKeyword);
        if (statement.Where is { } where)
        {
            AppendPlain(' ');
            AppendExpression(where);
        }
    }

    private void WriteAlterIndex(AlterIndexStatement statement)
    {
        WriteKw(statement.AlterKeyword);
        WriteKw(statement.IndexKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.RenameKeyword);
        WriteKw(statement.ToKeyword);
        if (statement.NewName is { } newName)
        {
            WriteQualifiedName(newName);
        }

        WriteKw(statement.OnKeyword);
        if (statement.TableName is { } tableName)
        {
            WriteQualifiedName(tableName);
        }

        WriteKw(statement.Action);
    }

    private void WriteDropIndex(DropIndexStatement statement)
    {
        WriteKw(statement.DropKeyword);
        WriteKw(statement.IndexKeyword);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.OnKeyword);
        if (statement.TableName is { } tableName)
        {
            WriteQualifiedName(tableName);
        }

        WriteKw(statement.Behavior);
    }

    private void WriteCommentStatement(CommentStatement statement)
    {
        WriteKw(statement.CommentKeyword);
        WriteKw(statement.OnKeyword);
        WriteKw(statement.Kind);
        WriteQualifiedName(statement.Name);
        WriteKw(statement.IsKeyword);
        WriteLexical(statement.Value);
    }

    private void WriteNamePath(IReadOnlyList<IReadOnlyList<SyntaxToken>> path)
    {
        for (var i = 0; i < path.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteQualifiedName(path[i]);
        }
    }
}
