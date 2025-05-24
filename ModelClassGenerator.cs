using System.Text;

public class ModelClassGenerator
{
    private readonly List<ForeignKeyInfo> _foreignKeys;
    private readonly Dictionary<string, List<ForeignKeyInfo>> _fksFromTable;
    private readonly Dictionary<string, List<ForeignKeyInfo>> _fksToTable;

    public ModelClassGenerator(List<ForeignKeyInfo> foreignKeys)
    {
        _foreignKeys = foreignKeys;

        // Pre-grouped for performance
        _fksFromTable = _foreignKeys.GroupBy(fk => fk.FromTable).ToDictionary(g => g.Key, g => g.ToList());
        _fksToTable = _foreignKeys.GroupBy(fk => fk.ToTable).ToDictionary(g => g.Key, g => g.ToList());
    }

    public string GenerateModelClass(string tableName, List<ColumnInfo> columns)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GenerateClassComment(tableName));
        sb.AppendLine($"public class {tableName}");
        sb.AppendLine("{");

        // Properties for table columns
        foreach (var col in columns)
        {
            var type = MapToCSharpType(col.SqlType, col.IsNullable);

            bool needsNullForgiveness = !col.IsNullable && (type == "string" || type.EndsWith("[]") || type == "object");
            if (needsNullForgiveness)
            {
                sb.AppendLine($"    public {type} {col.Name} {{ get; set; }} = null!;");
            }
            else
            {
                sb.AppendLine($"    public {type} {col.Name} {{ get; set; }}");
            }
        }

        // FK: navigation properties (e.g. public virtual Token? Token { get; set; })
        if (_fksFromTable.TryGetValue(tableName, out var fromFks))
        {
            sb.AppendLine();
            foreach (var fk in fromFks)
            {
                string toClass = NameHumanizer.Singularize(fk.ToTable);
                sb.AppendLine($"    public virtual {toClass}? {toClass} {{ get; set; }}");
            }
        }

        // Reverse FK: collections (e.g. public virtual ICollection<UserToken> UserTokens { get; set; })
        if (_fksToTable.TryGetValue(tableName, out var toFks))
        {
            sb.AppendLine();
            foreach (var fk in toFks)
            {
                string targetClass = NameHumanizer.Singularize(fk.ToTable);
                string collectionProp = NameHumanizer.Pluralize(targetClass);
                sb.AppendLine($"    public virtual ICollection<{targetClass}> {collectionProp} {{ get; set; }} = new List<{targetClass}>();");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string MapToCSharpType(string sqlType, bool isNullable)
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


        return isNullable ? type + "?" : type;
    }

    private string GenerateClassComment(string className)
    {
        return
    $@"/**
*  \File:	  {className}.cs
*  \Author:		    
*  \Date:	  {DateTime.Now.ToString("MMM/dd/yyyy")}
*  \Version:  1.0
*  \Brief:	  Model Class which mimics the corresponding table in the database.
*             Sole purpose is to hold data.
*/
    ";
    }
}
