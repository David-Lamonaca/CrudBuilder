using Microsoft.Data.SqlClient;

public class SqlServerSchemaReader : IDatabaseSchemaReader
{

    public List<string> GetTables(string connectionString)
    {
        var tables = new List<string>();
        using var conn = new SqlConnection(connectionString);
        conn.Open();

        var cmd = new SqlCommand(@"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
        ", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public List<ColumnInfo> GetColumns(string connectionString, string tableName)
    {
        var columns = new List<ColumnInfo>();
        var primaryKeyCols = new HashSet<string>();

        using var conn = new SqlConnection(connectionString);
        conn.Open();

        // First: get actual PK columns
        using (var pkCmd = new SqlCommand(@"
        SELECT kcu.COLUMN_NAME
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
            ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
        WHERE tc.TABLE_NAME = @Table
            AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
    ", conn))
        {
            pkCmd.Parameters.AddWithValue("@Table", tableName);
            using var pkReader = pkCmd.ExecuteReader();
            while (pkReader.Read())
            {
                primaryKeyCols.Add(pkReader.GetString(0));
            }
        }

        // Then: get all columns and tag PKs properly
        using (var colCmd = new SqlCommand(@"
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = @Table
    ", conn))
        {
            colCmd.Parameters.AddWithValue("@Table", tableName);
            using var reader = colCmd.ExecuteReader();

            while (reader.Read())
            {
                string name = reader.GetString(0);
                string sqlType = reader.GetString(1);
                bool isNullable = reader.GetString(2) == "YES";
                int? maxLength = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                bool isPrimaryKey = primaryKeyCols.Contains(name);

                columns.Add(new ColumnInfo(name, sqlType, isNullable, maxLength, isPrimaryKey));
            }
        }

        return columns;
    }

    public List<ForeignKeyInfo> GetForeignKeys(string connectionString)
    {
        var foreignKeys = new List<ForeignKeyInfo>();

        using var conn = new SqlConnection(connectionString);
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT 
                FK.TABLE_NAME AS FromTable,
                CU.COLUMN_NAME AS FromColumn,
                PK.TABLE_NAME AS ToTable,
                PT.COLUMN_NAME AS ToColumn
            FROM 
                INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
            INNER JOIN 
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK 
                ON RC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
            INNER JOIN 
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK 
                ON RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
            INNER JOIN 
                INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU 
                ON RC.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
            INNER JOIN 
                (
                    SELECT 
                        i1.CONSTRAINT_NAME, i2.COLUMN_NAME
                    FROM 
                        INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                    INNER JOIN 
                        INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 
                        ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                    WHERE 
                        i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) PT 
                ON PT.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME
        ", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            foreignKeys.Add(new ForeignKeyInfo
            {
                FromTable = reader["FromTable"].ToString()!,
                FromColumn = reader["FromColumn"].ToString()!,
                ToTable = reader["ToTable"].ToString()!,
                ToColumn = reader["ToColumn"].ToString()!
            });
        }

        return foreignKeys;
    }
}
