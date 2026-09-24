namespace QlFmt.Tests;

public sealed class FormatStandardTests
{
    [Fact]
    public void Formats_direct_sql_script()
    {
        AssertFormatted(
            "select a from t; select b from u",
            """
            SELECT
                a
            FROM t;
            SELECT
                b
            FROM u
            """);
    }

    [Fact]
    public void Formats_direct_sql_script_with_trailing_semicolon()
    {
        AssertFormatted(
            "select a from t; insert into t values (1);",
            """
            SELECT
                a
            FROM t;
            INSERT INTO t
            VALUES
                (1);
            """);
    }

    [Fact]
    public void Still_omits_trailing_semicolon_on_one_statement()
    {
        AssertFormatted(
            "select a from t;",
            """
            SELECT
                a
            FROM t
            """);
    }

    [Fact]
    public void Formats_unicode_delimited_identifiers()
    {
        AssertFormatted(
            """select U&"foo", u&"a""b" from t""",
            """
            SELECT
                U&"foo",
                u&"a""b"
            FROM t
            """);
    }

    [Fact]
    public void Formats_charset_introducer_identifier()
    {
        AssertFormatted(
            """select _latin1"Foo" from t""",
            """
            SELECT
                _latin1"Foo"
            FROM t
            """);
    }

    [Fact]
    public void Formats_charset_introducer_string()
    {
        AssertFormatted(
            "select _utf8'hello' from t",
            """
            SELECT
                _utf8'hello'
            FROM t
            """);
    }

    [Fact]
    public void Formats_unicode_string()
    {
        AssertFormatted(
            @"select U&'\0061' from t",
            """
            SELECT
                U&'\0061'
            FROM t
            """);
    }

    [Fact]
    public void Preserves_nested_block_comments()
    {
        AssertFormatted(
            "select a /* outer /* inner */ still */ from t",
            """
            SELECT
                a /* outer /* inner */ still */
            FROM t
            """);
    }

    [Fact]
    public void Formats_treat()
    {
        AssertFormatted(
            "select treat(x as int), treat(x as public.my_udt) from t",
            """
            SELECT
                TREAT(x AS INT),
                TREAT(x AS PUBLIC.MY_UDT)
            FROM t
            """);
    }

    [Fact]
    public void Formats_next_value_for()
    {
        AssertFormatted(
            "select next value for seq, next value for public.seq from t",
            """
            SELECT
                NEXT VALUE FOR seq,
                NEXT VALUE FOR public.seq
            FROM t
            """);
    }

    [Fact]
    public void Formats_row_ref_and_array_types()
    {
        AssertFormatted(
            "select cast(x as row(a int, b varchar(10))), cast(y as integer array[3]), cast(z as ref(foo) scope t) from t",
            """
            SELECT
                CAST(x AS ROW(a INT, b VARCHAR(10))),
                CAST(y AS INTEGER ARRAY[3]),
                CAST(z AS REF(FOO) SCOPE t)
            FROM t
            """);
    }

    [Fact]
    public void Formats_multiset_and_mdarray_types()
    {
        AssertFormatted(
            "select cast(x as integer multiset), cast(y as integer mdarray[0:4, 0:9]) from t",
            """
            SELECT
                CAST(x AS INTEGER MULTISET),
                CAST(y AS INTEGER MDARRAY[0:4, 0:9])
            FROM t
            """);
    }

    [Fact]
    public void Formats_xml_type_modifier()
    {
        AssertFormatted(
            "select cast(x as xml(document)) from t",
            """
            SELECT
                CAST(x AS XML(DOCUMENT))
            FROM t
            """);
    }

    [Fact]
    public void Formats_special_forms()
    {
        AssertFormatted(
            "select upper(x), char_length(x using characters), power(x, 2), width_bucket(x, 0, 1), cardinality(array[1]) from t",
            """
            SELECT
                UPPER(x),
                CHAR_LENGTH(x USING CHARACTERS),
                POWER(x, 2),
                WIDTH_BUCKET(x, 0, 1),
                CARDINALITY(ARRAY[1])
            FROM t
            """);
    }

    [Fact]
    public void Formats_overlay()
    {
        AssertFormatted(
            "select overlay(x placing y from 1), overlay(x placing y from 1 for 2) from t",
            """
            SELECT
                OVERLAY(x PLACING y FROM 1),
                OVERLAY(x PLACING y FROM 1 FOR 2)
            FROM t
            """);
    }

    [Fact]
    public void Formats_array_subquery()
    {
        AssertFormatted(
            "select array(select a from t order by a)",
            """
            SELECT
                ARRAY(
                    SELECT
                        a
                    FROM t
                    ORDER BY a
                )
            """);
    }

    [Fact]
    public void Formats_multiset_constructors_and_ops()
    {
        AssertFormatted(
            "select multiset[1, 2], set(multiset[1]), a multiset union all b, table(select 1) from t",
            """
            SELECT
                MULTISET[1, 2],
                SET(MULTISET[1]),
                a MULTISET UNION ALL b,
                TABLE(
                    SELECT
                        1
                )
            FROM t
            """);
    }

    [Fact]
    public void Formats_absent_on_null()
    {
        AssertFormatted(
            "select absent on null from t",
            """
            SELECT
                ABSENT ON NULL
            FROM t
            """);
    }

    [Fact]
    public void Formats_deref_ref_and_specifictype()
    {
        AssertFormatted(
            "select deref(p.manager), ref(p), specifictype(p) from t",
            """
            SELECT
                DEREF(p.manager),
                REF(p),
                SPECIFICTYPE(p)
            FROM t
            """);
    }

    [Fact]
    public void Formats_dereference_and_methods()
    {
        AssertFormatted(
            "select p.manager -> name, p.label(), (p as sch.person).label(p.first_name), sch.person :: make(p.first_name), new sch.person(p.first_name) from t",
            """
            SELECT
                p.manager->name,
                p.label(),
                (p AS SCH.PERSON).label(p.first_name),
                sch.person::make(p.first_name),
                NEW sch.person(p.first_name)
            FROM t
            """);
    }

    [Fact]
    public void Formats_extra_niladic_functions()
    {
        AssertFormatted(
            "select localtime, localtime(3), localtimestamp, current_role, current_catalog, current_schema, current_path",
            """
            SELECT
                LOCALTIME,
                LOCALTIME(3),
                LOCALTIMESTAMP,
                CURRENT_ROLE,
                CURRENT_CATALOG,
                CURRENT_SCHEMA,
                CURRENT_PATH
            """);
    }

    [Fact]
    public void Formats_coalesce_and_nullif_as_special_forms()
    {
        AssertFormatted(
            "select coalesce(a, b), nullif(a, b) from t",
            """
            SELECT
                coalesce(a, b),
                nullif(a, b)
            FROM t
            """);
    }

    private static void AssertFormatted(string sql, string expected) =>
        Assert.Equal(expected, Sql.Format(sql));
}
