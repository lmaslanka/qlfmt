namespace QlFmt.Tests;

public sealed class FormatQueryDmlTests
{
    [Fact]
    public void Formats_similar_to()
    {
        AssertFormatted(
            "select a from t where x similar to 'a+' and y not similar to 'a+' escape '/'",
            """
            SELECT
                a
            FROM t
            WHERE x SIMILAR TO 'a+'
                AND y NOT SIMILAR TO 'a+' ESCAPE '/'
            """);
    }

    [Fact]
    public void Formats_is_distinct_from()
    {
        AssertFormatted(
            "select a from t where x is distinct from y or z is not distinct from w",
            """
            SELECT
                a
            FROM t
            WHERE x IS DISTINCT FROM y OR z IS NOT DISTINCT FROM w
            """);
    }

    [Fact]
    public void Formats_is_normalized()
    {
        AssertFormatted(
            "select a from t where x is normalized and y is not nfc normalized",
            """
            SELECT
                a
            FROM t
            WHERE x IS NORMALIZED
                AND y IS NOT NFC NORMALIZED
            """);
    }

    [Fact]
    public void Formats_is_json_extras()
    {
        AssertFormatted(
            "select a from t where j is not json object with unique keys",
            """
            SELECT
                a
            FROM t
            WHERE j IS NOT JSON OBJECT WITH UNIQUE KEYS
            """);
    }

    [Fact]
    public void Formats_period_predicates()
    {
        AssertFormatted(
            "select a from t where period (a, b) contains period (c, d) and period (a, b) immediately precedes period (c, d)",
            """
            SELECT
                a
            FROM t
            WHERE PERIOD(a, b) CONTAINS PERIOD(c, d)
                AND PERIOD(a, b) IMMEDIATELY PRECEDES PERIOD(c, d)
            """);
    }

    [Fact]
    public void Formats_select_into()
    {
        AssertFormatted(
            "select a, b into x, ?, :host from t",
            """
            SELECT
                a,
                b
            INTO x, ?, :host
            FROM t
            """);
    }

    [Fact]
    public void Formats_order_nulls_offset_fetch()
    {
        AssertFormatted(
            "select a from t order by a nulls first, b desc nulls last offset 5 rows fetch first 10 percent rows with ties",
            """
            SELECT
                a
            FROM t
            ORDER BY a NULLS FIRST, b DESC NULLS LAST
            OFFSET 5 ROWS
            FETCH FIRST 10 PERCENT ROWS WITH TIES
            """);
    }

    [Fact]
    public void Formats_fetch_next_row_only()
    {
        AssertFormatted(
            "select a from t fetch next row only",
            """
            SELECT
                a
            FROM t
            FETCH NEXT ROW ONLY
            """);
    }

    [Fact]
    public void Formats_search_cycle_and_recursion_limit()
    {
        AssertFormatted(
            "with recursive linear cte as (select 1) search depth first by id set seq cycle id set is_cycle to true default false using path select * from cte",
            """
            WITH RECURSIVE LINEAR
                cte AS (
                    SELECT
                        1
                ) SEARCH DEPTH FIRST BY id SET seq CYCLE id SET is_cycle TO TRUE DEFAULT FALSE USING path
            SELECT
                *
            FROM cte
            """);
    }

    [Fact]
    public void Formats_from_only_lateral_unnest_tablesample_and_table_function()
    {
        AssertFormatted(
            "select * from only (catalog.schema.person) as p (a, b), lateral (select 1) as l (x), unnest(a, b) with ordinality as u (i, n), t tablesample system (5), table(gen(1)) as tf (id)",
            """
            SELECT
                *
            FROM ONLY (catalog.schema.person) AS p(a, b), LATERAL (
                SELECT
                    1
            ) AS l(x), UNNEST(a, b) WITH ORDINALITY AS u(i, n), t TABLESAMPLE SYSTEM (5), TABLE(gen(1)) AS tf(id)
            """);
    }

    [Fact]
    public void Formats_grouping_sets()
    {
        AssertFormatted(
            "select grouping(a, b) from t group by a, rollup (b, c), cube (d), grouping sets ((a, b), ())",
            """
            SELECT
                GROUPING(a, b)
            FROM t
            GROUP BY a, ROLLUP(b, c), CUBE(d), GROUPING SETS((a, b), ())
            """);
    }

    [Fact]
    public void Formats_for_share()
    {
        AssertFormatted(
            "select a from t for share of a, b",
            """
            SELECT
                a
            FROM t
            FOR SHARE OF a, b
            """);
    }

    [Fact]
    public void Formats_window_functions()
    {
        AssertFormatted(
            "select rank() over (partition by a order by b desc nulls last rows between unbounded preceding and current row exclude ties), dense_rank() over (), percent_rank() over w, lag(a, 1, 0) ignore nulls over (order by a), nth_value(a, 2) from last over (groups between 1 preceding and 1 following exclude current row) from t window w as (partition by a), w2 as (w order by b)",
            """
            SELECT
                rank() OVER (PARTITION BY a ORDER BY b DESC NULLS LAST ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW EXCLUDE TIES),
                dense_rank() OVER (),
                percent_rank() OVER w,
                lag(a, 1, 0) IGNORE NULLS OVER (ORDER BY a),
                nth_value(a, 2) FROM LAST OVER (GROUPS BETWEEN 1 PRECEDING AND 1 FOLLOWING EXCLUDE CURRENT ROW)
            FROM t
            WINDOW w AS (PARTITION BY a), w2 AS (w ORDER BY b)
            """);
    }

    [Fact]
    public void Formats_insert_default_values_and_overriding()
    {
        AssertFormatted(
            "insert into t default values",
            """
            INSERT INTO t
            DEFAULT VALUES
            """);
        AssertFormatted(
            "insert into t overriding user value values (1)",
            """
            INSERT INTO t OVERRIDING USER VALUE
            VALUES
                (1)
            """);
    }

    [Fact]
    public void Formats_update_and_delete()
    {
        AssertFormatted(
            "update only (s.t) as x set a = 1, b = default where a > 0",
            """
            UPDATE ONLY (s.t) AS x SET a = 1, b = DEFAULT
            WHERE a > 0
            """);
        AssertFormatted(
            "delete from t where current of c1",
            """
            DELETE FROM t
            WHERE CURRENT OF c1
            """);
    }

    [Fact]
    public void Formats_update_row_assignments()
    {
        AssertFormatted(
            "update t set a = default, a[1] = 2, (b, c) = (1, 2)",
            """
            UPDATE t SET a = DEFAULT, a[1] = 2, (b, c) = (1, 2)
            """);
    }

    [Fact]
    public void Formats_merge()
    {
        AssertFormatted(
            """
            merge into t as tgt
            using s
            on tgt.id = s.id
            when matched then update set z = default
            when not matched then insert values (1)
            """,
            """
            MERGE INTO t AS tgt
            USING s
            ON tgt.id = s.id
            WHEN MATCHED THEN UPDATE SET z = DEFAULT
            WHEN NOT MATCHED THEN INSERT VALUES (1)
            """);
    }

    [Fact]
    public void Formats_truncate()
    {
        AssertFormatted(
            "truncate table cat.sch.t restart identity",
            """
            TRUNCATE TABLE cat.sch.t RESTART IDENTITY
            """);
    }

    [Fact]
    public void Formats_set_assignment()
    {
        AssertFormatted(
            "set a = 1",
            """
            SET a = 1
            """);
    }

    private static void AssertFormatted(string sql, string expected) =>
        Assert.Equal(expected, Sql.Format(sql));
}
