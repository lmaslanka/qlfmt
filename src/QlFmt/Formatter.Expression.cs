using QlParse;

namespace QlFmt;

internal sealed partial class Formatter
{
    private void AppendExpression(Expression expression)
    {
        switch (expression)
        {
            case ArrayExpression array:
                WriteKeyword(array.ArrayKeyword, Keyword.ArrayUpper);
                AppendLeadingTrivia(array.OpenBracket);
                AppendPlain('[');
                for (var i = 0; i < array.Elements.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    AppendExpression(array.Elements[i]);
                }

                AppendPlain(']');
                break;
            case CastExpression cast:
                WriteKeyword(cast.CastKeyword, Keyword.CastUpper);
                AppendPlain('(');
                AppendExpression(cast.Expression);
                AppendPlain(' ');
                WriteKeyword(cast.AsKeyword, Keyword.AsUpper);
                AppendPlain(' ');
                WriteDataType(cast.Type);
                AppendPlain(')');
                break;
            case CoalesceExpression coalesce:
                WriteIdentifier(coalesce.CoalesceKeyword);
                AppendPlain('(');
                AppendCommaExpressions(coalesce.Arguments);
                AppendPlain(')');
                break;
            case NullIfExpression nullIf:
                WriteIdentifier(nullIf.NullIfKeyword);
                AppendPlain('(');
                AppendExpression(nullIf.First);
                AppendPlain(',');
                AppendPlain(' ');
                AppendExpression(nullIf.Second);
                AppendPlain(')');
                break;
            case ColonCastExpression colonCast:
                AppendExpression(colonCast.Expression);
                AppendLeadingTrivia(colonCast.DoubleColon);
                AppendPlain(DoubleColon);
                WriteDataType(colonCast.Type);
                break;
            case CollateExpression collate:
                AppendExpression(collate.Expression);
                AppendPlain(' ');
                WriteKeyword(collate.CollateKeyword, Keyword.CollateUpper);
                AppendPlain(' ');
                WriteIdentifier(collate.Name);
                break;
            case IdentifierExpression identifier:
                WriteIdentifier(identifier.Identifier);
                break;
            case EmbeddedHostExpression host:
                AppendLeadingTrivia(host.Name);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain(host.Name.TextOf(_source));
                break;
            case HostParameterExpression parameter:
                AppendLeadingTrivia(parameter.QuestionMark);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain('?');
                break;
            case NiladicFunctionExpression niladic:
                WriteNiladic(niladic);
                break;
            case LiteralExpression literal:
                if (literal.Literal.Kind == SyntaxKind.TrueKeyword)
                {
                    WriteKeyword(literal.Literal, Keyword.TrueUpper);
                    break;
                }

                if (literal.Literal.Kind == SyntaxKind.FalseKeyword)
                {
                    WriteKeyword(literal.Literal, Keyword.FalseUpper);
                    break;
                }

                if (literal.Literal.Kind == SyntaxKind.NullKeyword)
                {
                    WriteKeyword(literal.Literal, Keyword.NullUpper);
                    break;
                }

                AppendLeadingTrivia(literal.Literal);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendLiteral(literal.Literal);
                break;
            case DatetimeLiteralExpression datetime:
                WriteKeyword(datetime.KindKeyword, DatetimeKeywordText(datetime.KindKeyword.Kind));
                AppendLeadingTrivia(datetime.Literal);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendLiteral(datetime.Literal);
                break;
            case IntervalLiteralExpression interval:
                WriteInterval(interval);
                break;
            case TrimExpression trim:
                WriteTrim(trim);
                break;
            case ExtractExpression extract:
                WriteExtract(extract);
                break;
            case SubstringExpression substring:
                WriteSubstring(substring);
                break;
            case PositionExpression position:
                WritePosition(position);
                break;
            case UsingTransformExpression transform:
                WriteUsingTransform(transform);
                break;
            case BinaryExpression binary:
                AppendExpression(binary.Left);
                if (binary.OperatorToken.Kind is not (SyntaxKind.JsonArrowToken or SyntaxKind.JsonTextArrowToken))
                {
                    AppendPlain(' ');
                }

                AppendOperator(binary.OperatorToken);
                if (binary.OperatorToken.Kind is not (SyntaxKind.JsonArrowToken or SyntaxKind.JsonTextArrowToken))
                {
                    AppendPlain(' ');
                }

                AppendExpression(binary.Right);
                break;
            case MemberAccessExpression member:
                AppendExpression(member.Target);
                AppendPlain('.');
                WriteIdentifier(member.Member);
                break;
            case BetweenExpression between:
                AppendExpression(between.Target);
                AppendPlain(' ');
                if (between.NotKeyword is { } notBetween)
                {
                    WriteKeyword(notBetween, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(between.BetweenKeyword, Keyword.BetweenUpper);
                AppendPlain(' ');
                AppendExpression(between.Lower);
                AppendPlain(' ');
                WriteKeyword(between.AndKeyword, Keyword.AndUpper);
                AppendPlain(' ');
                AppendExpression(between.Upper);
                break;
            case InExpression inExpression:
                AppendExpression(inExpression.Target);
                AppendPlain(' ');
                if (inExpression.NotKeyword is { } notIn)
                {
                    WriteKeyword(notIn, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(inExpression.InKeyword, Keyword.InUpper);
                AppendPlain(' ');
                if (inExpression.Query is { } inQuery)
                {
                    AppendSubquery(inExpression.OpenParen, inQuery, inExpression.CloseParen);
                    break;
                }

                AppendPlain('(');
                for (var i = 0; i < inExpression.Values.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendPlain(',');
                        AppendPlain(' ');
                    }

                    AppendExpression(inExpression.Values[i]);
                }

                AppendPlain(')');
                break;
            case LikeExpression like:
                AppendExpression(like.Target);
                AppendPlain(' ');
                if (like.NotKeyword is { } notLike)
                {
                    WriteKeyword(notLike, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(like.LikeKeyword, Keyword.LikeUpper);
                AppendPlain(' ');
                AppendExpression(like.Pattern);
                if (like.EscapeKeyword is { } escapeKeyword && like.Escape is { } escape)
                {
                    AppendPlain(' ');
                    WriteKeyword(escapeKeyword, Keyword.EscapeUpper);
                    AppendPlain(' ');
                    AppendExpression(escape);
                }

                break;
            case IsExpression isExpression:
                AppendExpression(isExpression.Target);
                AppendPlain(' ');
                WriteKeyword(isExpression.IsKeyword, Keyword.IsUpper);
                AppendPlain(' ');
                if (isExpression.NotKeyword is { } notKeyword)
                {
                    WriteKeyword(notKeyword, Keyword.NotUpper);
                    AppendPlain(' ');
                }

                WriteKeyword(isExpression.Value, IsValueText(isExpression.Value.Kind));
                break;
            case NotExpression not:
                WriteKeyword(not.NotKeyword, Keyword.NotUpper);
                AppendPlain(' ');
                AppendExpression(not.Expression);
                break;
            case FunctionCallExpression call:
                WriteIdentifier(call.Name);
                AppendPlain('(');
                AppendCommaExpressions(call.Arguments);
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

                break;
            case RowConstructorExpression row:
                AppendLeadingTrivia(row.OpenParen);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain('(');
                AppendCommaExpressions(row.Elements);
                AppendPlain(')');
                break;
            case OverlapsExpression overlaps:
                AppendExpression(overlaps.Left);
                AppendPlain(' ');
                WriteKeyword(overlaps.OverlapsKeyword, Keyword.OverlapsUpper);
                AppendPlain(' ');
                AppendExpression(overlaps.Right);
                break;
            case ParenExpression paren:
                AppendLeadingTrivia(paren.OpenParen);
                EnsureContentIndent();
                AppendPlain('(');
                AppendExpression(paren.Inner);
                AppendPlain(')');
                break;
            case ScalarSubqueryExpression subquery:
                AppendSubquery(subquery.OpenParen, subquery.Query, subquery.CloseParen);
                break;
            case ExistsExpression exists:
                WriteKeyword(exists.ExistsKeyword, Keyword.ExistsUpper);
                AppendPlain(' ');
                AppendSubquery(exists.OpenParen, exists.Query, exists.CloseParen);
                break;
            case UniqueExpression unique:
                WriteKeyword(unique.UniqueKeyword, Keyword.UniqueUpper);
                AppendPlain(' ');
                AppendSubquery(unique.OpenParen, unique.Query, unique.CloseParen);
                break;
            case MatchExpression match:
                AppendExpression(match.Left);
                AppendPlain(' ');
                WriteKeyword(match.MatchKeyword, Keyword.MatchUpper);
                if (match.UniqueKeyword is { } uniqueKeyword)
                {
                    AppendPlain(' ');
                    WriteKeyword(uniqueKeyword, Keyword.UniqueUpper);
                }

                if (match.MatchType is { } matchType)
                {
                    AppendPlain(' ');
                    WriteKeyword(matchType, MatchTypeText(matchType.Kind));
                }

                AppendPlain(' ');
                AppendSubquery(match.OpenParen, match.Query, match.CloseParen);
                break;
            case QuantifiedSubqueryExpression quantified:
                AppendExpression(quantified.Left);
                AppendPlain(' ');
                AppendOperator(quantified.OperatorToken);
                AppendPlain(' ');
                WriteKeyword(quantified.Quantifier, QuantifierText(quantified.Quantifier.Kind));
                AppendPlain(' ');
                AppendSubquery(quantified.OpenParen, quantified.Query, quantified.CloseParen);
                break;
            case StarExpression star:
                AppendLeadingTrivia(star.Star);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain('*');
                break;
            case QualifiedStarExpression qualifiedStar:
                AppendExpression(qualifiedStar.Target);
                AppendPlain('.');
                AppendPlain('*');
                break;
            case CaseExpression caseExpression:
                AppendCase(caseExpression);
                break;
            case UnaryExpression unary:
                AppendLeadingTrivia(unary.OperatorToken);
                EnsureContentIndent();
                AppendSpaceIfNeeded();
                AppendPlain(unary.OperatorToken.Kind == SyntaxKind.MinusToken ? '-' : '+');
                AppendExpression(unary.Expression);
                break;
            case TreatExpression treat:
                WriteTreat(treat);
                break;
            case NextValueExpression next:
                WriteNextValue(next);
                break;
            case SpecialFormExpression special:
                WriteSpecialForm(special);
                break;
            case OverlayExpression overlay:
                WriteOverlay(overlay);
                break;
            case ArrayQueryExpression arrayQuery:
                WriteKeyword(arrayQuery.ArrayKeyword, Keyword.ArrayUpper);
                AppendSubqueryCall(arrayQuery.OpenParen, arrayQuery.Query, arrayQuery.CloseParen);
                break;
            case MultisetExpression multiset:
                WriteMultiset(multiset);
                break;
            case MultisetQueryExpression multisetQuery:
                WriteTypeName(multisetQuery.Keyword);
                AppendSubqueryCall(multisetQuery.OpenParen, multisetQuery.Query, multisetQuery.CloseParen);
                break;
            case MultisetOperationExpression multisetOp:
                WriteMultisetOperation(multisetOp);
                break;
            case MultisetSetExpression multisetSet:
                WriteTypeName(multisetSet.SetKeyword);
                AppendPlain('(');
                AppendExpression(multisetSet.Expression);
                AppendPlain(')');
                break;
            case AbsentOnNullExpression absent:
                WriteTypeName(absent.AbsentKeyword);
                WriteKeyword(absent.OnKeyword, Keyword.OnUpper);
                WriteKeyword(absent.NullKeyword, Keyword.NullUpper);
                break;
            case DerefExpression deref:
                WriteTypeName(deref.DerefKeyword);
                AppendPlain('(');
                AppendExpression(deref.Expression);
                AppendPlain(')');
                break;
            case RefValueExpression refValue:
                WriteTypeName(refValue.RefKeyword);
                AppendPlain('(');
                AppendExpression(refValue.Expression);
                AppendPlain(')');
                break;
            case DereferenceExpression dereference:
                AppendExpression(dereference.Reference);
                AppendLeadingTrivia(dereference.Arrow);
                AppendPlain(JsonArrow);
                WriteIdentifier(dereference.Attribute);
                break;
            case SpecifictypeExpression specifictype:
                WriteTypeName(specifictype.SpecifictypeKeyword);
                AppendPlain('(');
                AppendExpression(specifictype.Expression);
                AppendPlain(')');
                break;
            case MethodInvocationExpression method:
                WriteMethodInvocation(method);
                break;
            case StaticMethodInvocationExpression staticMethod:
                WriteStaticMethod(staticMethod);
                break;
            case NewSpecificationExpression created:
                WriteNewSpecification(created);
                break;
            default:
                throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}");
        }
    }

    private void AppendCase(CaseExpression expression)
    {
        WriteKeyword(expression.CaseKeyword, Keyword.CaseUpper);
        if (expression.Operand is { } operand)
        {
            AppendPlain(' ');
            AppendExpression(operand);
        }

        _indent++;
        foreach (var arm in expression.Arms)
        {
            AppendLeadingTrivia(arm.WhenKeyword);
            AppendLine();
            WriteKeyword(arm.WhenKeyword, Keyword.WhenUpper);
            AppendPlain(' ');
            AppendExpression(arm.Condition);
            AppendPlain(' ');
            WriteKeyword(arm.ThenKeyword, Keyword.ThenUpper);
            AppendPlain(' ');
            AppendExpression(arm.Result);
        }

        if (expression.ElseKeyword is { } elseKeyword && expression.ElseResult is { } elseResult)
        {
            AppendLeadingTrivia(elseKeyword);
            AppendLine();
            WriteKeyword(elseKeyword, Keyword.ElseUpper);
            AppendPlain(' ');
            AppendExpression(elseResult);
        }

        _indent--;
        AppendLeadingTrivia(expression.EndKeyword);
        AppendLine();
        WriteKeyword(expression.EndKeyword, Keyword.EndUpper);
    }

    private void WriteNiladic(NiladicFunctionExpression niladic)
    {
        WriteTypeName(niladic.Name);
        if (niladic.OpenParen is not { } openParen
            || niladic.Precision is not { } precision
            || niladic.CloseParen is null)
        {
            return;
        }

        var last = _lastChar;
        AppendLeadingTrivia(openParen);
        if (_lastChar != last)
        {
            AppendSpaceIfNeeded();
        }

        AppendPlain('(');
        AppendPlain(precision.TextOf(_source));
        AppendPlain(')');
    }

    private void WriteTreat(TreatExpression treat)
    {
        WriteTypeName(treat.TreatKeyword);
        AppendPlain('(');
        AppendExpression(treat.Expression);
        AppendPlain(' ');
        WriteKeyword(treat.AsKeyword, Keyword.AsUpper);
        AppendPlain(' ');
        WriteDataType(treat.Type);
        AppendPlain(')');
    }

    private void WriteNextValue(NextValueExpression next)
    {
        WriteTypeName(next.NextKeyword);
        WriteTypeName(next.ValueKeyword);
        WriteKeyword(next.ForKeyword, Keyword.ForUpper);
        WriteQualifiedName(next.NameParts);
    }

    private void WriteSpecialForm(SpecialFormExpression special)
    {
        WriteTypeName(special.Name);
        AppendPlain('(');
        AppendCommaExpressions(special.Arguments);
        if (special.UsingKeyword is { } usingKeyword && special.UsingName is { } usingName)
        {
            AppendPlain(' ');
            WriteKeyword(usingKeyword, Keyword.UsingUpper);
            AppendPlain(' ');
            WriteTypeName(usingName);
        }

        AppendPlain(')');
    }

    private void WriteOverlay(OverlayExpression overlay)
    {
        WriteTypeName(overlay.OverlayKeyword);
        AppendPlain('(');
        AppendExpression(overlay.Source);
        AppendPlain(' ');
        WriteTypeName(overlay.PlacingKeyword);
        AppendPlain(' ');
        AppendExpression(overlay.Replacement);
        AppendPlain(' ');
        WriteKeyword(overlay.FromKeyword, Keyword.FromUpper);
        AppendPlain(' ');
        AppendExpression(overlay.Start);
        if (overlay.ForKeyword is { } forKeyword && overlay.Length is { } length)
        {
            AppendPlain(' ');
            WriteKeyword(forKeyword, Keyword.ForUpper);
            AppendPlain(' ');
            AppendExpression(length);
        }

        AppendPlain(')');
    }

    private void WriteMultiset(MultisetExpression multiset)
    {
        WriteTypeName(multiset.MultisetKeyword);
        AppendLeadingTrivia(multiset.OpenBracket);
        AppendPlain('[');
        AppendCommaExpressions(multiset.Elements);
        AppendPlain(']');
    }

    private void WriteMultisetOperation(MultisetOperationExpression multisetOp)
    {
        AppendExpression(multisetOp.Left);
        AppendPlain(' ');
        WriteTypeName(multisetOp.MultisetKeyword);
        AppendPlain(' ');
        WriteTypeName(multisetOp.Operator);
        if (multisetOp.Quantifier is { } quantifier)
        {
            AppendPlain(' ');
            WriteTypeName(quantifier);
        }

        AppendPlain(' ');
        AppendExpression(multisetOp.Right);
    }

    private void WriteMethodInvocation(MethodInvocationExpression method)
    {
        if (method.OpenParen is { } open)
        {
            AppendLeadingTrivia(open);
            EnsureContentIndent();
            AppendSpaceIfNeeded();
            AppendPlain('(');
            AppendExpression(method.Target);
            if (method.AsKeyword is { } asKeyword && method.Type is { } type)
            {
                AppendPlain(' ');
                WriteKeyword(asKeyword, Keyword.AsUpper);
                AppendPlain(' ');
                WriteDataType(type);
            }

            AppendPlain(')');
        }
        else
        {
            AppendExpression(method.Target);
        }

        AppendPlain('.');
        WriteIdentifier(method.Name);
        AppendPlain('(');
        AppendCommaExpressions(method.Arguments);
        AppendPlain(')');
    }

    private void WriteStaticMethod(StaticMethodInvocationExpression staticMethod)
    {
        AppendExpression(staticMethod.Type);
        AppendLeadingTrivia(staticMethod.DoubleColon);
        AppendPlain(DoubleColon);
        WriteIdentifier(staticMethod.Name);
        AppendPlain('(');
        AppendCommaExpressions(staticMethod.Arguments);
        AppendPlain(')');
    }

    private void WriteNewSpecification(NewSpecificationExpression created)
    {
        WriteTypeName(created.NewKeyword);
        WriteQualifiedName(created.TypeName);
        AppendPlain('(');
        AppendCommaExpressions(created.Arguments);
        AppendPlain(')');
    }

    private void WriteTrim(TrimExpression trim)
    {
        WriteKeyword(trim.TrimKeyword, Keyword.TrimUpper);
        AppendPlain('(');
        if (trim.Specification is { } specification)
        {
            WriteTypeName(specification);
        }

        if (trim.Characters is { } characters)
        {
            AppendExpression(characters);
        }

        if (trim.FromKeyword is { } fromKeyword)
        {
            AppendPlain(' ');
            WriteKeyword(fromKeyword, Keyword.FromUpper);
        }

        AppendExpression(trim.Source);
        AppendPlain(')');
    }

    private void WriteExtract(ExtractExpression extract)
    {
        WriteKeyword(extract.ExtractKeyword, Keyword.ExtractUpper);
        AppendPlain('(');
        WriteTypeName(extract.Field);
        AppendPlain(' ');
        WriteKeyword(extract.FromKeyword, Keyword.FromUpper);
        AppendExpression(extract.Source);
        AppendPlain(')');
    }

    private void WriteSubstring(SubstringExpression substring)
    {
        WriteKeyword(substring.SubstringKeyword, Keyword.SubstringUpper);
        AppendPlain('(');
        AppendExpression(substring.Source);
        AppendPlain(' ');
        WriteKeyword(substring.FromKeyword, Keyword.FromUpper);
        AppendExpression(substring.Start);
        if (substring.ForKeyword is { } forKeyword && substring.Length is { } length)
        {
            AppendPlain(' ');
            WriteKeyword(forKeyword, Keyword.ForUpper);
            AppendExpression(length);
        }

        AppendPlain(')');
    }

    private void WriteUsingTransform(UsingTransformExpression transform)
    {
        WriteKeyword(
            transform.FunctionKeyword,
            transform.FunctionKeyword.Kind == SyntaxKind.ConvertKeyword
                ? Keyword.ConvertUpper
                : Keyword.TranslateUpper);
        AppendPlain('(');
        AppendExpression(transform.Expression);
        AppendPlain(' ');
        WriteKeyword(transform.UsingKeyword, Keyword.UsingUpper);
        AppendPlain(' ');
        WriteIdentifier(transform.Name);
        AppendPlain(')');
    }

    private void WritePosition(PositionExpression position)
    {
        WriteKeyword(position.PositionKeyword, Keyword.PositionUpper);
        AppendPlain('(');
        AppendExpression(position.Needle);
        AppendPlain(' ');
        WriteKeyword(position.InKeyword, Keyword.InUpper);
        AppendExpression(position.Haystack);
        AppendPlain(')');
    }

    private void WriteInterval(IntervalLiteralExpression interval)
    {
        WriteKeyword(interval.IntervalKeyword, Keyword.IntervalUpper);
        if (interval.Sign is { } sign)
        {
            AppendLeadingTrivia(sign);
            EnsureContentIndent();
            AppendSpaceIfNeeded();
            AppendPlain(sign.Kind == SyntaxKind.MinusToken ? '-' : '+');
        }

        AppendLeadingTrivia(interval.Literal);
        EnsureContentIndent();
        AppendSpaceIfNeeded();
        AppendLiteral(interval.Literal);
        AppendPlain(' ');
        WriteIntervalField(interval.Qualifier.Start);
        if (interval.Qualifier.ToKeyword is { } to && interval.Qualifier.End is { } end)
        {
            AppendPlain(' ');
            WriteKeyword(to, Keyword.ToUpper);
            AppendPlain(' ');
            WriteIntervalField(end);
        }
    }

    private void WriteIntervalField(IntervalField field)
    {
        WriteTypeName(field.Name);
        if (field.OpenParen is null || field.Precision is not { } precision || field.CloseParen is null)
        {
            return;
        }

        AppendPlain('(');
        AppendPlain(precision.TextOf(_source));
        if (field.Scale is { } scale)
        {
            AppendPlain(',');
            AppendPlain(' ');
            AppendPlain(scale.TextOf(_source));
        }

        AppendPlain(')');
    }

    private static string DatetimeKeywordText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.DateKeyword => Keyword.DateUpper,
        SyntaxKind.TimeKeyword => Keyword.TimeUpper,
        SyntaxKind.TimestampKeyword => Keyword.TimestampUpper,
        _ => throw new InvalidOperationException($"Unknown datetime keyword {kind}"),
    };

    private void WriteDataType(DataType type)
    {
        WriteTypeName(type.Name);
        WriteTypeNameTail(type.Name, type.NameTail);
        WriteDataTypeArguments(type);

        if (type.WithKeyword is { } withKeyword
            && type.TimeKeyword is { } timeKeyword
            && type.Zone is { } zone)
        {
            AppendPlain(' ');
            WriteKeyword(withKeyword, Keyword.WithUpper);
            AppendPlain(' ');
            WriteKeyword(timeKeyword, Keyword.TimeUpper);
            AppendPlain(' ');
            WriteKeyword(zone, Keyword.ZoneUpper);
        }

        if (type.ScopeKeyword is { } scopeKeyword && type.ScopeName is { } scopeName)
        {
            AppendPlain(' ');
            WriteTypeName(scopeKeyword);
            AppendPlain(' ');
            WriteQualifiedName(scopeName);
        }

        foreach (var suffix in type.Collections)
        {
            WriteCollectionSuffix(suffix);
        }
    }

    private void WriteDataTypeArguments(DataType type)
    {
        if (type.Fields is { } fields)
        {
            WriteRowFields(type, fields);
            return;
        }

        if (type.ReferencedType is { } referenced)
        {
            AppendPlain('(');
            WriteDataType(referenced);
            AppendPlain(')');
            return;
        }

        if (type.Modifier is { } modifier)
        {
            AppendPlain('(');
            WriteTypeName(modifier);
            AppendPlain(')');
            return;
        }

        if (type.OpenParen is not null && type.Precision is { } precision && type.CloseParen is not null)
        {
            AppendPlain('(');
            AppendPlain(precision.TextOf(_source));
            if (type.Scale is { } scale)
            {
                AppendPlain(',');
                AppendPlain(' ');
                AppendPlain(scale.TextOf(_source));
            }

            AppendPlain(')');
        }
    }

    private void WriteRowFields(DataType type, IReadOnlyList<FieldDefinition> fields)
    {
        if (type.OpenParen is { } open)
        {
            AppendLeadingTrivia(open);
        }

        AppendPlain('(');
        for (var i = 0; i < fields.Count; i++)
        {
            if (i > 0)
            {
                AppendPlain(',');
                AppendPlain(' ');
            }

            WriteIdentifier(fields[i].Name);
            WriteDataType(fields[i].Type);
        }

        AppendPlain(')');
    }

    private void WriteCollectionSuffix(CollectionSuffix suffix)
    {
        WriteTypeName(suffix.Keyword);
        if (suffix.OpenBracket is not { } openBracket || suffix.CloseBracket is null)
        {
            return;
        }

        AppendLeadingTrivia(openBracket);
        AppendPlain('[');
        if (suffix.Dimensions is { } dimensions)
        {
            for (var i = 0; i < dimensions.Count; i++)
            {
                if (i > 0)
                {
                    AppendPlain(',');
                    AppendPlain(' ');
                }

                WriteMdarrayDimension(dimensions[i]);
            }
        }
        else if (suffix.Cardinality is { } cardinality)
        {
            AppendPlain(cardinality.TextOf(_source));
        }

        AppendPlain(']');
    }

    private void WriteMdarrayDimension(MdarrayDimension dimension)
    {
        if (dimension.Lower is { } lower)
        {
            AppendPlain(lower.TextOf(_source));
        }

        if (dimension.Colon is not null)
        {
            AppendPlain(':');
        }

        AppendPlain(dimension.Upper.TextOf(_source));
    }

}
