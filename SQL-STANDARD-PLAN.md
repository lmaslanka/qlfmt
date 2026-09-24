# Formatter plan: ISO SQL coverage

Source of truth: `../qlparse/SQL-STANDARD.md` (`[x]` = qlparse accepts it).
This plan is how qlfmt prints every item on that list.

Work one step at a time. Tests first. After each Write/Edit: `qlcheck` on
changed paths, then `dotnet test tests/QlFmt.Tests`.

---

## Current state

`Formatter` (`src/QlFmt/Formatter.cs`, ~1600 lines) is a hand-written switch
over a subset of `QlParse` nodes. It already formats:

- `SELECT` / `WITH` / set ops / `VALUES` / `INSERT … SELECT|VALUES`
- Core scalar expressions, predicates (`BETWEEN` `IN` `LIKE` `IS` `EXISTS`
  `UNIQUE` `MATCH` `OVERLAPS` quantified comparison), joins including
  `UNION JOIN` / `NATURAL` / `USING`, `CASE`, `CAST` / `::`, `ARRAY[…]`,
  `FILTER (WHERE …)`, JSON `->` / `->>`, host `?` / `:name`, comments

It throws `Unknown query` / `Unknown expression` / `Unknown table` on
everything else. `SqlVisitor` in qlparse already enumerates the full AST;
the formatter does not.

Gaps that already exist on nodes the formatter *does* visit:

| Node | Missing fields |
| --- | --- |
| `SelectStatement` | `INTO`, `WINDOW`, `FETCH`, `OFFSET` `ROW`/`ROWS` |
| `OrderByItem` | `NULLS FIRST` / `NULLS LAST` |
| `LockClause` | `FOR SHARE` |
| `WithQuery` | `RecursionLimit` |
| `CommonTableExpression` | `SEARCH`, `CYCLE` |
| `InsertStatement` | `OVERRIDING`, `DEFAULT VALUES` (`Query` is nullable) |
| `DerivedTable` | `LATERAL` |
| `TableReference` | `SystemTime` |
| `FunctionCallExpression` | `OVER`, null treatment, `FROM` position |
| `DataType` | `Collections`, `Fields`, `ReferencedType`, `SCOPE`, `Modifier` |
| `NiladicFunctionExpression` | `LOCALTIME`, `LOCALTIMESTAMP`, `CURRENT_ROLE`, `CURRENT_CATALOG`, `CURRENT_SCHEMA`, `CURRENT_PATH` |
| `IsExpression` | JSON extras (`KindKeyword`, `UNIQUE`, `KEYS`, …) |
| identifiers | `U&"…"` and `_charset"…"` are not treated as delimited (would be lowercased) |

`StageDumper` only dumps `SelectStatement` in detail. Keep it compiling; do
not expand it unless a trace test in that step needs it.

---

## Working rules

- Match existing layout: uppercase keywords, lowercase unquoted identifiers,
  preserve delimited identifier spelling, preserve literal spelling, insert
  `AS` on bare aliases, comments stay attached to the next token.
- Every new `Query` / `Expression` / `TableSource` arm must also update
  `StartToken` when the node can appear in a select list.
- Exhaustive switches: unknown node types throw. That is the safety net.
- Round-trip: `QlParse.Sql.Parse(Sql.Format(sql))` succeeds (no error).
- Tests live in `tests/QlFmt.Tests/FormatTests.cs` (or a new file per
  later step if that file gets unwieldy). Pattern: `AssertFormatted(input,
  expected)`.
- Add `Keyword.*Upper` constants only when `WriteKeyword` needs a fixed
  spelling. Prefer uppercasing the token text (same as `WriteTypeName`) for
  long-tail keywords so `Keyword.cs` does not explode.
- Do not change qlparse in this work.

---

## Step 0 — Scaffolding

Goal: the formatter can grow without becoming one 10k-line file, and new
nodes fail loudly in tests rather than at runtime in the CLI.

