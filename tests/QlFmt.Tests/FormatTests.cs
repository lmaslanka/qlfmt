using QlFmt;

namespace QlFmt.Tests;

public sealed class FormatTests
{
    [Fact]
    public void Formats_select_from()
    {
        AssertFormatted(
            "select a from t",
            """
            SELECT
                a
            FROM t
            """);
    }

    [Fact]
    public void Formats_multiple_select_columns()
    {
        AssertFormatted(
            "select a,b from t",
            """
            SELECT
                a,
                b
            FROM t
            """);
    }

    [Fact]
    public void Formats_where_equality()
    {
        AssertFormatted(
            "select a from t where x=1",
            """
            SELECT
                a
            FROM t
            WHERE x = 1
            """);
    }

    [Fact]
    public void Formats_where_and_or_with_precedence()
    {
        AssertFormatted(
            "select a from t where x=1 or y=2 and z=3",
            """
            SELECT
                a
            FROM t
            WHERE x = 1 OR y = 2 AND z = 3
            """);
    }

    [Fact]
    public void Preserves_line_and_block_comments()
    {
        AssertFormatted(
            """
            select a -- col
            from /* table */ t
            """,
            """
            SELECT
                a -- col
            FROM /* table */ t
            """);
    }

    [Fact]
    public void Uppercases_keywords_and_lowercases_identifiers()
    {
        AssertFormatted(
            "Select A From T Where X=1",
            """
            SELECT
                a
            FROM t
            WHERE x = 1
            """);
    }

    [Fact]
    public void Formats_qualified_names_alias_and_drops_semicolon()
    {
        AssertFormatted(
            "SELECT p.first_name, p.last_name, p.age, p.eye_color FROM person As p WHERE p.record_id = 42;",
            """
            SELECT
                p.first_name,
                p.last_name,
                p.age,
                p.eye_color
            FROM person AS p
            WHERE p.record_id = 42
            """);
    }

    [Fact]
    public void Formats_bare_table_alias_with_as()
    {
        AssertFormatted(
            "select a from person p",
            """
            SELECT
                a
            FROM person AS p
            """);
    }

