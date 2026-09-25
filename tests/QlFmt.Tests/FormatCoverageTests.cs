using QlParse;

namespace QlFmt.Tests;

public sealed class FormatCoverageTests
{
    private const string FormatterProjectRelative = "../../../../../src/QlFmt";
    private const string FormatterSources = "Formatter*.cs";
    private const string CreateTableUpper = "CREATE TABLE";
    private const string InsertIntoUpper = "INSERT INTO";
    private const string CommitUpper = "COMMIT";
    private const string CaseArmPrefix = "case ";
    public static TheoryData<string> Samples() =>
    [
        "create schema s authorization u",
        "alter schema cat.s rename to t",
        "drop schema s cascade",
        "create table s.t (a integer not null, b character varying(10), primary key (a))",
        "create table t (a, b) as select 1, 2 with data",
        "create global temporary table t (a integer) on commit preserve rows",
        "create table t (like u including defaults, a integer)",
        "create table emp of person (ref is id system generated)",
        "alter table t add column a integer",
        "alter table t drop column a cascade",
        "alter table t add primary key (a)",
        "alter table t drop constraint c restrict",
        "alter table t alter column a set default 1",
        "alter table emp add period for dept_period (valid_start, valid_end)",
        "alter table emp add system versioning",
        "drop table s.t cascade",
        "create view s.v (a, b) as select 1, 2",
        "create view v as select 1 with cascaded check option",
        "alter view s.v as select 1",
        "drop view s.v cascade",
        "create domain d as integer default 1 check (value > 0)",
        "alter domain d set default 1",
        "drop domain d restrict",
        "create type emp_id as integer final",
        "create type phones as character varying(20) array[5]",
        "drop type person restrict",
        "create ordering for person equals only by state",
        "create cast (integer as emp_id) with function int_to_emp as assignment",
        "create assertion a check (1 = 1)",
        "drop assertion a cascade",
        "create character set cs as get latin1",
        "drop character set cs",
        "create collation c for latin1 from latin1",
        "drop collation c cascade",
        "create translation tr for latin1 to utf8 from existing",
        "drop translation tr",
        "create sequence s as integer start with 1 increment by 1",
        "drop sequence s cascade",
        "create unique index i on t (a desc)",
        "alter index i rename to j",
        "drop index i",
        "comment on table t is 'note'",
        "grant select, insert (a) on table t to u with grant option",
        "grant r1 to u with admin option",
        "revoke select on table t from u cascade",
        "revoke r from u cascade",
        "create role r",
        "drop role r",
        "set role none",
        "start transaction isolation level serializable, read only",
        "set transaction read write",
        "commit work and chain",
        "rollback to savepoint s",
        "savepoint s",
        "release savepoint s",
        "set constraints all deferred",
        "set session authorization u",
        "set session characteristics as isolation level serializable, read only",
        "set names utf8",
        "set time zone local",
        "set catalog c",
        "set schema s",
        "set path s",
        "connect to db as c user u",
        "disconnect all",
        "set connection default",
        "declare c cursor for select 1",
        "open c",
        "fetch next from c into x",
        "close c",
        "allocate c cursor for s",
        "deallocate prepare s",
        "prepare s from 'select 1'",
        "execute s",
        "execute immediate 'select 1'",
        "describe s using sql descriptor d",
        "allocate descriptor d",
        "get diagnostics x = number",
        "signal sqlstate '45000'",
        "resignal",
        "create trigger tr after insert on t for each row select 1",
        "drop trigger tr",
        "create function sch.f(x integer) returns integer language sql deterministic sql return x + 1",
        "drop procedure p cascade",
        "call p(1)",
        "return null",
        "begin set a = 1; end",
        "leave lab",
        "iterate lab",
        "create property graph g vertex tables ( person )",
        "drop property graph g cascade",
        "exec sql select 1;",
        "end declare section",
        "whenever sqlerror continue",
        "select * from t for system_time as of current_timestamp",
        "select * from information_schema.tables",
        "select xmlelement (name e, a) from t",
        "select json_scalar ( 1 ) from t",
        "select j[0] from t",
        "select mdarray_sum (a) from t",
        "select * from xmltable ('/emp' passing x columns id integer) as e",
        "select * from t match_recognize (pattern (a) define a as a > 0)",
        "select mdarray [ 0 : 1 ] ( 1 , 2 ) from t",
        "select xmlexists ('/a' passing x) from t",
        "select x is document from t",
        """
        module mod1 names are latin1 language c schema sch authorization u path sch, other
        declare c cursor for select 1;
        procedure p (x integer);
        select x;
        """,
    ];

    [Theory]
    [MemberData(nameof(Samples))]
    public void Formats_without_throwing(string sql)
    {
        var formatted = Sql.Format(sql);
        Assert.False(string.IsNullOrWhiteSpace(formatted));
        var parsed = QlParse.Sql.Parse(formatted, SqlFlags.AtParameters);
        Assert.Null(parsed.Error);
    }

    [Fact]
    public void Formats_create_table()
    {
        Assert.Equal(
            """
            CREATE TABLE t (
                a INTEGER,
                b VARCHAR(10)
            )
            """,
            Sql.Format("create table t (a integer, b varchar(10))"));
    }

    [Fact]
    public void Formats_grant_and_commit()
    {
        Assert.Equal("GRANT SELECT ON TABLE t TO u WITH GRANT OPTION", Sql.Format("grant select on table t to u with grant option"));
        Assert.Equal("COMMIT WORK AND CHAIN", Sql.Format("commit work and chain"));
    }

    [Fact]
    public void Formats_information_schema_as_identifiers()
    {
        Assert.Equal(
            """
            SELECT
                *
            FROM information_schema.tables
            """,
            Sql.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES"));
    }

    [Fact]
    public void Switch_covers_all_query_expression_and_table_types()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, FormatterProjectRelative));
        var text = string.Join('\n', Directory.GetFiles(root, FormatterSources).Select(File.ReadAllText));
        foreach (var type in typeof(Query).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsNotPublic)
            {
                continue;
            }

            if (!type.IsSubclassOf(typeof(Query))
                && !type.IsSubclassOf(typeof(Expression))
                && !type.IsSubclassOf(typeof(TableSource)))
            {
                continue;
            }

            Assert.Contains(CaseArmPrefix + type.Name, text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Formats_mixed_script()
    {
        var formatted = Sql.Format("create table t (a integer); insert into t values (1); commit;");
        Assert.Contains(CreateTableUpper, formatted, StringComparison.Ordinal);
        Assert.Contains(InsertIntoUpper, formatted, StringComparison.Ordinal);
        Assert.Contains(CommitUpper, formatted, StringComparison.Ordinal);
        Assert.Null(QlParse.Sql.Parse(formatted, SqlFlags.AtParameters).Error);
    }
}
