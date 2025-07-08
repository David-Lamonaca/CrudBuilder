public static class SqlTypeMapper
{
    public static string MapToCSharpType(string sqlType, bool isNullable)
    {
        string type = sqlType.ToLower() switch
        {
            // Integer types
            "int" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",

            // Boolean
            "bit" => "bool",

            // Decimal & currency
            "decimal" => "decimal",
            "numeric" => "decimal",
            "money" => "decimal",
            "smallmoney" => "decimal",

            // Floating point
            "float" => "double",
            "real" => "float",

            // Date and time
            "date" => "DateTime",
            "datetime" => "DateTime",
            "datetime2" => "DateTime",
            "smalldatetime" => "DateTime",
            "datetimeoffset" => "DateTimeOffset",
            "time" => "TimeSpan",

            // Character strings
            "char" => "string",
            "varchar" => "string",
            "nchar" => "string",
            "nvarchar" => "string",
            "text" => "string",
            "ntext" => "string",

            // Binary data
            "binary" => "byte[]",
            "varbinary" => "byte[]",
            "image" => "byte[]",
            "rowversion" => "byte[]",
            "timestamp" => "byte[]",

            // Unique identifier
            "uniqueidentifier" => "Guid",

            // XML
            "xml" => "string",

            // SQL Variant
            "sql_variant" => "object",

            // Default fallback
            _ => "string"
        };

        // Only apply nullability to value types (string/byte[] already allow null)
        bool isReferenceType = type == "string" || type == "byte[]" || type == "object";
        return isNullable && !isReferenceType ? type + "?" : type;
    }
}
