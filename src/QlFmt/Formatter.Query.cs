using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void Write(Query query)
    {
        switch (query)
        {
            case SelectStatement select:
                WriteSelect(select);
                break;
            case SetOperation setOp:
                WriteSetOperation(setOp);
                break;
            case ParenQuery paren:
                WriteParenQuery(paren);
                break;
            case WithQuery withQuery:
                WriteWithQuery(withQuery);
                break;
            case ValuesQuery values:
                WriteValues(values);
                break;
            case InsertStatement insert:
                WriteInsert(insert);
                break;
            case UpdateStatement update:
                WriteUpdate(update);
                break;
            case DeleteStatement delete:
                WriteDelete(delete);
                break;
            case MergeStatement merge:
                WriteMerge(merge);
                break;
            case TruncateStatement truncate:
                WriteTruncate(truncate);
                break;
            case SetAssignmentStatement assignment:
                WriteSetAssignment(assignment);
                break;
            case SchemaDefinition schema:
                WriteSchemaDefinition(schema);
                break;
            case AlterSchemaStatement alterSchema:
                WriteAlterSchema(alterSchema);
                break;
            case DropSchemaStatement dropSchema:
                WriteDropSchema(dropSchema);
                break;
            case CreateTableStatement createTable:
                WriteCreateTable(createTable);
                break;
            case AlterTableStatement alterTable:
                WriteAlterTable(alterTable);
                break;
            case DropTableStatement dropTable:
                WriteDropNamed(dropTable.DropKeyword, dropTable.TableKeyword, dropTable.Name, dropTable.Behavior);
                break;
            case CreateViewStatement createView:
                WriteCreateView(createView);
                break;
            case AlterViewStatement alterView:
                WriteAlterView(alterView);
                break;
            case DropViewStatement dropView:
                WriteDropNamed(dropView.DropKeyword, dropView.ViewKeyword, dropView.Name, dropView.Behavior);
                break;
            case CreateDomainStatement createDomain:
                WriteCreateDomain(createDomain);
                break;
            case AlterDomainStatement alterDomain:
                WriteAlterDomain(alterDomain);
                break;
            case DropDomainStatement dropDomain:
                WriteDropNamed(dropDomain.DropKeyword, dropDomain.DomainKeyword, dropDomain.Name, dropDomain.Behavior);
                break;
            case CreateTypeStatement createType:
                WriteCreateType(createType);
                break;
            case DropTypeStatement dropType:
                WriteDropNamed(dropType.DropKeyword, dropType.TypeKeyword, dropType.Name, dropType.Behavior);
                break;
            case CreateOrderingStatement createOrdering:
                WriteCreateOrdering(createOrdering);
                break;
            case CreateCastStatement createCast:
                WriteCreateCast(createCast);
                break;
            case CreateTransformStatement createTransform:
                WriteCreateTransform(createTransform);
                break;
            case CreateAssertionStatement createAssertion:
                WriteCreateAssertion(createAssertion);
                break;
            case DropAssertionStatement dropAssertion:
                WriteDropNamed(dropAssertion.DropKeyword, dropAssertion.AssertionKeyword, dropAssertion.Name, dropAssertion.Behavior);
                break;
            case CreateCharacterSetStatement createCharacterSet:
                WriteCreateCharacterSet(createCharacterSet);
                break;
            case DropCharacterSetStatement dropCharacterSet:
                WriteDropCharacterSet(dropCharacterSet);
                break;
            case CreateCollationStatement createCollation:
                WriteCreateCollation(createCollation);
                break;
            case DropCollationStatement dropCollation:
                WriteDropNamed(dropCollation.DropKeyword, dropCollation.CollationKeyword, dropCollation.Name, dropCollation.Behavior);
                break;
            case CreateTranslationStatement createTranslation:
                WriteCreateTranslation(createTranslation);
                break;
            case DropTranslationStatement dropTranslation:
                WriteKw(dropTranslation.DropKeyword);
                WriteKw(dropTranslation.TranslationKeyword);
                WriteQualifiedName(dropTranslation.Name);
                break;
            case CreateSequenceStatement createSequence:
                WriteCreateSequence(createSequence);
                break;
            case DropSequenceStatement dropSequence:
                WriteDropNamed(dropSequence.DropKeyword, dropSequence.SequenceKeyword, dropSequence.Name, dropSequence.Behavior);
                break;
            case CreateIndexStatement createIndex:
                WriteCreateIndex(createIndex);
                break;
            case AlterIndexStatement alterIndex:
                WriteAlterIndex(alterIndex);
                break;
            case DropIndexStatement dropIndex:
                WriteDropIndex(dropIndex);
                break;
            case CommentStatement comment:
                WriteCommentStatement(comment);
                break;
            case GrantPrivilegeStatement grantPrivilege:
                WriteGrantPrivilege(grantPrivilege);
                break;
            case GrantRoleStatement grantRole:
                WriteGrantRole(grantRole);
                break;
            case RevokePrivilegeStatement revokePrivilege:
                WriteRevokePrivilege(revokePrivilege);
                break;
            case RevokeRoleStatement revokeRole:
                WriteRevokeRole(revokeRole);
                break;
            case CreateRoleStatement createRole:
                WriteCreateRole(createRole);
                break;
            case DropRoleStatement dropRole:
                WriteKw(dropRole.DropKeyword);
                WriteKw(dropRole.RoleKeyword);
                WriteIdentifier(dropRole.Name);
                break;
            case SetRoleStatement setRole:
                WriteKw(setRole.SetKeyword);
                WriteKw(setRole.RoleKeyword);
                WriteLexical(setRole.Value);
                break;
            case StartTransactionStatement startTransaction:
                WriteStartTransaction(startTransaction);
                break;
            case SetTransactionStatement setTransaction:
                WriteSetTransaction(setTransaction);
                break;
            case CommitStatement commit:
                WriteCommit(commit);
                break;
            case RollbackStatement rollback:
                WriteRollback(rollback);
                break;
            case SavepointStatement savepoint:
                WriteKw(savepoint.SavepointKeyword);
                WriteIdentifier(savepoint.Name);
                break;
            case ReleaseSavepointStatement release:
                WriteKw(release.ReleaseKeyword);
                WriteKw(release.SavepointKeyword);
                WriteIdentifier(release.Name);
                break;
            case SetConstraintsStatement setConstraints:
                WriteSetConstraints(setConstraints);
                break;
            case SetSessionAuthorizationStatement setAuth:
                WriteSetSessionAuthorization(setAuth);
                break;
            case SetSessionCharacteristicsStatement setCharacteristics:
                WriteSetSessionCharacteristics(setCharacteristics);
                break;
            case SetNamesStatement setNames:
                WriteSetNames(setNames);
                break;
            case SetCharacterSetStatement setCharacterSet:
                WriteSetCharacterSet(setCharacterSet);
                break;
            case SetCollationStatement setCollation:
                WriteSetCollation(setCollation);
                break;
            case SetTimeZoneStatement setTimeZone:
                WriteSetTimeZone(setTimeZone);
                break;
            case SetCatalogStatement setCatalog:
                WriteSetCatalog(setCatalog);
                break;
            case SetSchemaStatement setSchema:
                WriteSetSchema(setSchema);
                break;
            case SetPathStatement setPath:
                WriteSetPath(setPath);
                break;
            case ConnectStatement connect:
                WriteConnect(connect);
                break;
            case DisconnectStatement disconnect:
                WriteKw(disconnect.DisconnectKeyword);
                WriteLexical(disconnect.Target);
                break;
            case SetConnectionStatement setConnection:
                WriteKw(setConnection.SetKeyword);
                WriteKw(setConnection.ConnectionKeyword);
                WriteLexical(setConnection.Target);
                break;
            case DeclareCursorStatement declareCursor:
                WriteDeclareCursor(declareCursor);
                break;
            case OpenStatement open:
                WriteKw(open.OpenKeyword);
                WriteIdentifier(open.Cursor);
                break;
            case FetchStatement fetch:
                WriteFetchStatement(fetch);
                break;
            case CloseStatement close:
                WriteKw(close.CloseKeyword);
                WriteIdentifier(close.Cursor);
                break;
            case AllocateCursorStatement allocateCursor:
                WriteAllocateCursor(allocateCursor);
                break;
            case DeallocateStatement deallocate:
                WriteKw(deallocate.DeallocateKeyword);
                WriteKw(deallocate.Kind);
                WriteIdentifier(deallocate.Name);
                break;
            case PrepareStatement prepare:
                WritePrepare(prepare);
                break;
            case ExecuteStatement execute:
                WriteExecute(execute);
                break;
            case ExecuteImmediateStatement executeImmediate:
                WriteKw(executeImmediate.ExecuteKeyword);
                WriteKw(executeImmediate.ImmediateKeyword);
                WriteLexical(executeImmediate.Source);
                break;
            case DescribeStatement describe:
                WriteDescribe(describe);
                break;
            case DynamicDeclareCursorStatement dynamicCursor:
                WriteDynamicDeclareCursor(dynamicCursor);
                break;
            case AllocateDescriptorStatement allocateDescriptor:
                WriteAllocateDescriptor(allocateDescriptor);
                break;
            case GetDiagnosticsStatement diagnostics:
                WriteGetDiagnostics(diagnostics);
                break;
            case SignalStatement signal:
                WriteSignal(signal);
                break;
            case ResignalStatement resignal:
                WriteResignal(resignal);
                break;
            case CompoundStatement compound:
                WriteCompound(compound);
                break;
            case DeclareVariableStatement declareVariable:
                WriteDeclareVariable(declareVariable);
                break;
            case DeclareConditionStatement declareCondition:
                WriteDeclareCondition(declareCondition);
                break;
            case DeclareHandlerStatement declareHandler:
                WriteDeclareHandler(declareHandler);
                break;
            case IfStatement ifStatement:
                WriteIfStatement(ifStatement);
                break;
            case CaseStatement caseStatement:
                WriteCaseStatement(caseStatement);
                break;
            case LoopStatement loop:
                WriteLoop(loop);
                break;
            case WhileStatement whileStatement:
                WriteWhile(whileStatement);
                break;
            case RepeatStatement repeat:
                WriteRepeat(repeat);
                break;
            case ForStatement forStatement:
                WriteFor(forStatement);
                break;
            case LeaveStatement leave:
                WriteKw(leave.LeaveKeyword);
                WriteIdentifier(leave.Label);
                break;
            case IterateStatement iterate:
                WriteKw(iterate.IterateKeyword);
                WriteIdentifier(iterate.Label);
                break;
            case CreateTriggerStatement createTrigger:
                WriteCreateTrigger(createTrigger);
                break;
            case DropTriggerStatement dropTrigger:
                WriteKw(dropTrigger.DropKeyword);
                WriteKw(dropTrigger.TriggerKeyword);
                WriteQualifiedName(dropTrigger.Name);
                break;
            case CreateRoutineStatement createRoutine:
                WriteCreateRoutine(createRoutine);
                break;
            case AlterRoutineStatement alterRoutine:
                WriteAlterRoutine(alterRoutine);
                break;
            case DropRoutineStatement dropRoutine:
                WriteKw(dropRoutine.DropKeyword);
                WriteRoutineDesignator(dropRoutine.Designator);
                WriteKw(dropRoutine.Behavior);
                break;
            case CallStatement call:
                WriteCall(call);
                break;
            case ReturnStatement returnStatement:
                WriteKw(returnStatement.ReturnKeyword);
                AppendPlain(' ');
                AppendExpression(returnStatement.Value);
                break;
            case CreatePropertyGraphStatement createGraph:
                WriteCreatePropertyGraph(createGraph);
                break;
            case DropPropertyGraphStatement dropGraph:
                WriteDropPropertyGraph(dropGraph);
                break;
            case ModuleDefinition module:
                WriteModule(module);
                break;
            case ModuleProcedure procedure:
                WriteModuleProcedure(procedure);
                break;
            case EmbeddedSqlStatement embedded:
                WriteEmbedded(embedded);
                break;
            case DeclareSectionStatement declareSection:
                WriteKw(declareSection.BeginOrEnd);
                WriteKw(declareSection.DeclareKeyword);
                WriteKw(declareSection.SectionKeyword);
                break;
            case WheneverStatement whenever:
                WriteWhenever(whenever);
                break;
            case DirectSqlScript script:
                WriteDirectSqlScript(script);
                break;
            default:
                throw new InvalidOperationException($"Unknown query {query.GetType().Name}");
        }
    }

    private void WriteDirectSqlScript(DirectSqlScript script)
    {
        for (var i = 0; i < script.Statements.Count; i++)
        {
            Write(script.Statements[i]);
            if (i >= script.Semicolons.Count)
            {
                continue;
            }

            AppendLeadingTrivia(script.Semicolons[i]);
            AppendPlain(';');
            if (i < script.Statements.Count - 1)
            {
                AppendLine();
            }
        }
    }

    private void WriteWithQuery(WithQuery withQuery)
    {
        WriteKeyword(withQuery.WithKeyword, Keyword.WithUpper);
        if (withQuery.RecursiveKeyword is { } recursive)
        {
            AppendPlain(' ');
            WriteKeyword(recursive, Keyword.RecursiveUpper);
        }

        if (withQuery.RecursionLimit is { } recursionLimit)
        {
            AppendPlain(' ');
            WriteTypeName(recursionLimit);
        }

        _indent++;
        for (var i = 0; i < withQuery.Ctes.Count; i++)
        {
            AppendLine();
            WriteCte(withQuery.Ctes[i]);
            if (i < withQuery.Ctes.Count - 1)
            {
                AppendPlain(',');
            }
        }

        _indent--;
        AppendLine();
        Write(withQuery.Query);
    }

    private void WriteCte(CommonTableExpression cte)
    {
        WriteIdentifier(cte.Name);
        if (cte.Columns is { } columns)
        {
            AppendPlain('(');
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteIdentifier(columns[i]);
            }

            AppendPlain(')');
        }

        AppendPlain(' ');
        WriteKeyword(cte.AsKeyword, Keyword.AsUpper);
        AppendPlain(' ');
        AppendSubquery(cte.OpenQuery, cte.Query, cte.CloseQuery);
        if (cte.Search is { } search)
        {
            WriteSearchClause(search);
        }

        if (cte.Cycle is { } cycle)
        {
            WriteCycleClause(cycle);
        }
    }

    private void WriteSearchClause(SearchClause search)
    {
        AppendPlain(' ');
        WriteTypeName(search.SearchKeyword);
        AppendPlain(' ');
        WriteTypeName(search.OrderKeyword);
        AppendPlain(' ');
        WriteTypeName(search.FirstKeyword);
        AppendPlain(' ');
        WriteKeyword(search.ByKeyword, Keyword.ByUpper);
        AppendPlain(' ');
        WriteCommaIdentifiers(search.Columns);
        AppendPlain(' ');
        WriteTypeName(search.SetKeyword);
        AppendPlain(' ');
        WriteIdentifier(search.SequenceColumn);
    }

    private void WriteCycleClause(CycleClause cycle)
    {
        AppendPlain(' ');
        WriteTypeName(cycle.CycleKeyword);
        AppendPlain(' ');
        WriteCommaIdentifiers(cycle.Columns);
        AppendPlain(' ');
        WriteTypeName(cycle.SetKeyword);
        AppendPlain(' ');
        WriteIdentifier(cycle.MarkColumn);
        AppendPlain(' ');
        WriteKeyword(cycle.ToKeyword, Keyword.ToUpper);
        AppendPlain(' ');
        AppendExpression(cycle.MarkValue);
        AppendPlain(' ');
        WriteTypeName(cycle.DefaultKeyword);
        AppendPlain(' ');
        AppendExpression(cycle.DefaultValue);
        AppendPlain(' ');
        WriteKeyword(cycle.UsingKeyword, Keyword.UsingUpper);
        AppendPlain(' ');
        WriteIdentifier(cycle.PathColumn);
    }

    private void WriteCommaIdentifiers(IReadOnlyList<SyntaxToken> names)
    {
        for (var i = 0; i < names.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteIdentifier(names[i]);
        }
    }

    private void WriteParenQuery(ParenQuery paren)
    {
        AppendLeadingTrivia(paren.OpenParen);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        AppendLine();
        Write(paren.Inner);
        _indent--;
        AppendLeadingTrivia(paren.CloseParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
    }

    private void WriteSetOperation(SetOperation setOp)
    {
        Write(setOp.Left);
        AppendLeadingTrivia(setOp.Operator);
        AppendLine();
        WriteKeyword(setOp.Operator, SetOperatorText(setOp.Operator.Kind));
        if (setOp.AllKeyword is { } all)
        {
            AppendPlain(' ');
            WriteKeyword(all, Keyword.AllUpper);
        }

        if (setOp.CorrespondingKeyword is { } corresponding)
        {
            AppendPlain(' ');
            WriteKeyword(corresponding, Keyword.CorrespondingUpper);
            if (setOp.ByKeyword is { } by && setOp.Columns is { } columns)
            {
                AppendPlain(' ');
                WriteKeyword(by, Keyword.ByUpper);
                AppendPlain(' ');
                AppendPlain('(');
                for (var i = 0; i < columns.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    WriteIdentifier(columns[i]);
                }

                AppendPlain(')');
            }
        }

        AppendLine();
        Write(setOp.Right);
    }

    private void WriteValues(ValuesQuery values)
    {
        WriteKeyword(values.ValuesKeyword, Keyword.ValuesUpper);
        _indent++;
        for (var i = 0; i < values.Rows.Count; i++)
        {
            var row = values.Rows[i];
            AppendLeadingTrivia(row.OpenParen);
            AppendLine();
            EnsureContentIndent();
            AppendPlain('(');
            AppendCommaExpressions(row.Values);
            AppendPlain(')');
            if (i < values.Rows.Count - 1)
            {
                AppendPlain(',');
            }
        }

        _indent--;
    }

    private void WriteSelect(SelectStatement statement)
    {
        WriteKeyword(statement.SelectKeyword, Keyword.SelectUpper);
        if (statement.DistinctKeyword is { } distinct)
        {
            AppendPlain(' ');
            WriteKeyword(distinct, Keyword.DistinctUpper);
        }
        else if (statement.AllKeyword is { } all)
        {
            AppendPlain(' ');
            WriteKeyword(all, Keyword.AllUpper);
        }

        _indent++;
        for (var i = 0; i < statement.SelectList.Count; i++)
        {
            if (i > 0)
            {
                AppendLeadingTrivia(StartToken(statement.SelectList[i].Expression));
            }

            AppendLine();
            AppendSelectItem(statement.SelectList[i]);
            if (i < statement.SelectList.Count - 1)
            {
                AppendPlain(',');
            }
        }

        _indent--;
        if (statement.IntoKeyword is { } intoKeyword && statement.IntoTargets is { } intoTargets)
        {
            AppendLeadingTrivia(intoKeyword);
            AppendLine();
            WriteKeyword(intoKeyword, Keyword.IntoUpper);
            AppendPlain(' ');
            for (var i = 0; i < intoTargets.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteIntoTarget(intoTargets[i]);
            }
        }

        if (statement.FromKeyword is { } fromKeyword && statement.From is { } from)
        {
            AppendLeadingTrivia(fromKeyword);
            AppendLine();
            WriteKeyword(fromKeyword, Keyword.FromUpper);
            AppendPlain(' ');
            AppendTable(from);
            foreach (var join in statement.Joins)
            {
                AppendLine();
                AppendJoin(join);
            }

            foreach (var extra in statement.ExtraFrom)
            {
                AppendPlain(',');
                AppendPlain(' ');
                AppendTable(extra.Table);
                foreach (var join in extra.Joins)
                {
                    AppendLine();
                    AppendJoin(join);
                }
            }
        }

        if (statement.Where is not null)
        {
            AppendLeadingTrivia(statement.Where.WhereKeyword);
            AppendLine();
            WriteKeyword(statement.Where.WhereKeyword, Keyword.WhereUpper);
            AppendPlain(' ');
            AppendWhereExpression(statement.Where.Expression);
        }

        if (statement.GroupBy is not null)
        {
            AppendLeadingTrivia(statement.GroupBy.GroupKeyword);
            AppendLine();
            WriteKeyword(statement.GroupBy.GroupKeyword, Keyword.GroupUpper);
            AppendPlain(' ');
            WriteKeyword(statement.GroupBy.ByKeyword, Keyword.ByUpper);
            AppendPlain(' ');
            AppendCommaExpressions(statement.GroupBy.Keys);
        }

        if (statement.Having is not null)
        {
            AppendLeadingTrivia(statement.Having.HavingKeyword);
            AppendLine();
            WriteKeyword(statement.Having.HavingKeyword, Keyword.HavingUpper);
            AppendPlain(' ');
            AppendWhereExpression(statement.Having.Expression);
        }

        if (statement.Window is { } window)
        {
            WriteWindowClause(window);
        }

        if (statement.OrderBy is not null)
        {
            WriteOrderByClause(statement.OrderBy, newLine: true);
        }

        if (statement.Limit is not null)
        {
            AppendLeadingTrivia(statement.Limit.LimitKeyword);
            AppendLine();
            WriteKeyword(statement.Limit.LimitKeyword, Keyword.LimitUpper);
            AppendPlain(' ');
            AppendExpression(statement.Limit.Count);
        }

        if (statement.Offset is not null)
        {
            WriteOffsetClause(statement.Offset);
        }

        if (statement.Fetch is { } fetch)
        {
            WriteFetchClause(fetch);
        }

        if (statement.Lock is { } lockClause)
        {
            WriteLockClause(lockClause);
        }
    }

    private void WriteLockClause(LockClause lockClause)
    {
        AppendLeadingTrivia(lockClause.ForKeyword);
        AppendLine();
        WriteKeyword(lockClause.ForKeyword, Keyword.ForUpper);
        if (lockClause.ReadKeyword is { } readKeyword && lockClause.OnlyKeyword is { } onlyKeyword)
        {
            AppendPlain(' ');
            WriteKeyword(readKeyword, Keyword.ReadUpper);
            AppendPlain(' ');
            WriteKeyword(onlyKeyword, Keyword.OnlyUpper);
            return;
        }

        if (lockClause.UpdateKeyword is { } updateKeyword)
        {
            AppendPlain(' ');
            WriteKeyword(updateKeyword, Keyword.UpdateUpper);
        }

        if (lockClause.ShareKeyword is { } shareKeyword)
        {
            AppendPlain(' ');
            WriteTypeName(shareKeyword);
        }

        if (lockClause.OfKeyword is { } ofKeyword && lockClause.Columns is { } columns)
        {
            AppendPlain(' ');
            WriteKeyword(ofKeyword, Keyword.OfUpper);
            AppendPlain(' ');
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteIdentifier(columns[i]);
            }
        }
    }

    private void WriteIntoTarget(SyntaxToken token)
    {
        if (token.Kind == SyntaxKind.QuestionMark)
        {
            AppendLeadingTrivia(token);
            EnsureContentIndent();
            AppendSpaceIfNeeded();
            AppendPlain('?');
            return;
        }

        if (token.Kind == SyntaxKind.EmbeddedHost)
        {
            AppendLeadingTrivia(token);
            EnsureContentIndent();
            AppendSpaceIfNeeded();
            AppendPlain(token.TextOf(_source));
            return;
        }

        WriteIdentifier(token);
    }

    private void WriteOrderByClause(OrderByClause orderBy, bool newLine)
    {
        if (newLine)
        {
            AppendLeadingTrivia(orderBy.OrderKeyword);
            AppendLine();
        }

        WriteKeyword(orderBy.OrderKeyword, Keyword.OrderUpper);
        AppendPlain(' ');
        WriteKeyword(orderBy.ByKeyword, Keyword.ByUpper);
        AppendPlain(' ');
        WriteOrderByItems(orderBy.Items);
    }

    private void WriteOrderByItems(IReadOnlyList<OrderByItem> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            var item = items[i];
            AppendExpression(item.Expression);
            if (item.Direction is { } direction)
            {
                AppendPlain(' ');
                WriteKeyword(
                    direction,
                    direction.Kind == SyntaxKind.DescKeyword ? Keyword.DescUpper : Keyword.AscUpper);
            }

            if (item.NullsKeyword is { } nullsKeyword && item.NullOrder is { } nullOrder)
            {
                AppendPlain(' ');
                WriteTypeName(nullsKeyword);
                AppendPlain(' ');
                WriteTypeName(nullOrder);
            }
        }
    }

    private void WriteOffsetClause(OffsetClause offset)
    {
        AppendLeadingTrivia(offset.OffsetKeyword);
        AppendLine();
        WriteKeyword(offset.OffsetKeyword, Keyword.OffsetUpper);
        AppendPlain(' ');
        AppendExpression(offset.Count);
        if (offset.RowKeyword is { } rowKeyword)
        {
            AppendPlain(' ');
            WriteTypeName(rowKeyword);
        }
    }

    private void WriteFetchClause(FetchClause fetch)
    {
        AppendLeadingTrivia(fetch.FetchKeyword);
        AppendLine();
        WriteTypeName(fetch.FetchKeyword);
        AppendPlain(' ');
        WriteTypeName(fetch.PositionKeyword);
        if (fetch.Count is { } count)
        {
            AppendPlain(' ');
            AppendExpression(count);
        }

        if (fetch.PercentKeyword is { } percent)
        {
            AppendPlain(' ');
            WriteTypeName(percent);
        }

        AppendPlain(' ');
        WriteTypeName(fetch.RowKeyword);
        AppendPlain(' ');
        WriteTypeName(fetch.OnlyOrWith);
        if (fetch.TiesKeyword is { } ties)
        {
            AppendPlain(' ');
            WriteTypeName(ties);
        }
    }

    private void AppendSelectItem(SelectItem item)
    {
        AppendExpression(item.Expression);
        if (item.Alias is { } alias)
        {
            AppendPlain(' ');
            if (item.AsKeyword is { } asKeyword)
            {
                WriteKeyword(asKeyword, Keyword.AsUpper);
            }
            else
            {
                AppendColored(Ansi.Keyword, Keyword.AsUpper);
            }

            AppendPlain(' ');
            WriteIdentifier(alias);
        }
    }

    private void AppendJoin(JoinClause join)
    {
        if (join.NaturalKeyword is { } natural)
        {
            WriteKeyword(natural, Keyword.NaturalUpper);
            AppendPlain(' ');
        }

        if (join.JoinType is { } joinType)
        {
            WriteKeyword(joinType, JoinTypeText(joinType.Kind));
            AppendPlain(' ');
        }

        if (join.OuterKeyword is { } outer)
        {
            WriteKeyword(outer, Keyword.OuterUpper);
            AppendPlain(' ');
        }

        WriteKeyword(join.JoinKeyword, Keyword.JoinUpper);
                    AppendPlain(' ');
        AppendTable(join.Table);
        switch (join.Constraint)
        {
            case OnConstraint on:
                _indent++;
                AppendLeadingTrivia(on.OnKeyword);
                AppendLine();
                WriteKeyword(on.OnKeyword, Keyword.OnUpper);
                AppendPlain(' ');
                AppendExpression(on.Condition);
                _indent--;
                break;
            case UsingConstraint usingConstraint:
                _indent++;
                AppendLeadingTrivia(usingConstraint.UsingKeyword);
                AppendLine();
                WriteKeyword(usingConstraint.UsingKeyword, Keyword.UsingUpper);
                AppendPlain(' ');
                AppendPlain('(');
                for (var i = 0; i < usingConstraint.Columns.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    WriteIdentifier(usingConstraint.Columns[i]);
                }

                AppendPlain(')');
                _indent--;
                break;
        }
    }

    private static string SetOperatorText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.UnionKeyword => Keyword.UnionUpper,
        SyntaxKind.ExceptKeyword => Keyword.ExceptUpper,
        SyntaxKind.IntersectKeyword => Keyword.IntersectUpper,
        _ => throw new InvalidOperationException($"Unknown set operator {kind}"),
    };

    private static string QuantifierText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.AllKeyword => Keyword.AllUpper,
        SyntaxKind.AnyKeyword => Keyword.AnyUpper,
        SyntaxKind.SomeKeyword => Keyword.SomeUpper,
        _ => throw new InvalidOperationException($"Unknown quantifier {kind}"),
    };

    private static string JoinTypeText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.InnerKeyword => Keyword.InnerUpper,
        SyntaxKind.LeftKeyword => Keyword.LeftUpper,
        SyntaxKind.RightKeyword => Keyword.RightUpper,
        SyntaxKind.FullKeyword => Keyword.FullUpper,
        SyntaxKind.CrossKeyword => Keyword.CrossUpper,
        SyntaxKind.UnionKeyword => Keyword.UnionUpper,
        _ => throw new InvalidOperationException($"Unknown join type {kind}"),
    };

    private void AppendTable(TableSource table)
    {
        switch (table)
        {
            case TableReference named:
                AppendNamedTable(named);
                break;
            case DerivedTable derived:
                AppendDerivedTable(derived);
                break;
            case JoinedTable joined:
                AppendJoinedTable(joined);
                break;
            case OnlyTable only:
                AppendOnlyTable(only);
                break;
            case UnnestTable unnest:
                AppendUnnestTable(unnest);
                break;
            case TableFunction function:
                AppendTableFunction(function);
                break;
            case SampledTable sampled:
                AppendSampledTable(sampled);
                break;
            case MarkupTable markup:
                AppendMarkupTable(markup);
                break;
            case MatchRecognizeTable matchRecognize:
                AppendMatchRecognize(matchRecognize);
                break;
            case PtfTable ptf:
                AppendPtfTable(ptf);
                break;
            case GraphTable graph:
                AppendGraphTable(graph);
                break;
            default:
                throw new InvalidOperationException($"Unknown table {table.GetType().Name}");
        }
    }

    private void AppendNamedTable(TableReference table)
    {
        for (var i = 0; i < table.NameParts.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain('.');
            }

            WriteIdentifier(table.NameParts[i]);
        }

        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
        WriteSystemTime(table.SystemTime);
    }

    private void AppendDerivedTable(DerivedTable table)
    {
        if (table.LateralKeyword is { } lateral)
        {
            WriteTypeName(lateral);
            AppendPlain(' ');
        }

        AppendSubquery(table.OpenParen, table.Query, table.CloseParen);
        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
    }

    private void AppendOnlyTable(OnlyTable table)
    {
        WriteKeyword(table.OnlyKeyword, Keyword.OnlyUpper);
        AppendLeadingTrivia(table.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteQualifiedName(table.NameParts);
        AppendPlain(')');
        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
        WriteSystemTime(table.SystemTime);
    }

    private void WriteSystemTime(SystemTimeClause? systemTime)
    {
        if (systemTime is null)
        {
            return;
        }

        WriteKw(systemTime.ForKeyword);
        WriteKw(systemTime.SystemTimeKeyword);
        WriteKw(systemTime.AsKeyword);
        WriteKw(systemTime.OfKeyword);
        if (systemTime.Point is { } point)
        {
            AppendPlain(' ');
            AppendExpression(point);
        }

        WriteKw(systemTime.BetweenKeyword);
        WriteKw(systemTime.Qualifier);
        WriteKw(systemTime.FromKeyword);
        if (systemTime.Start is { } start)
        {
            AppendPlain(' ');
            AppendExpression(start);
        }

        WriteKw(systemTime.AndKeyword);
        WriteKw(systemTime.ToKeyword);
        if (systemTime.End is { } end)
        {
            AppendPlain(' ');
            AppendExpression(end);
        }

        WriteKw(systemTime.AllKeyword);
    }

    private void AppendUnnestTable(UnnestTable table)
    {
        WriteTypeName(table.UnnestKeyword);
        AppendPlain('(');
        AppendCommaExpressions(table.Expressions);
        AppendPlain(')');
        if (table.WithKeyword is { } withKeyword && table.OrdinalityKeyword is { } ordinality)
        {
            AppendPlain(' ');
            WriteKeyword(withKeyword, Keyword.WithUpper);
            AppendPlain(' ');
            WriteTypeName(ordinality);
        }

        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
    }

    private void AppendTableFunction(TableFunction table)
    {
        if (table.TableKeyword is { } tableKeyword)
        {
            WriteTypeName(tableKeyword);
        }
        else
        {
            WriteQualifiedName(table.NameParts);
        }

        if (table.Query is { } query)
        {
            AppendSubqueryCall(table.OpenParen, query, table.CloseParen);
        }
        else
        {
            AppendLeadingTrivia(table.OpenParen);
            AppendPlain('(');
            AppendCommaExpressions(table.Arguments);
            AppendPlain(')');
        }

        AppendTableAlias(table.AsKeyword, table.Alias);
        AppendCorrelationColumns(table.ColumnOpenParen, table.Columns, table.ColumnCloseParen);
    }

    private void AppendSampledTable(SampledTable table)
    {
        AppendTable(table.Table);
        AppendPlain(' ');
        WriteTypeName(table.TablesampleKeyword);
        AppendPlain(' ');
        WriteTypeName(table.Method);
        AppendLeadingTrivia(table.OpenParen);
        AppendSpaceIfNeeded();
        AppendPlain('(');
        AppendExpression(table.Percentage);
        AppendPlain(')');
        if (table.RepeatableKeyword is { } repeatable
            && table.RepeatOpenParen is { } repeatOpen
            && table.RepeatArgument is { } repeatArgument)
        {
            AppendPlain(' ');
            WriteTypeName(repeatable);
            AppendLeadingTrivia(repeatOpen);
            AppendSpaceIfNeeded();
            AppendPlain('(');
            AppendExpression(repeatArgument);
            AppendPlain(')');
        }
    }

    private void AppendJoinedTable(JoinedTable table)
    {
        AppendLeadingTrivia(table.OpenParen);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendPlain('(');
        _indent++;
        AppendLine();
        AppendTable(table.Table);
        foreach (var join in table.Joins)
        {
            AppendLine();
            AppendJoin(join);
        }

        _indent--;
        AppendLeadingTrivia(table.CloseParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
    }

    private void AppendSubquery(SyntaxToken openParen, Query query, SyntaxToken closeParen)
    {
        AppendLeadingTrivia(openParen);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendPlain('(');
        WriteSubqueryBody(query, closeParen);
    }

    private void AppendSubqueryCall(SyntaxToken openParen, Query query, SyntaxToken closeParen)
    {
        AppendLeadingTrivia(openParen);
        AppendPlain('(');
        WriteSubqueryBody(query, closeParen);
    }

    private void WriteSubqueryBody(Query query, SyntaxToken closeParen)
    {
        _indent++;
        AppendLine();
        Write(query);
        _indent--;
        AppendLeadingTrivia(closeParen);
        AppendLine();
        EnsureContentIndent();
        AppendPlain(')');
    }

    private void AppendTableAlias(SyntaxToken? asKeyword, SyntaxToken? alias)
    {
        if (alias is not { } name)
        {
            return;
        }

        AppendPlain(' ');
        if (asKeyword is { } keyword)
        {
            WriteKeyword(keyword, Keyword.AsUpper);
        }
        else
        {
            AppendColored(Ansi.Keyword, Keyword.AsUpper);
        }

        AppendPlain(' ');
        WriteIdentifier(name);
    }

    private void AppendCorrelationColumns(
        SyntaxToken? openParen,
        IReadOnlyList<SyntaxToken>? columns,
        SyntaxToken? closeParen)
    {
        if (openParen is not { } open || columns is null || closeParen is null)
        {
            return;
        }

        var last = _lastChar;
        AppendLeadingTrivia(open);
        if (_lastChar != last)
        {
            AppendSpaceIfNeeded();
        }

        AppendPlain('(');
        for (var i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteIdentifier(columns[i]);
        }

        AppendPlain(')');
    }

    private void AppendWhereExpression(Expression expression)
    {
        if (expression is BinaryExpression { OperatorToken.Kind: SyntaxKind.AndKeyword } and)
        {
            AppendWhereExpression(and.Left);
            _indent++;
            AppendLeadingTrivia(and.OperatorToken);
            AppendLine();
            WriteKeyword(and.OperatorToken, Keyword.AndUpper);
            AppendPlain(' ');
            AppendExpression(and.Right);
            _indent--;
            return;
        }

        AppendExpression(expression);
    }

}