    [Fact]
    public void Formats_sample_query()
    {
        AssertFormatted(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "sample.sql")),
            """
            -- person directory extract
            SELECT DISTINCT
                *,
                p.*,
                p.record_id,
                p.first_name AS given_name,
                p.last_name,
                p.first_name || ' ' || p.last_name AS full_name,
                ARRAY[p.record_id::TEXT],
                p.notes->'city'->>'name',
                p.age,
                p.eye_color,
                p.email,
                p.phone_number,
                public.person.status,
                count(*) FILTER (WHERE p.age > 17),
                TRUE AS is_active,
                FALSE AS is_cycle,
                coalesce(o.job_title, 'none') AS title,
                CASE /* age band */
                    WHEN p.age < 18 THEN 'minor'
                    WHEN p.age >= 65 THEN 'senior'
                    ELSE 'adult'
                END AS age_group,
                a.city,
                a.province,
                a.country,
                a.postal_code,
                o.department,
                o.salary,
                CAST(o.salary AS NUMERIC),
                o.salary::NUMERIC(10, 2),
                o.salary * 1.1 AS raised,
                (
                    SELECT
                        max(salary)
                    FROM occupation
                ) AS cap
            FROM public.person AS p
            INNER JOIN public.address AS a
                ON p.address_id = a.address_id
            LEFT JOIN occupation AS o
                ON p.occupation_id = o.occupation_id
            RIGHT OUTER JOIN department AS d
                ON o.department_id = d.department_id
            FULL JOIN region AS r
                ON a.region_id = r.region_id
            JOIN country AS c
                ON a.country_id = c.country_id
            CROSS JOIN calendar AS cal
            NATURAL LEFT JOIN status_flag
            NATURAL JOIN audit
            INNER JOIN extra
                USING (record_id, tenant_id)
            JOIN (
                SELECT
                    record_id
                FROM watchlist
            ) AS w
                ON p.record_id = w.record_id
            WHERE p.record_id != 0
                AND p.age BETWEEN 18 AND 65
                AND p.age NOT BETWEEN 0 AND 1
                AND p.eye_color IN ('blue', 'green', 'hazel', 'brown')
                AND p.status = 'active'
                AND p.last_name LIKE 'S%'
                AND p.email NOT LIKE '%test%'
                AND a.country = 'Canada'
                AND p.age > 17
                AND p.age >= 18
                AND p.age < 66
                AND p.age <= 65
                AND p.record_id != 1
                AND NOT a.city IS NULL
                AND (o.salary IS NULL OR o.salary >= 50000)
                AND o.job_title IS NOT NULL
                AND p.record_id IN (
                    SELECT
                        record_id
                    FROM watchlist
                )
                AND p.record_id NOT IN (
                    SELECT
                        person_id
                    FROM ban
                )
                AND EXISTS (
                    SELECT
                        1
                    FROM login
                    WHERE login.person_id = p.record_id
                )
                AND NOT EXISTS (
                    SELECT
                        1
                    FROM ban
                    WHERE ban.person_id = p.record_id
                )
                AND o.salary > ALL (
                    SELECT
                        min_salary
                    FROM pay_band
                )
                AND o.salary >= ANY (
                    SELECT
                        mid_salary
                    FROM pay_band
                )
                AND p.age < SOME (
                    SELECT
                        max_age
                    FROM age_limit
                )
            GROUP BY p.record_id, p.first_name, p.last_name, p.age, p.eye_color, p.email, p.phone_number, public.person.status, a.city, a.province, a.country, a.postal_code, o.department, o.salary, o.job_title
            HAVING count(*) != 0
                AND NOT p.age IS NOT NULL
            UNION ALL
            SELECT
                p.record_id,
                p.first_name,
                p.last_name
            FROM public.person AS p
            WHERE p.status = 'alumni'
            INTERSECT
            SELECT
                p.record_id,
                p.first_name,
                p.last_name
            FROM public.person AS p
            WHERE p.record_id IN (
                SELECT
                    person_id
                FROM alumni
            )
            EXCEPT
            SELECT
                p.record_id,
                p.first_name,
                p.last_name
            FROM public.person AS p
            WHERE EXISTS (
                SELECT
                    1
                FROM suppress
                WHERE suppress.person_id = p.record_id
            )
            ORDER BY p.last_name DESC, p.first_name ASC, p.record_id
            LIMIT 10
            OFFSET 5
            """);
    }

    [Fact]
    public void Formats_union_join()
    {
        AssertFormatted(
            "select a from t union join u",
            """
            SELECT
                a
            FROM t
            UNION JOIN u
            """);
    }

    [Fact]
    public void Formats_union_join_with_other_joins()
    {
        AssertFormatted(
            "select a from t join u on t.id=u.id union join v",
            """
            SELECT
                a
            FROM t
            JOIN u
                ON t.id = u.id
            UNION JOIN v
            """);
    }

    [Fact]
    public void Rejects_union_join_on()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("select a from t union join u on t.id=u.id"));
        Assert.Equal("ON is not valid with NATURAL, CROSS, or UNION JOIN", error.Message);
    }

    [Fact]
    public void Formats_all_join_kinds()
    {
        AssertFormatted(
            "select a from t join u on t.id=u.id right outer join v as v on t.id=v.id full join w on t.id=w.id cross join x natural left join y natural join z inner join q using (id, name)",
            """
            SELECT
                a
            FROM t
            JOIN u
                ON t.id = u.id
            RIGHT OUTER JOIN v AS v
                ON t.id = v.id
            FULL JOIN w
                ON t.id = w.id
            CROSS JOIN x
            NATURAL LEFT JOIN y
            NATURAL JOIN z
            INNER JOIN q
                USING (id, name)
            """);
    }

    [Fact]
    public void Formats_star_column_alias_schema_group_order_limit()
    {
        AssertFormatted(
            "select *, p.name as n, public.person.x from public.person p group by p.name, p.id order by p.name desc, p.id asc limit 10",
            """
            SELECT
                *,
                p.name AS n,
                public.person.x
            FROM public.person AS p
            GROUP BY p.name, p.id
            ORDER BY p.name DESC, p.id ASC
            LIMIT 10
            """);
    }

    [Fact]
    public void Formats_qualified_star()
    {
        AssertFormatted(
            "select t.* from t",
            """
            SELECT
                t.*
            FROM t
            """);
    }

    [Fact]
    public void Formats_functions_not_is_not_null_not_equals_having_offset()
    {
        AssertFormatted(
            "select count(*), coalesce(p.name,'x') from p group by p.id having count(*)!=0 and not p.id is not null offset 5 limit 10",
            """
            SELECT
                count(*),
                coalesce(p.name, 'x')
            FROM p
            GROUP BY p.id
            HAVING count(*) != 0
                AND NOT p.id IS NOT NULL
            LIMIT 10
            OFFSET 5
            """);
    }

    [Fact]
    public void Formats_case_when()
    {
        AssertFormatted(
            "select case when p.age < 18 then 'minor' when p.age >= 65 then 'senior' else 'adult' end as age_group from p",
            """
            SELECT
                CASE
                    WHEN p.age < 18 THEN 'minor'
                    WHEN p.age >= 65 THEN 'senior'
                    ELSE 'adult'
                END AS age_group
            FROM p
            """);
    }

    [Fact]
    public void Formats_arithmetic_and_unary_minus()
    {
        AssertFormatted(
            "select a+b*c, -p.age, o.salary * 1.1 as raised from t",
            """
            SELECT
                a + b * c,
                -p.age,
                o.salary * 1.1 AS raised
            FROM t
            """);
    }

    [Fact]
    public void Formats_derived_table_in_from()
    {
        AssertFormatted(
            "select x.a from (select a from t) as x",
            """
            SELECT
                x.a
            FROM (
                SELECT
                    a
                FROM t
            ) AS x
            """);
    }

    [Fact]
    public void Formats_derived_table_in_join()
    {
        AssertFormatted(
            "select t.a from t join (select id from u) x on t.id=x.id",
            """
            SELECT
                t.a
            FROM t
            JOIN (
                SELECT
                    id
                FROM u
            ) AS x
                ON t.id = x.id
            """);
    }

    [Fact]
    public void Formats_nested_subqueries()
    {
        AssertFormatted(
            "select x.a from (select (select b from t) as a from u) x where x.a in (select id from v)",
            """
            SELECT
                x.a
            FROM (
                SELECT
                    (
                        SELECT
                            b
                        FROM t
                    ) AS a
                FROM u
            ) AS x
            WHERE x.a IN (
                SELECT
                    id
                FROM v
            )
            """);
    }

    [Fact]
    public void Formats_parenthesized_scalar_subquery()
    {
        AssertFormatted(
            "select ((select a from t)) from u",
            """
            SELECT
                ((
                    SELECT
                        a
                    FROM t
                ))
            FROM u
            """);
    }

    [Fact]
    public void Formats_union_corresponding()
    {
        AssertFormatted(
            "select a from t union corresponding select a from u",
            """
            SELECT
                a
            FROM t
            UNION CORRESPONDING
            SELECT
                a
            FROM u
            """);
    }

    [Fact]
    public void Formats_union_all_corresponding_by()
    {
        AssertFormatted(
            "select a, b from t union all corresponding by (a, b) select a, b from u",
            """
            SELECT
                a,
                b
            FROM t
            UNION ALL CORRESPONDING BY (a, b)
            SELECT
                a,
                b
            FROM u
            """);
    }

    [Fact]
    public void Formats_except_and_intersect_corresponding()
    {
        AssertFormatted(
            "select a from t except corresponding select a from u intersect corresponding select a from v",
            """
            SELECT
                a
            FROM t
            EXCEPT CORRESPONDING
            SELECT
                a
            FROM u
            INTERSECT CORRESPONDING
            SELECT
                a
            FROM v
            """);
    }

    [Fact]
    public void Preserves_comment_after_corresponding()
    {
        AssertFormatted(
            "select a from t union corresponding /* c */ by (a) select a from u",
            """
            SELECT
                a
            FROM t
            UNION CORRESPONDING /* c */ BY (a)
            SELECT
                a
            FROM u
            """);
    }

    [Fact]
    public void Formats_union()
    {
        AssertFormatted(
            "select a from t union select b from u",
            """
            SELECT
                a
            FROM t
            UNION
            SELECT
                b
            FROM u
            """);
    }

    [Fact]
    public void Formats_with_cte()
    {
        AssertFormatted(
            "with x as (select a from t) select a from x",
            """
            WITH
                x AS (
                    SELECT
                        a
                    FROM t
                )
            SELECT
                a
            FROM x
            """);
    }

    [Fact]
    public void Formats_multiple_ctes_with_column_list()
    {
        AssertFormatted(
            "with x as (select a from t), y(a, b) as (select 1, 2 from u) select a from y",
            """
            WITH
                x AS (
                    SELECT
                        a
                    FROM t
                ),
                y(a, b) AS (
                    SELECT
                        1,
                        2
                    FROM u
                )
            SELECT
                a
            FROM y
            """);
    }

    [Fact]
    public void Formats_recursive_cte_with_union_body()
    {
        AssertFormatted(
            "with recursive x as (select a from t union select a from x) select a from x union select a from y",
            """
            WITH RECURSIVE
                x AS (
                    SELECT
                        a
                    FROM t
                    UNION
                    SELECT
                        a
                    FROM x
                )
            SELECT
                a
            FROM x
            UNION
            SELECT
                a
            FROM y
            """);
    }

    [Fact]
    public void Formats_with_scalar_subquery()
    {
        AssertFormatted(
            "select (with x as (select a from t) select a from x) from u",
            """
            SELECT
                (
                    WITH
                        x AS (
                            SELECT
                                a
                            FROM t
                        )
                    SELECT
                        a
                    FROM x
                )
            FROM u
            """);
    }

    [Fact]
    public void Formats_with_in_derived_table_and_in_subquery()
    {
        AssertFormatted(
            "select d.a from (with x as (select a from t) select a from x) d where d.a in (with y as (select a from u) select a from y)",
            """
            SELECT
                d.a
            FROM (
                WITH
                    x AS (
                        SELECT
                            a
                        FROM t
                    )
                SELECT
                    a
                FROM x
            ) AS d
            WHERE d.a IN (
                WITH
                    y AS (
                        SELECT
                            a
                        FROM u
                    )
                SELECT
                    a
                FROM y
            )
            """);
    }

    [Fact]
    public void Formats_parenthesized_union_operand()
    {
        AssertFormatted(
            "select a from t union (select b from u)",
            """
            SELECT
                a
            FROM t
            UNION
            (
                SELECT
                    b
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_union_in_derived_table()
    {
        AssertFormatted(
            "select x.a from (select a from t union select a from u) x",
            """
            SELECT
                x.a
            FROM (
                SELECT
                    a
                FROM t
                UNION
                SELECT
                    a
                FROM u
            ) AS x
            """);
    }

    [Fact]
    public void Formats_union_order_by_limit()
    {
        AssertFormatted(
            "select a from t union select b from u order by a desc limit 10",
            """
            SELECT
                a
            FROM t
            UNION
            SELECT
                b
            FROM u
            ORDER BY a DESC
            LIMIT 10
            """);
    }

    [Fact]
    public void Formats_intersect_and_except()
    {
        AssertFormatted(
            "select a from t intersect select b from u except select c from v",
            """
            SELECT
                a
            FROM t
            INTERSECT
            SELECT
                b
            FROM u
            EXCEPT
            SELECT
                c
            FROM v
            """);
    }

    [Fact]
    public void Formats_union_all()
    {
        AssertFormatted(
            "select a from t union all select b from u",
            """
            SELECT
                a
            FROM t
            UNION ALL
            SELECT
                b
            FROM u
            """);
    }

    [Fact]
    public void Formats_quantified_subqueries()
    {
        AssertFormatted(
            "select a from t where n>all (select n from lim) and n=any (select n from ok) and n<some (select n from cap)",
            """
            SELECT
                a
            FROM t
            WHERE n > ALL (
                SELECT
                    n
                FROM lim
            )
                AND n = ANY (
                    SELECT
                        n
                    FROM ok
                )
                AND n < SOME (
                    SELECT
                        n
                    FROM cap
                )
            """);
    }

    [Fact]
    public void Formats_exists_and_not_exists()
    {
        AssertFormatted(
            "select a from t where exists (select 1 from u where u.id=t.id) and not exists (select 1 from v where v.id=t.id)",
            """
            SELECT
                a
            FROM t
            WHERE EXISTS (
                SELECT
                    1
                FROM u
                WHERE u.id = t.id
            )
                AND NOT EXISTS (
                    SELECT
                        1
                    FROM v
                    WHERE v.id = t.id
                )
            """);
    }

    [Fact]
    public void Formats_in_subquery()
    {
        AssertFormatted(
            "select a from t where id in (select id from u)",
            """
            SELECT
                a
            FROM t
            WHERE id IN (
                SELECT
                    id
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_not_in_list_and_subquery()
    {
        AssertFormatted(
            "select a from t where x not in (1,2) and id not in (select id from u)",
            """
            SELECT
                a
            FROM t
            WHERE x NOT IN (1, 2)
                AND id NOT IN (
                    SELECT
                        id
                    FROM u
                )
            """);
    }

    [Fact]
    public void Formats_scalar_subquery()
    {
        AssertFormatted(
            "select (select a from t) from u",
            """
            SELECT
                (
                    SELECT
                        a
                    FROM t
                )
            FROM u
            """);
    }

    [Fact]
    public void Rejects_derived_table_without_alias()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("select a from (select a from t)"));
        Assert.Equal("Expected alias after derived table", error.Message);
    }

    [Fact]
    public void Formats_between_in_is_null_and_parens()
    {
        AssertFormatted(
            "select a from t where x between 1 and 2 and y in ('a','b') and (z is null or z>=3)",
            """
            SELECT
                a
            FROM t
            WHERE x BETWEEN 1 AND 2
                AND y IN ('a', 'b')
                AND (z IS NULL OR z >= 3)
            """);
    }



    [Fact]
    public void Formats_match()
    {
        AssertFormatted(
            "select a from t where (a, b) match (select x, y from u)",
            """
            SELECT
                a
            FROM t
            WHERE (a, b) MATCH (
                SELECT
                    x,
                    y
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_match_unique()
    {
        AssertFormatted(
            "select a from t where (a, b) match unique (select x, y from u)",
            """
            SELECT
                a
            FROM t
            WHERE (a, b) MATCH UNIQUE (
                SELECT
                    x,
                    y
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_match_full()
    {
        AssertFormatted(
            "select a from t where (a, b) match full (select x, y from u)",
            """
            SELECT
                a
            FROM t
            WHERE (a, b) MATCH FULL (
                SELECT
                    x,
                    y
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_match_unique_partial()
    {
        AssertFormatted(
            "select a from t where (a, b) match unique partial (select x, y from u)",
            """
            SELECT
                a
            FROM t
            WHERE (a, b) MATCH UNIQUE PARTIAL (
                SELECT
                    x,
                    y
                FROM u
            )
            """);
    }

    [Fact]
    public void Preserves_comment_after_match()
    {
        AssertFormatted(
            "select a from t where (a, b) match /* m */ (select x, y from u)",
            """
            SELECT
                a
            FROM t
            WHERE (a, b) MATCH /* m */ (
                SELECT
                    x,
                    y
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_unique()
    {
        AssertFormatted(
            "select a from t where unique (select b from u)",
            """
            SELECT
                a
            FROM t
            WHERE UNIQUE (
                SELECT
                    b
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_not_unique()
    {
        AssertFormatted(
            "select a from t where not unique (select b from u)",
            """
            SELECT
                a
            FROM t
            WHERE NOT UNIQUE (
                SELECT
                    b
                FROM u
            )
            """);
    }

    [Fact]
    public void Preserves_comment_after_unique()
    {
        AssertFormatted(
            "select a from t where unique /* q */ (select b from u)",
            """
            SELECT
                a
            FROM t
            WHERE UNIQUE /* q */ (
                SELECT
                    b
                FROM u
            )
            """);
    }

    [Fact]
    public void Formats_overlaps()
    {
        AssertFormatted(
            "select a from t where (x, y) overlaps (p, q)",
            """
            SELECT
                a
            FROM t
            WHERE (x, y) OVERLAPS (p, q)
            """);
    }

    [Fact]
    public void Formats_parenthesized_expression()
    {
        AssertFormatted(
            "select (a) from t",
            """
            SELECT
                (a)
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_after_overlaps()
    {
        AssertFormatted(
            "select a from t where (x, y) overlaps /* r */ (p, q)",
            """
            SELECT
                a
            FROM t
            WHERE (x, y) OVERLAPS /* r */ (p, q)
            """);
    }

    [Fact]
    public void Formats_collate()
    {
        AssertFormatted(
            "select x collate latin1 from t",
            """
            SELECT
                x COLLATE latin1
            FROM t
            """);
    }

    [Fact]
    public void Formats_order_by_collate_desc()
    {
        AssertFormatted(
            "select x from t order by name collate posix desc",
            """
            SELECT
                x
            FROM t
            ORDER BY name COLLATE posix DESC
            """);
    }

    [Fact]
    public void Preserves_comment_after_collate()
    {
        AssertFormatted(
            "select x collate /* c */ latin1 from t",
            """
            SELECT
                x COLLATE /* c */ latin1
            FROM t
            """);
    }

    [Fact]
    public void Formats_is_true()
    {
        AssertFormatted(
            "select a from t where x is true",
            """
            SELECT
                a
            FROM t
            WHERE x IS TRUE
            """);
    }

    [Fact]
    public void Formats_is_not_false()
    {
        AssertFormatted(
            "select a from t where x is not false",
            """
            SELECT
                a
            FROM t
            WHERE x IS NOT FALSE
            """);
    }

    [Fact]
    public void Formats_is_unknown()
    {
        AssertFormatted(
            "select a from t where x is unknown",
            """
            SELECT
                a
            FROM t
            WHERE x IS UNKNOWN
            """);
    }

    [Fact]
    public void Formats_not_between()
    {
        AssertFormatted(
            "select a from t where x not between 1 and 2",
            """
            SELECT
                a
            FROM t
            WHERE x NOT BETWEEN 1 AND 2
            """);
    }

    [Fact]
    public void Formats_not_between_with_and_or()
    {
        AssertFormatted(
            "select a from t where x not between 1 and 2 or y between 3 and 4",
            """
            SELECT
                a
            FROM t
            WHERE x NOT BETWEEN 1 AND 2 OR y BETWEEN 3 AND 4
            """);
    }

    [Fact]
    public void Preserves_comment_between_not_and_between()
    {
        AssertFormatted(
            "select a from t where x not /* r */ between 1 and 2",
            """
            SELECT
                a
            FROM t
            WHERE x NOT /* r */ BETWEEN 1 AND 2
            """);
    }

    [Fact]
    public void Formats_values()
    {
        AssertFormatted(
            "values (1, 2)",
            """
            VALUES
                (1, 2)
            """);
    }

    [Fact]
    public void Formats_values_rows()
    {
        AssertFormatted(
            "values (1, 2), (3, 4)",
            """
            VALUES
                (1, 2),
                (3, 4)
            """);
    }

    [Fact]
    public void Formats_values_as_derived_table()
    {
        AssertFormatted(
            "select a from (values (1)) as t",
            """
            SELECT
                a
            FROM (
                VALUES
                    (1)
            ) AS t
            """);
    }

    [Fact]
    public void Formats_insert_select()
    {
        AssertFormatted(
            "insert into t select 1",
            """
            INSERT INTO t
            SELECT
                1
            """);
    }

    [Fact]
    public void Formats_insert_select_column_list()
    {
        AssertFormatted(
            "insert into t (a, b) select 1, 2",
            """
            INSERT INTO t (
                a,
                b
            )
            SELECT
                1,
                2
            """);
    }

    [Fact]
    public void Formats_insert_select_with_at_parameter()
    {
        AssertFormatted(
            """
            INSERT INTO activity_feed (
                    audit_event_id,
                    occurred_on,
                    actor_kind,
                    actor_user_id,
                    actor_auth0_id,
                    actor_display_name,
                    subject_type,
                    subject_record_id,
                    subject_label,
                    parent_subject_type,
                    parent_subject_record_id,
                    action
                )
                 SELECT record_id,
                        occurred_on,
                        actor_kind,
                        actor_user_id,
                        actor_auth0_id,
                        actor_display_name,
                        subject_type,
                        subject_record_id,
                        subject_label,
                        parent_subject_type,
                        parent_subject_record_id,
                        action
                   FROM audit_event
                  WHERE record_id = @auditEventId;
            """,
            """
            INSERT INTO activity_feed (
                audit_event_id,
                occurred_on,
                actor_kind,
                actor_user_id,
                actor_auth0_id,
                actor_display_name,
                subject_type,
                subject_record_id,
                subject_label,
                parent_subject_type,
                parent_subject_record_id,
                action
            )
            SELECT
                record_id,
                occurred_on,
                actor_kind,
                actor_user_id,
                actor_auth0_id,
                actor_display_name,
                subject_type,
                subject_record_id,
                subject_label,
                parent_subject_type,
                parent_subject_record_id,
                action
            FROM audit_event
            WHERE record_id = @auditEventId
            """);
    }

    [Fact]
    public void Formats_colon_host_parameter()
    {
        AssertFormatted(
            "select a from t where x = :id",
            """
            SELECT
                a
            FROM t
            WHERE x = :id
            """);
    }

    [Fact]
    public void Preserves_at_parameter_casing()
    {
        AssertFormatted(
            "select x = @AuditEventId from t",
            """
            SELECT
                x = @AuditEventId
            FROM t
            """);
    }

    [Fact]
    public void Formats_question_host_parameter()
    {
        AssertFormatted(
            "select a from t where x = ?",
            """
            SELECT
                a
            FROM t
            WHERE x = ?
            """);
    }

    [Fact]
    public void Formats_insert_values()
    {
        AssertFormatted(
            "insert into t values (1), (2)",
            """
            INSERT INTO t
            VALUES
                (1),
                (2)
            """);
    }

    [Fact]
    public void Formats_insert_qualified_table()
    {
        AssertFormatted(
            "insert into dbo.t (a) select 1",
            """
            INSERT INTO dbo.t (
                a
            )
            SELECT
                1
            """);
    }

    [Fact]
    public void Preserves_comments_in_insert()
    {
        AssertFormatted(
            """
            insert -- i
            into /* tbl */ t /* cols */ (a) select 1
            """,
            """
            INSERT -- i
            INTO /* tbl */ t /* cols */ (
                a
            )
            SELECT
                1
            """);
    }

    [Fact]
    public void Formats_comma_from()
    {
        AssertFormatted(
            "select a from t, u",
            """
            SELECT
                a
            FROM t, u
            """);
    }

    [Fact]
    public void Formats_like_escape()
    {
        AssertFormatted(
            "select a from t where x like 'a!%' escape '!'",
            """
            SELECT
                a
            FROM t
            WHERE x LIKE 'a!%' ESCAPE '!'
            """);
    }

    [Fact]
    public void Formats_select_all()
    {
        AssertFormatted(
            "select all a from t",
            """
            SELECT ALL
                a
            FROM t
            """);
    }

    [Fact]
    public void Formats_true()
    {
        AssertFormatted(
            "select true from t",
            """
            SELECT
                TRUE
            FROM t
            """);
    }

    [Fact]
    public void Formats_false_alias()
    {
        AssertFormatted(
            "select false as flag from t",
            """
            SELECT
                FALSE AS flag
            FROM t
            """);
    }

    [Fact]
    public void Formats_true_in_where()
    {
        AssertFormatted(
            "select a from t where x = true",
            """
            SELECT
                a
            FROM t
            WHERE x = TRUE
            """);
    }

    [Fact]
    public void Formats_filter_where()
    {
        AssertFormatted(
            "select sum(x) filter (where y = 1) from t",
            """
            SELECT
                sum(x) FILTER (WHERE y = 1)
            FROM t
            """);
    }

    [Fact]
    public void Formats_count_star_filter()
    {
        AssertFormatted(
            "select count(*) filter (where z) from t",
            """
            SELECT
                count(*) FILTER (WHERE z)
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_after_filter()
    {
        AssertFormatted(
            "select sum(x) filter /* f */ (where y = 1) from t",
            """
            SELECT
                sum(x) FILTER /* f */ (WHERE y = 1)
            FROM t
            """);
    }

    [Fact]
    public void Formats_json_arrow()
    {
        AssertFormatted(
            "select metadata->'a' from t",
            """
            SELECT
                metadata->'a'
            FROM t
            """);
    }

    [Fact]
    public void Formats_json_text_arrow()
    {
        AssertFormatted(
            "select metadata->>'b' from t",
            """
            SELECT
                metadata->>'b'
            FROM t
            """);
    }

    [Fact]
    public void Formats_json_arrow_chain()
    {
        AssertFormatted(
            "select metadata->'a'->>'b' from t",
            """
            SELECT
                metadata->'a'->>'b'
            FROM t
            """);
    }

    [Fact]
    public void Formats_qualified_json_arrow_chain()
    {
        AssertFormatted(
            "select l.metadata->'a'->>'b' from t",
            """
            SELECT
                l.metadata->'a'->>'b'
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_after_json_arrow()
    {
        AssertFormatted(
            "select metadata-> /* k */ 'a' from t",
            """
            SELECT
                metadata->/* k */ 'a'
            FROM t
            """);
    }

    [Fact]
    public void Formats_array()
    {
        AssertFormatted(
            "select array[1, 2] from t",
            """
            SELECT
                ARRAY[1, 2]
            FROM t
            """);
    }

    [Fact]
    public void Formats_empty_array()
    {
        AssertFormatted(
            "select array[] from t",
            """
            SELECT
                ARRAY[]
            FROM t
            """);
    }

    [Fact]
    public void Formats_array_with_colon_cast()
    {
        AssertFormatted(
            "select array[id::text] from t",
            """
            SELECT
                ARRAY[id::TEXT]
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_inside_array()
    {
        AssertFormatted(
            "select array[ /* n */ 1] from t",
            """
            SELECT
                ARRAY[/* n */ 1]
            FROM t
            """);
    }

    [Fact]
    public void Formats_concat()
    {
        AssertFormatted(
            "select a || b from t",
            """
            SELECT
                a || b
            FROM t
            """);
    }

    [Fact]
    public void Formats_concat_chain()
    {
        AssertFormatted(
            "select p.first_name || ' ' || p.last_name from t",
            """
            SELECT
                p.first_name || ' ' || p.last_name
            FROM t
            """);
    }

    [Fact]
    public void Formats_cast()
    {
        AssertFormatted(
            "select cast(x as numeric) from t",
            """
            SELECT
                CAST(x AS NUMERIC)
            FROM t
            """);
    }

    [Fact]
    public void Formats_cast_in_where()
    {
        AssertFormatted(
            "select a from t where cast(x as int) = 1",
            """
            SELECT
                a
            FROM t
            WHERE CAST(x AS INT) = 1
            """);
    }

    [Fact]
    public void Preserves_comment_between_as_and_type()
    {
        AssertFormatted(
            "select cast(x as /* t */ numeric) from t",
            """
            SELECT
                CAST(x AS /* t */ NUMERIC)
            FROM t
            """);
    }

    [Fact]
    public void Formats_colon_cast()
    {
        AssertFormatted(
            "select x::numeric from t",
            """
            SELECT
                x::NUMERIC
            FROM t
            """);
    }

    [Fact]
    public void Formats_cast_with_precision_and_scale()
    {
        AssertFormatted(
            "select cast(x as numeric(10, 2)), y::numeric(10,2) from t",
            """
            SELECT
                CAST(x AS NUMERIC(10, 2)),
                y::NUMERIC(10, 2)
            FROM t
            """);
    }

    [Fact]
    public void Formats_like()
    {
        AssertFormatted(
            "select a from t where x like 'a%'",
            """
            SELECT
                a
            FROM t
            WHERE x LIKE 'a%'
            """);
    }

    [Fact]
    public void Formats_not_like()
    {
        AssertFormatted(
            "select a from t where x not like 'a%'",
            """
            SELECT
                a
            FROM t
            WHERE x NOT LIKE 'a%'
            """);
    }

    [Fact]
    public void Formats_like_with_and_or()
    {
        AssertFormatted(
            "select a from t where x like 'a%' or y not like '%z' and z like '_'",
            """
            SELECT
                a
            FROM t
            WHERE x LIKE 'a%' OR y NOT LIKE '%z' AND z LIKE '_'
            """);
    }

    [Fact]
    public void Preserves_comment_between_like_and_pattern()
    {
        AssertFormatted(
            "select a from t where x like /* pat */ 'a%'",
            """
            SELECT
                a
            FROM t
            WHERE x LIKE /* pat */ 'a%'
            """);
    }

    [Fact]
    public void Formats_select_without_from()
    {
        AssertFormatted(
            "select 1",
            """
            SELECT
                1
            """);
    }

    [Fact]
    public void Formats_select_distinct_without_from()
    {
        AssertFormatted(
            "select distinct a, 2",
            """
            SELECT DISTINCT
                a,
                2
            """);
    }

    [Fact]
    public void Formats_select_without_from_with_where()
    {
        AssertFormatted(
            "select 1 where x = 1",
            """
            SELECT
                1
            WHERE x = 1
            """);
    }

    [Fact]
    public void Formats_scalar_subquery_without_from()
    {
        AssertFormatted(
            "select (select 1)",
            """
            SELECT
                (
                    SELECT
                        1
                )
            """);
    }

    [Fact]
    public void Formats_select_distinct()
    {
        AssertFormatted(
            "select distinct a from t",
            """
            SELECT DISTINCT
                a
            FROM t
            """);
    }

    [Fact]
    public void Formats_select_distinct_columns_and_where()
    {
        AssertFormatted(
            "select distinct a, b from t where x=1",
            """
            SELECT DISTINCT
                a,
                b
            FROM t
            WHERE x = 1
            """);
    }

    [Fact]
    public void Formats_distinct_in_subquery_and_cte()
    {
        AssertFormatted(
            "with c as (select distinct a from t) select distinct b from (select distinct b from c) as s",
            """
            WITH
                c AS (
                    SELECT DISTINCT
                        a
                    FROM t
                )
            SELECT DISTINCT
                b
            FROM (
                SELECT DISTINCT
                    b
                FROM c
            ) AS s
            """);
    }

    [Fact]
    public void Preserves_comment_between_select_and_distinct()
    {
        AssertFormatted(
            "select /* keep */ distinct a from t",
            """
            SELECT /* keep */ DISTINCT
                a
            FROM t
            """);
    }

    [Fact]
    public void Formats_quoted_identifiers()
    {
        AssertFormatted(
            """select "First Name", "select" as "from" from "Person" as "p" where "p"."Id" = 1""",
            """
            SELECT
                "First Name",
                "select" AS "from"
            FROM "Person" AS "p"
            WHERE "p"."Id" = 1
            """);
    }

    [Fact]
    public void Formats_quoted_reserved_words_as_identifiers()
    {
        AssertFormatted(
            """
            select "date", "trim", "extract" from "interval"
            """,
            """
            SELECT
                "date",
                "trim",
                "extract"
            FROM "interval"
            """);
    }

    [Fact]
    public void Formats_quoted_identifier_with_escaped_quote()
    {
        AssertFormatted(
            """select "a""b" from t""",
            """
            SELECT
                "a""b"
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_before_quoted_identifier()
    {
        AssertFormatted(
            """
            select a from /* c */ "Person"
            """,
            """
            SELECT
                a
            FROM /* c */ "Person"
            """);
    }

    [Fact]
    public void Rejects_unterminated_quoted_identifier()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("""select "A from t"""));
        Assert.Equal("Unterminated quoted identifier", error.Message);
    }

    [Fact]
    public void Rejects_empty_quoted_identifier()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("""select "" from t"""));
        Assert.Equal("Empty quoted identifier", error.Message);
    }

    [Fact]
    public void Formats_date_time_timestamp_literals()
    {
        AssertFormatted(
            "select date '1999-01-01', time '12:00:00', timestamp '1999-01-01 12:00:00'",
            """
            SELECT
                DATE '1999-01-01',
                TIME '12:00:00',
                TIMESTAMP '1999-01-01 12:00:00'
            """);
    }

    [Fact]
    public void Formats_datetime_literal_in_where()
    {
        AssertFormatted(
            "select a from t where x = date '1999-01-01'",
            """
            SELECT
                a
            FROM t
            WHERE x = DATE '1999-01-01'
            """);
    }

    [Fact]
    public void Preserves_comment_between_date_and_string()
    {
        AssertFormatted(
            "select date /* d */ '1999-01-01'",
            """
            SELECT
                DATE /* d */ '1999-01-01'
            """);
    }

    [Fact]
    public void Formats_cast_to_datetime_types()
    {
        AssertFormatted(
            "select cast(x as date), y::time, z::timestamp, w::interval from t",
            """
            SELECT
                CAST(x AS DATE),
                y::TIME,
                z::TIMESTAMP,
                w::INTERVAL
            FROM t
            """);
    }

    [Fact]
    public void Formats_interval_literals()
    {
        AssertFormatted(
            "select interval '1' day, interval -'1' year to month, interval '1' second(3)",
            """
            SELECT
                INTERVAL '1' DAY,
                INTERVAL -'1' YEAR TO MONTH,
                INTERVAL '1' SECOND(3)
            """);
    }

    [Fact]
    public void Formats_interval_with_precision_and_end_field()
    {
        AssertFormatted(
            "select interval +'1' day(2) to second(6) from t",
            """
            SELECT
                INTERVAL +'1' DAY(2) TO SECOND(6)
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_in_interval_literal()
    {
        AssertFormatted(
            "select interval /* i */ '1' /* u */ day",
            """
            SELECT
                INTERVAL /* i */ '1' /* u */ DAY
            """);
    }

    [Fact]
    public void Formats_trim()
    {
        AssertFormatted(
            "select trim(s), trim(both from s), trim(leading 'x' from s), trim('x' from s) from t",
            """
            SELECT
                TRIM(s),
                TRIM(BOTH FROM s),
                TRIM(LEADING 'x' FROM s),
                TRIM('x' FROM s)
            FROM t
            """);
    }

    [Fact]
    public void Formats_extract()
    {
        AssertFormatted(
            "select extract(year from d), extract(timezone_hour from date '1999-01-01') from t",
            """
            SELECT
                EXTRACT(YEAR FROM d),
                EXTRACT(TIMEZONE_HOUR FROM DATE '1999-01-01')
            FROM t
            """);
    }

    [Fact]
    public void Formats_substring()
    {
        AssertFormatted(
            "select substring(s from 1), substring(s from 1 for 2) from t",
            """
            SELECT
                SUBSTRING(s FROM 1),
                SUBSTRING(s FROM 1 FOR 2)
            FROM t
            """);
    }

    [Fact]
    public void Formats_position()
    {
        AssertFormatted(
            "select position('a' in s) from t",
            """
            SELECT
                POSITION('a' IN s)
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comments_in_special_forms()
    {
        AssertFormatted(
            "select trim(/* t */ both /* c */ 'x' /* f */ from s), extract(/* e */ year /* f */ from d), substring(s /* f */ from 1 /* n */ for 2), position('a' /* i */ in s) from t",
            """
            SELECT
                TRIM(/* t */ BOTH /* c */ 'x' /* f */ FROM s),
                EXTRACT(/* e */ YEAR /* f */ FROM d),
                SUBSTRING(s /* f */ FROM 1 /* n */ FOR 2),
                POSITION('a' /* i */ IN s)
            FROM t
            """);
    }

    [Fact]
    public void Formats_null_literal()
    {
        AssertFormatted(
            "select null, coalesce(null, 1) from t",
            """
            SELECT
                NULL,
                coalesce(NULL, 1)
            FROM t
            """);
    }

    [Fact]
    public void Formats_null_in_where_and_is_null()
    {
        AssertFormatted(
            "select a from t where x = null and y is null",
            """
            SELECT
                a
            FROM t
            WHERE x = NULL
                AND y IS NULL
            """);
    }

    [Fact]
    public void Formats_niladic_functions()
    {
        AssertFormatted(
            "select current_date, current_time, current_time(3), current_timestamp, user, current_user, session_user, system_user",
            """
            SELECT
                CURRENT_DATE,
                CURRENT_TIME,
                CURRENT_TIME(3),
                CURRENT_TIMESTAMP,
                USER,
                CURRENT_USER,
                SESSION_USER,
                SYSTEM_USER
            """);
    }

    [Fact]
    public void Formats_current_timestamp_with_precision()
    {
        AssertFormatted(
            "select current_timestamp(6) from t",
            """
            SELECT
                CURRENT_TIMESTAMP(6)
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comment_in_current_time_precision()
    {
        AssertFormatted(
            "select current_time /* p */ (3)",
            """
            SELECT
                CURRENT_TIME /* p */ (3)
            """);
    }

    [Fact]
    public void Formats_multi_word_cast_types()
    {
        AssertFormatted(
            "select cast(x as double precision), cast(y as character varying(10)), cast(z as char varying), cast(a as time with time zone), b::timestamp(6) with time zone from t",
            """
            SELECT
                CAST(x AS DOUBLE PRECISION),
                CAST(y AS CHARACTER VARYING(10)),
                CAST(z AS CHAR VARYING),
                CAST(a AS TIME WITH TIME ZONE),
                b::TIMESTAMP(6) WITH TIME ZONE
            FROM t
            """);
    }

    [Fact]
    public void Formats_time_precision_with_time_zone()
    {
        AssertFormatted(
            "select cast(x as time(6) with time zone) from t",
            """
            SELECT
                CAST(x AS TIME(6) WITH TIME ZONE)
            FROM t
            """);
    }

    [Fact]
    public void Preserves_comments_in_multi_word_types()
    {
        AssertFormatted(
            "select cast(x as double /* p */ precision), y::time /* w */ with /* t */ time /* z */ zone from t",
            """
            SELECT
                CAST(x AS DOUBLE /* p */ PRECISION),
                y::TIME /* w */ WITH /* t */ TIME /* z */ ZONE
            FROM t
            """);
    }

    [Fact]
    public void Rejects_parens_on_current_date()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("select current_date()"));
        Assert.Equal("CURRENT_DATE does not take parentheses", error.Message);
    }

    [Fact]
    public void Preserves_comment_before_null()
    {
        AssertFormatted(
            "select /* n */ null from t",
            """
            SELECT
                /* n */
                NULL
            FROM t
            """);
    }

    [Fact]
    public void Formats_parenthesized_join()
    {
        AssertFormatted(
            "select a from (t join u on t.id=u.id)",
            """
            SELECT
                a
            FROM (
                t
                JOIN u
                    ON t.id = u.id
            )
            """);
    }

    [Fact]
    public void Formats_parenthesized_join_as_join_operand()
    {
        AssertFormatted(
            "select a from t join (u join v on u.id=v.id) on t.id=u.id",
            """
            SELECT
                a
            FROM t
            JOIN (
                u
                JOIN v
                    ON u.id = v.id
            )
                ON t.id = u.id
            """);
    }

    [Fact]
    public void Formats_nested_parenthesized_joins()
    {
        AssertFormatted(
            "select a from ((t join u on t.id=u.id) join v on t.id=v.id)",
            """
            SELECT
                a
            FROM (
                (
                    t
                    JOIN u
                        ON t.id = u.id
                )
                JOIN v
                    ON t.id = v.id
            )
            """);
    }

    [Fact]
    public void Formats_parenthesized_join_then_outer_join()
    {
        AssertFormatted(
            "select a from (t join u on t.id=u.id) join v on t.id=v.id",
            """
            SELECT
                a
            FROM (
                t
                JOIN u
                    ON t.id = u.id
            )
            JOIN v
                ON t.id = v.id
            """);
    }

    [Fact]
    public void Preserves_comment_in_parenthesized_join()
    {
        AssertFormatted(
            "select a from (t join /* u */ u on t.id=u.id)",
            """
            SELECT
                a
            FROM (
                t
                JOIN /* u */ u
                    ON t.id = u.id
            )
            """);
    }

    [Fact]
    public void Formats_table_correlation_columns()
    {
        AssertFormatted(
            "select a from t as x(a, b)",
            """
            SELECT
                a
            FROM t AS x(a, b)
            """);
    }

    [Fact]
    public void Formats_bare_alias_correlation_columns()
    {
        AssertFormatted(
            "select a from t x(a, b)",
            """
            SELECT
                a
            FROM t AS x(a, b)
            """);
    }

    [Fact]
    public void Formats_derived_table_correlation_columns()
    {
        AssertFormatted(
            "select a from (select a from u) as x(a, b)",
            """
            SELECT
                a
            FROM (
                SELECT
                    a
                FROM u
            ) AS x(a, b)
            """);
    }

    [Fact]
    public void Preserves_comment_before_correlation_columns()
    {
        AssertFormatted(
            "select a from t as x /* c */ (a, b)",
            """
            SELECT
                a
            FROM t AS x /* c */ (a, b)
            """);
    }

    [Fact]
    public void Formats_national_bit_and_hex_strings()
    {
        AssertFormatted(
            "select N'café', B'1010', X'FF' from t",
            """
            SELECT
                N'café',
                B'1010',
                X'FF'
            FROM t
            """);
    }

    [Fact]
    public void Formats_prefixed_strings_case_preserving()
    {
        AssertFormatted(
            "select n'foo', b'01', x'ab'",
            """
            SELECT
                n'foo',
                b'01',
                x'ab'
            """);
    }

    [Fact]
    public void Formats_empty_national_string()
    {
        AssertFormatted(
            "select N'' from t",
            """
            SELECT
                N''
            FROM t
            """);
    }

    [Fact]
    public void Rejects_unterminated_national_string()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("select N'foo"));
        Assert.Equal("Unterminated string", error.Message);
    }

    [Fact]
    public void Rejects_parenthesized_table_without_join()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("select a from (t)"));
        Assert.Equal("Expected JOIN in parenthesized table", error.Message);
    }

    [Fact]
    public void Formats_convert_and_translate()
    {
        AssertFormatted(
            "select convert(a using utf8), translate(b using t) from t",
            """
            SELECT
                CONVERT(a USING utf8),
                TRANSLATE(b USING t)
            FROM t
            """);
    }

    [Fact]
    public void Formats_for_update()
    {
        AssertFormatted(
            "select a from t for update",
            """
            SELECT
                a
            FROM t
            FOR UPDATE
            """);
    }

    [Fact]
    public void Formats_for_read_only()
    {
        AssertFormatted(
            "select a from t for read only",
            """
            SELECT
                a
            FROM t
            FOR READ ONLY
            """);
    }

    [Fact]
    public void Formats_for_update_of_columns()
    {
        AssertFormatted(
            "select a from t for update of x, y",
            """
            SELECT
                a
            FROM t
            FOR UPDATE OF x, y
            """);
    }

    [Fact]
    public void Formats_for_update_after_order_and_limit()
    {
        AssertFormatted(
            "select a from t order by a limit 1 for update",
            """
            SELECT
                a
            FROM t
            ORDER BY a
            LIMIT 1
            FOR UPDATE
            """);
    }

    [Fact]
    public void Preserves_comment_in_for_update()
    {
        AssertFormatted(
            "select a from t for /* u */ update of x",
            """
            SELECT
                a
            FROM t
            FOR /* u */ UPDATE OF x
            """);
    }

    [Fact]
    public void Rejects_bare_for()
    {
        var error = Assert.Throws<QlParse.SqlParseException>(() => Sql.Format("select a from t for"));
        Assert.Equal("Expected READ or UPDATE", error.Message);
    }

    [Fact]
    public void Preserves_comments_in_convert()
    {
        AssertFormatted(
            "select convert(/* e */ a /* u */ using latin1) from t",
            """
            SELECT
                CONVERT(/* e */ a /* u */ USING latin1)
            FROM t
            """);
    }

    [Fact]
    public void Colors_keywords_literals_and_comments()
    {
        var sql = """
            select a -- c
            from t where x=1 and y='z'
            """;
        var plain = Sql.Format(sql);
        var colored = Sql.Format(sql, color: true);

        Assert.DoesNotContain('\u001b', plain);
        Assert.Contains('\u001b', colored);
        Assert.Equal(plain, StripAnsi(colored));
    }

    private static void AssertFormatted(string sql, string expected) =>
        Assert.Equal(expected, Sql.Format(sql));

    private const int AnsiCsiPrefixLength = 2;

    private static string StripAnsi(string text)
    {
        var stripped = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\u001b' && i + 1 < text.Length && text[i + 1] == '[')
            {
                i += AnsiCsiPrefixLength;
                while (i < text.Length && text[i] is not 'm')
                {
                    i++;
                }

                continue;
            }

            stripped.Append(text[i]);
        }

        return stripped.ToString();
    }
}
