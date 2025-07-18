using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using Microsoft.EntityFrameworkCore;

public static class DbContextGenerator
{
    public static void GenerateContextClassToFile(
        string dbName,
        Dictionary<string, List<ColumnInfo>> tables,
        List<ForeignKeyInfo> foreignKeys)
    {
        string contextCode = GenerateContextClass(dbName, tables, foreignKeys);

        string outputDir = Path.Combine("Output", dbName);
        Directory.CreateDirectory(outputDir);

        string outputPath = Path.Combine(outputDir, $"DA_Layer/{dbName}Context.cs");
        File.WriteAllText(outputPath, contextCode);
    }

    private static string GenerateContextClass(
        string dbName,
        Dictionary<string, List<ColumnInfo>> tables,
        List<ForeignKeyInfo> foreignKeys)
    {
        var sb = new StringBuilder();
        var contextName = $"{dbName}Context";

        sb.AppendLine(ClassCommentGenerator.Generate(contextName, @"Used to Query/Save data From/To the database, configure domain classes(Models Folder), 
*              database related mappings, change tracking settings, caching, transaction, etc."));
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine();
        sb.AppendLine($"public partial class {contextName} : DbContext");
        sb.AppendLine("{");
        sb.AppendLine($"    public {contextName}(DbContextOptions<{contextName}> options)");
        sb.AppendLine("        : base(options) { }");
        sb.AppendLine();

        // DbSet<T> properties
        foreach (var table in tables)
        {
            var className = NameHumanizer.Singularize(table.Key);
            var plural = NameHumanizer.Pluralize(className);
            sb.AppendLine($"    public virtual DbSet<{className}> {plural} {{ get; set; }}");
        }

        sb.AppendLine();
        sb.AppendLine("    protected override void OnModelCreating(ModelBuilder modelBuilder)");
        sb.AppendLine("    {");

        foreach (var (tableName, columns) in tables)
        {
            sb.AppendLine(GenerateFluentMapping(tableName, columns, foreignKeys));
        }

        sb.AppendLine("        OnModelCreatingPartial(modelBuilder);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateFluentMapping(
        string tableName,
        List<ColumnInfo> columns,
        List<ForeignKeyInfo> foreignKeys)
    {
        var sb = new StringBuilder();
        var className = NameHumanizer.Singularize(tableName);

        sb.AppendLine($"        modelBuilder.Entity<{className}>(entity =>");
        sb.AppendLine("        {");

        // ToTable
        sb.AppendLine($"            entity.ToTable(\"{tableName}\");");
        sb.AppendLine();

        // Primary key
        var pkColumns = columns.Where(c => c.IsPrimaryKey).ToList();
        if (pkColumns.Count == 1)
        {
            sb.AppendLine($"            entity.HasKey(e => e.{pkColumns[0].Name});");
        }
        else if (pkColumns.Count > 1)
        {
            var keys = string.Join(", ", pkColumns.Select(c => $"e.{c.Name}"));
            sb.AppendLine($"            entity.HasKey(e => new {{ {keys} }});");
        }
        sb.AppendLine();

        // Properties
        foreach (var col in columns)
        {
            sb.Append($"            entity.Property(e => e.{col.Name})");

            if (col.MaxLength.HasValue && col.MaxLength.Value > 0)
                sb.Append($".HasMaxLength({col.MaxLength.Value})");

            if (!string.IsNullOrEmpty(col.SqlType))
                sb.Append($".HasColumnType(\"{col.SqlType}\")");

            sb.AppendLine(";");
        }
        sb.AppendLine();

        // Indexes and uniqueness
        foreach (var col in columns)
        {
            if (col.IsIndexed || col.IsUnique)
            {
                sb.Append($"            entity.HasIndex(e => e.{col.Name})");
                if (col.IsUnique)
                    sb.Append(".IsUnique()");
                sb.AppendLine(";");
            }
        }
        if (columns.Any(c => c.IsIndexed || c.IsUnique))
            sb.AppendLine();

        // Foreign keys
        var fks = foreignKeys.Where(fk => fk.FromTable == tableName).ToList();
        foreach (var fk in fks)
        {
            var navProp = NameHumanizer.Singularize(fk.ToTable);
            var collectionNav = NameHumanizer.Pluralize(NameHumanizer.Singularize(fk.FromTable));

            sb.AppendLine($"            entity.HasOne(d => d.{navProp})");
            sb.AppendLine($"                .WithMany(p => p.{collectionNav})");
            sb.AppendLine($"                .HasForeignKey(d => d.{fk.FromColumn})");
            sb.AppendLine($"                .HasConstraintName(\"FK_{fk.FromTable}_{fk.ToTable}_{fk.ToColumn}\");");
            sb.AppendLine();
        }

        sb.AppendLine("        });");
        sb.AppendLine();

        return sb.ToString();
    }
}
