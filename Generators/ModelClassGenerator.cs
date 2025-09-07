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

    public void GenerateModelClass(string className, string tableName, List<ColumnInfo> columns, string outputDirectory)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ClassCommentGenerator.Generate(className, @"Model Class which mimics the corresponding table in the database.
*             Sole purpose is to hold data."));
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        // Properties for table columns
        foreach (var col in columns)
        {
            var type = SqlTypeMapper.MapToCSharpType(col.SqlType ?? "", col.IsNullable);

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
                string targetClass = NameHumanizer.Singularize(fk.FromTable);
                string collectionProp = NameHumanizer.Pluralize(targetClass);
                sb.AppendLine($"    public virtual ICollection<{targetClass}> {collectionProp} {{ get; set; }} = new List<{targetClass}>();");
            }
        }

        sb.AppendLine("}");

        string path = Path.Combine(outputDirectory, $"{tableName}.cs");
        FileWriteHelper.WriteFileSafely(path, sb.ToString());
    }
}
