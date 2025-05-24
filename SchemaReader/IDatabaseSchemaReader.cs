public interface IDatabaseSchemaReader
{
    List<string> GetTables(string connectionString);
    List<ColumnInfo> GetColumns(string connectionString, string tableName);
    List<ForeignKeyInfo> GetForeignKeys(string connectionString);
}