1. Split `Formatter` the same way qlparse splits `Parser`:
   - `Formatter.cs` — constructor, `Format`, trivia, indent, keyword /
     identifier / literal helpers, `StartToken`
   - `Formatter.Query.cs` — `SELECT` / `WITH` / set ops / `VALUES`
   - `Formatter.Expression.cs` — `AppendExpression` and helpers
   - later steps add `Formatter.Dml.cs`, `Formatter.Ddl.cs`,
     `Formatter.From.cs`, `Formatter.Window.cs`, `Formatter.Psm.cs`,
     `Formatter.Markup.cs`, `Formatter.Graph.cs` as those domains land
2. Generalize identifier emission: a token is delimited (do not case-fold)
   when it starts with `"`, `U&"` / `u&"`, or `_…"` / `_…'`. Everything
   else still lowercases.
3. Add `tests/QlFmt.Tests/SqlStandardCoverageTests.cs` that parses each
   fixture in a step and asserts `Sql.Format` does not throw. Keep it empty
   until step 1.
4. Handle `DirectSqlScript` in `Write(Query)`: print statements separated
   by `;`, optional trailing `;` on a one-statement script stays omitted
   (today's behavior). Multi-statement scripts get `;` between statements
   and a trailing newline.

Done when existing tests still pass and `select a from t; select b from u`
formats as two statements.

---

## Step 1 — Lexical leftovers

SQL-STANDARD.md: Identifiers, Literals, Comments and tokens.

Most of this already works because the lexer preserves token text. Finish:

- Unicode delimited identifiers: `U&"foo"`, `u&"a""b"` printed unchanged
- Character-set introducers: `_latin1"ident"`, `_utf8'hello'` unchanged
- Unicode strings: `U&'…'` unchanged (already a `String` token)
- Nested block comments already preserved; add a nested-comment format test
- Interval literals / datetime literals already formatted; add `TIME` /
  `TIMESTAMP` with precision if missing from tests

No new AST nodes. Touch `AppendIdentifier` / `AppendLiteral` only.

---

## Step 2 — Data types and type operations

SQL-STANDARD.md: Data types.

Extend `WriteDataType`:

- Multi-word names already use `NameTail` (`DOUBLE PRECISION`,
  `CHAR VARYING`, `CHARACTER LARGE OBJECT`)
- `WITH TIME ZONE` already printed
- `ROW (field type, …)` from `Fields`
- `REF (type)` / `SCOPE name` from `ReferencedType` + `ScopeName`
- `ARRAY` / `MULTISET` / `MDARRAY` suffixes from `Collections` (optional
  `[n]` / dimension list)
- `Modifier` (e.g. user-defined distinct)

New expression arms:

- `TreatExpression` — `TREAT (x AS type)` (same shape as `CAST`)
- `NextValueExpression` — `NEXT VALUE FOR name`

Tests: `CAST`, `TREAT`, `NEXT VALUE FOR`, `VARCHAR(10)`, `TIMESTAMP(3)
WITH TIME ZONE`, `ROW(a INT, b TEXT)`, `INT ARRAY[3]`, `REF(person) SCOPE
people`.

---

## Step 3 — Remaining scalar expressions

SQL-STANDARD.md: Scalar expressions (operators already done; finish the
rest).

New `AppendExpression` arms:

- `SpecialFormExpression` — `UPPER` `LOWER` `CHAR_LENGTH`
  `CHARACTER_LENGTH` `OCTET_LENGTH` `BIT_LENGTH` `NORMALIZE` `FLOOR`
  `CEIL`/`CEILING` `POWER` `SQRT` `LN` `EXP` `MOD` `ABS` `WIDTH_BUCKET`
  (optional `USING` name)
- `OverlayExpression` — `OVERLAY (src PLACING rep FROM start [FOR n])`
- `ArrayQueryExpression` — `ARRAY (subquery)`
- `MultisetExpression` / `MultisetQueryExpression` /
  `MultisetOperationExpression` / `MultisetSetExpression` — constructors,
  `MULTISET UNION|EXCEPT|INTERSECT [ALL|DISTINCT]`, `SET (…)`
- `AbsentOnNullExpression` — `ABSENT ON NULL`
- `DerefExpression` / `RefValueExpression` / `DereferenceExpression`
  (`->` attribute) / `SpecifictypeExpression`
- `MethodInvocationExpression` / `StaticMethodInvocationExpression` /
  `NewSpecificationExpression`

Niladic list: add `LOCALTIME[(p)]`, `LOCALTIMESTAMP[(p)]`, `CURRENT_ROLE`,
`CURRENT_CATALOG`, `CURRENT_SCHEMA`, `CURRENT_PATH` to `NiladicKeywordText`.

`COALESCE` / `NULLIF` already work. Add tests that they stay special forms
(not generic calls) and that `COALESCE` as a function call still formats.

---

## Step 4 — Remaining predicates

SQL-STANDARD.md: Predicates.

- `SimilarExpression` — `SIMILAR TO` / `NOT SIMILAR TO` `[ESCAPE]`
- `DistinctFromExpression` — `IS [NOT] DISTINCT FROM`
- `NormalizedPredicateExpression` — `IS [NOT] [form] NORMALIZED`
- `IsExpression` extras — `IS [NOT] JSON [VALUE|ARRAY|OBJECT|SCALAR]
  [WITH|WITHOUT UNIQUE KEYS]` (fields already on the node)
- `PeriodPredicateExpression` / `PeriodExpression` — temporal period
  predicates (`CONTAINS`, `EQUALS`, `OVERLAPS`, `PRECEDES`, `SUCCEEDS`,
  `IMMEDIATELY`)

`MATCH` types: `MatchTypeText` currently only `PARTIAL`/`FULL`. Add
`SIMPLE`.

---

## Step 5 — SELECT / WITH / FROM / grouping / paging leftovers

SQL-STANDARD.md: Query expressions except Window, Locks (locks mostly
done), Recursion extras.

`WriteSelect`:

- `SELECT … INTO` targets after the select list
- `WINDOW` clause after `HAVING`
- `FETCH FIRST|NEXT {n} [PERCENT] {ROW|ROWS} {ONLY|WITH TIES}`
- `OFFSET n [ROW|ROWS]`
- `ORDER BY … NULLS FIRST|LAST`

`WriteWithQuery` / `WriteCte`:

- `SEARCH DEPTH|BREADTH FIRST BY … SET …`
- `CYCLE … SET … TO … DEFAULT … USING …`
- recursion-limit keyword if present

`AppendTable` new arms:

- `OnlyTable` — `ONLY (name)`
- `UnnestTable` — `UNNEST (…) [WITH ORDINALITY]`
- `TableFunction` — `TABLE (…)` / table function call, including
  `TABLE (VALUES …)`
- `SampledTable` — `TABLESAMPLE method (p) [REPEATABLE (n)]`
- print `LATERAL` on `DerivedTable`

Grouping: `GroupingOperationExpression` (`ROLLUP` `CUBE` `GROUPING SETS`
`GROUPING()`) and `EmptyGroupingSetExpression` (`()`).

Locks: print `FOR SHARE`.

Tests should cover comma `FROM`, parenthesized joined tables, correlation
column lists (already formatted).

---

## Step 6 — Window functions

SQL-STANDARD.md: Window.

- Print `FunctionCallExpression.Over` (`OVER name` or `OVER (…)` )
- `PARTITION BY`, window `ORDER BY`, `ROWS`/`RANGE`/`GROUPS` frame,
  frame bounds, `EXCLUDE`
- Named `WINDOW` clause (step 5) plus `OVER w`
- Null treatment `RESPECT|IGNORE NULLS` on the call
- Nested window functions are just nested calls; no extra node

New file: `Formatter.Window.cs`. Helpers: `WriteWindowSpecification`,
`WriteWindowFrame`, `WriteWindowFrameBound`.

Tests: `RANK() OVER (PARTITION BY a ORDER BY b ROWS BETWEEN UNBOUNDED
PRECEDING AND CURRENT ROW EXCLUDE TIES)`, `LAG`/`LEAD`/`NTILE`,
`WINDOW w AS (ORDER BY a)`.

---

## Step 7 — Finish DML

SQL-STANDARD.md: DML.

New file: `Formatter.Dml.cs`.

- `InsertStatement`: `OVERRIDING SYSTEM|USER VALUE`, `DEFAULT VALUES`
- `UpdateStatement` searched and positioned (`WHERE CURRENT OF`)
- `DeleteStatement` searched and positioned
- `MergeStatement` (`WHEN MATCHED` / `WHEN NOT MATCHED` arms)
- `TruncateStatement`
- `SetAssignmentStatement` / row `SET` assignments used by UPDATE/PSM

`Write(Query)` must dispatch these. `INSERT` already does; extend it, do
not rewrite.

---

## Step 8 — Schema / DDL and constraints

SQL-STANDARD.md: Schema / DDL, Constraints.

New file: `Formatter.Ddl.cs`. One writer per statement type. Shared
helpers for qualified names, column lists, and constraint clauses.

Statements (all `Query` subclasses already in `SqlVisitor`):

- Schema/catalog: `SchemaDefinition`, `AlterSchemaStatement`,
  `DropSchemaStatement`
- Tables: `CreateTableStatement` (including `AS`, `OF`, `GLOBAL|LOCAL`
  temporary, `LIKE`, generated / identity columns, period definitions),
  `AlterTableStatement`, `DropTableStatement`
- Views: `CreateViewStatement` (`WITH [CASCADED|LOCAL] CHECK OPTION`),
  `AlterViewStatement`, `DropViewStatement`
- Domains/types: `Create`/`Alter`/`DropDomainStatement`,
  `CreateTypeStatement` (distinct / structured / array / multiset),
  `DropTypeStatement`, `CreateOrderingStatement`, `CreateCastStatement`,
  `CreateTransformStatement`
- Other objects: assertion, character set, collation, translation,
  sequence, index, `CommentStatement`

Constraints print as part of table/domain/alter:

- `NOT NULL` `UNIQUE` `PRIMARY KEY` `FOREIGN KEY` `CHECK` `DEFAULT`
- `MATCH SIMPLE|PARTIAL|FULL`, referential actions
- `DEFERRABLE` / `INITIALLY DEFERRED|IMMEDIATE`
- named constraints
- `UNIQUE NULLS [NOT] DISTINCT`

Indent column definitions one level, one column/constraint per line,
trailing commas except the last — same as today's INSERT column list.

---

## Step 9 — Privileges, transactions, sessions, connections

SQL-STANDARD.md: Privileges and roles; Transactions and sessions;
Connections.

Mostly one-line statements. Print keywords uppercase, names lowercase,
lists comma-separated.

- `GRANT` / `REVOKE` privilege and role, `WITH GRANT OPTION` /
  `WITH ADMIN OPTION`
- `CREATE ROLE` `DROP ROLE` `SET ROLE`
- `START TRANSACTION` / `SET TRANSACTION` (isolation, `READ ONLY` /
  `READ WRITE`, `DIAGNOSTICS SIZE`)
- `COMMIT` / `ROLLBACK` `[WORK]` `[AND [NO] CHAIN]`
- `SAVEPOINT` `RELEASE SAVEPOINT` `ROLLBACK TO`
- `SET CONSTRAINTS` `SET SESSION AUTHORIZATION` /
  `SET SESSION CHARACTERISTICS` `SET NAMES` charset/collation
  `SET TIME ZONE` `SET CATALOG` `SET SCHEMA` `SET PATH`
- `CONNECT` `DISCONNECT` `SET CONNECTION`

New file: `Formatter.Session.cs` (or fold into `Formatter.Ddl.cs` if it
stays small).

---

## Step 10 — Cursors, dynamic SQL, diagnostics

SQL-STANDARD.md: Cursors, Dynamic SQL, Diagnostics.

- `DECLARE CURSOR` (sensitivity / scroll / hold / return)
- `OPEN` `FETCH` `CLOSE`
- `ALLOCATE` / `DEALLOCATE`
- `PREPARE` `EXECUTE` `EXECUTE IMMEDIATE` `DESCRIBE`
- dynamic `DECLARE CURSOR`, `ALLOCATE DESCRIPTOR`
- `GET DIAGNOSTICS`
- `SIGNAL` / `RESIGNAL`

Positioned `UPDATE`/`DELETE` already in step 7.

New file: `Formatter.Cursor.cs`.

---

## Step 11 — Triggers, routines, SQL/PSM

SQL-STANDARD.md: Triggers and routines; SQL/PSM.

New file: `Formatter.Psm.cs`.

Routines:

- `CREATE FUNCTION|PROCEDURE|METHOD` with parameters, return type,
  external vs SQL, determinism / null-call / data access clauses
- `ALTER` / `DROP` routine
- `CALL` `RETURN`

Triggers:

- `CREATE TRIGGER` `BEFORE`/`AFTER`/`INSTEAD OF`
- `DROP TRIGGER`

PSM (body of SQL routines and standalone):

- `BEGIN … END` (`CompoundStatement`) — indent body
- `DECLARE` variable / condition / handler
- `SET` assignment
- `IF` `CASE` statements (not the expression `CASE`)
- `LOOP` `WHILE` `REPEAT` `FOR`
- `LEAVE` `ITERATE`

Compound statements nest; reuse `Write(Query)` for the body list.

---

## Step 12 — Typed tables, sequences, temporal

SQL-STANDARD.md: Typed tables and UDTs; Sequences and identity; Temporal.

Much of this is extra clauses on nodes from steps 2, 5, 7, 8:

- Typed tables / `UNDER` / `REF` / `DEREF` / `SCOPE` / `SPECIFICTYPE` /
  method invocations (expressions in step 3; `CREATE TABLE OF` in step 8)
- `CREATE SEQUENCE`, identity column options, `OVERRIDING` (steps 7–8)
- `SystemTimeClause` on `TableReference` / `OnlyTable`:
  `FOR SYSTEM_TIME AS OF` / `BETWEEN` / `FROM … TO` / `ALL`
- `PortionClause` on DML
- Application-time periods on `CREATE TABLE`

Add the remaining printers and tests here rather than reopening earlier
files blindly. If a field was left null-unchecked in step 5/7/8, this
step fills it.

---

## Step 13 — SQL/XML and JSON

SQL-STANDARD.md: SQL/XML; JSON.

New file: `Formatter.Markup.cs`.

Expressions:

- `MarkupCallExpression` — `XMLPARSE` `XMLSERIALIZE` `XMLELEMENT`
  `XMLATTRIBUTES` `XMLFOREST` `XMLCONCAT` `XMLAGG` `XMLCOMMENT` `XMLPI`
  `XMLDOCUMENT` `XMLQUERY` `XMLEXISTS` `XMLCAST` `XMLVALIDATE`
  `JSON_OBJECT` `JSON_ARRAY` `JSON_OBJECTAGG` `JSON_ARRAYAGG`
  `JSON_VALUE` `JSON_QUERY` `JSON_EXISTS` `JSON_SERIALIZE` `JSON_SCALAR`
- `JsonAccessorExpression` — `[path]` simplified accessor

Table sources:

- `MarkupTable` — `XMLTABLE` / `JSON_TABLE` with column definitions

Predicates: `IS [NOT] DOCUMENT` / `IS [NOT] CONTENT` / `IS [NOT] JSON`
(step 4 may already cover JSON).

JSON arrows already format. Do not restyle `->` / `->>`.

---

## Step 14 — MATCH_RECOGNIZE, PTF, MDA, SQL/PGQ

SQL-STANDARD.md: Row pattern matching; Polymorphic table functions;
SQL/MDA; SQL/PGQ.

New files: `Formatter.Pattern.cs`, `Formatter.Graph.cs`. MDA constructors
can live in `Formatter.Expression.cs`.

- `MatchRecognizeTable` plus `RowPattern` tree (`PatternPrimary`,
  concatenation, alternation, quantified, group, exclusion, permute),
  `MEASURES`, `DEFINE`, `SUBSET`, `ONE ROW`/`ALL ROWS PER MATCH`,
  `AFTER MATCH SKIP`
- `PtfTable` / `PtfArgument` — copartitioning, row vs table semantics
- `MdarrayConstructorExpression` / `MdarraySliceExpression` /
  `MdarrayAggregateExpression` / `MdarrayAxis`
- `CreatePropertyGraphStatement` / `DropPropertyGraphStatement`
- `GraphTable` / `GraphTableElement` / `GraphElement` / `GraphProperty`
  — `GRAPH_TABLE`, `MATCH` graph patterns, quantified paths, labels

These are large, self-contained printers. Keep each in its own file.

---

## Step 15 — Direct SQL, modules, embedded SQL

SQL-STANDARD.md: Direct SQL and modules; Information schema.

- `DirectSqlScript` already in step 0; add tests for optional trailing `;`
  and mixed statement kinds
- `ModuleDefinition` / `ModuleProcedure` — module header, path,
  contained procedures
- `EmbeddedSqlStatement` — `EXEC SQL … END-EXEC` / `;`
- `DeclareSectionStatement` — `BEGIN|END DECLARE SECTION`
- `WheneverStatement`

`INFORMATION_SCHEMA` is ordinary identifiers; no extra syntax. Add one
test that `information_schema.tables` lowercases like any other name.

---

## Step 16 — Coverage close-out

1. Walk `../qlparse/SQL-STANDARD.md` and tick a qlfmt copy
   (`SQL-STANDARD.md` in this repo) only when a format test exists for
   that item.
2. Exhaustiveness test: reflect every `Query` / `Expression` /
   `TableSource` subclass in `QlParse` and assert `Formatter` has a
   switch arm (or a known-skip list that must be empty at the end).
3. Round-trip a combined fixture that concatenates one example per
   SQL-STANDARD.md section with `;`.
4. `sample.sql` may grow, but do not dump the whole standard into it.

Done when every `[x]` in qlparse's `SQL-STANDARD.md` has a matching
formatter test, `dotnet test tests/QlFmt.Tests` is green, and
`Sql.Format` never throws `Unknown …` on parser-accepted input.

---

## Suggested order vs. size

| Step | Domain | Rough size |
| --- | --- | --- |
| 0 | Split + scripts + identifier prefixes | small |
| 1 | Lexical | small |
| 2 | Types | small |
| 3 | Scalar expressions | medium |
| 4 | Predicates | small |
| 5 | SELECT/FROM leftovers | medium |
| 6 | Window | medium |
| 7 | DML | medium |
| 8 | DDL + constraints | large |
| 9 | Privileges / session | small |
| 10 | Cursors / dynamic | medium |
| 11 | PSM / routines / triggers | large |
| 12 | UDT / sequence / temporal | medium |
| 13 | XML / JSON | medium |
| 14 | Pattern / PTF / MDA / graph | large |
| 15 | Modules / embedded | small |
| 16 | Checklist | small |

Do not start a later step until the earlier step's tests are green.
Steps 9 and 10 are independent of 8 and can swap if DDL is blocking.
Steps 13 and 14 are independent of each other once steps 0–7 exist.
