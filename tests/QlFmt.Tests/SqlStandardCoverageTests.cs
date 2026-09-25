using QlParse;

namespace QlFmt.Tests;

public sealed class SqlStandardCoverageTests
{
    [Theory]
    [InlineData("""select U&"foo" from t""")]
    [InlineData("""select _latin1"Foo" from t""")]
    [InlineData("select _utf8'hello' from t")]
    [InlineData(@"select U&'\0061' from t")]
    [InlineData("select a /* outer /* inner */ still */ from t")]
    [InlineData("select treat(x as int) from t")]
    [InlineData("select next value for seq from t")]
    [InlineData("select cast(x as row(a int, b varchar(10))) from t")]
    [InlineData("select cast(y as integer array[3]) from t")]
    [InlineData("select cast(z as ref(foo) scope t) from t")]
    [InlineData("select upper(x), overlay(x placing y from 1) from t")]
    [InlineData("select array(select a from t)")]
    [InlineData("select multiset[1], a multiset union all b from t")]
    [InlineData("select deref(p.manager), new sch.person(p.first_name) from t")]
    [InlineData("select localtime, current_role")]
    [InlineData("select a from t; select b from u")]
    [InlineData("select a from t where x similar to 'a+'")]
    [InlineData("select a from t where x is distinct from y")]
    [InlineData("select a from t where j is not json object with unique keys")]
    [InlineData("select a from t where period (a, b) contains period (c, d)")]
    [InlineData("select a, b into x from t")]
    [InlineData("select a from t order by a nulls first fetch next row only")]
    [InlineData("select * from unnest(a) with ordinality as x (n)")]
    [InlineData("select grouping(a) from t group by rollup (a), cube (b)")]
    [InlineData("select rank() over (partition by a order by b) from t")]
    [InlineData("insert into t default values")]
    [InlineData("update t set a = 1 where a > 0")]
    [InlineData("delete from t where a = 1")]
    [InlineData("merge into t using s on t.id = s.id when matched then delete")]
    [InlineData("truncate table t")]
    [InlineData("set a = 1")]
    public void Formats_without_throwing_and_round_trips(string sql)
    {
        var formatted = Sql.Format(sql);
        var parsed = QlParse.Sql.Parse(formatted, SqlFlags.AtParameters);
        Assert.Null(parsed.Error);
    }
}
