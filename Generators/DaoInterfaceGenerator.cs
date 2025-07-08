using System.Text;

public static class DaoInterfaceGenerator
{
    public static string GenerateInterface(string className, List<ColumnInfo> columns)
    {
        var now = DateTime.Now.ToString("yyyy-MM-dd");
        var sb = new StringBuilder();

        sb.AppendLine("﻿/*");
        sb.AppendLine($" *  \\File:\t\tI{className}DAO.cs");
        sb.AppendLine(" *  \\Author:\t\t");
        sb.AppendLine($" *  \\Date:\t\t{now}");
        sb.AppendLine(" *  \\Version:\t1.0");
        sb.AppendLine(" *  \\Brief:\t\tInterface created for the corresponding DAO class, declares the methods the DAO class uses.");
        sb.AppendLine(" *\t\t\t\tUsed in the dependency injection system (RegisterServices), allowing us to easily swap implementations.");
        sb.AppendLine(" */");
        sb.AppendLine();
        sb.AppendLine($"public interface I{className}DAO");
        sb.AppendLine("{");
        sb.AppendLine($"    Task<int> Add({className} newEntity);");
        sb.AppendLine($"    Task<int> Delete({className} deleteEntity);");
        sb.AppendLine($"    Task<{className}?> GetById(int id);");
        sb.AppendLine($"    Task<int> Update({className} updatedEntity);");
        sb.AppendLine($"    Task<ICollection<{className}>> GetAll(int? page = null, int? pageSize = null);");

        foreach (ColumnInfo col in columns)
        {
            if (col.IsPrimaryKey || (!col.IsIndexed && !col.IsUnique)) continue;

            string methodName = $"Get{className}By{col.Name}";
            string clrType = SqlTypeMapper.MapToCSharpType(col.SqlType , col.IsNullable);

            sb.AppendLine($"    Task<ICollection<{className}>> {methodName}({clrType} {CamelCase(col.Name)}, int? page = null, int? pageSize = null);");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static void WriteInterfaceToFile(string className, List<ColumnInfo> columns, string outputDirectory)
    {
        string code = GenerateInterface(className, columns);
        string path = Path.Combine(outputDirectory, $"I{className}DAO.cs");

        if (File.Exists(path))
        {
            Console.Write($"File '{path}' exists. Overwrite? (y/n): ");
            var input = Console.ReadLine()?.Trim().ToLower();
            if (input != "y") return;
        }

        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(path, code);
        Console.WriteLine($"✔ Interface written: {path}");
    }

    private static string CamelCase(string input)
        => char.ToLowerInvariant(input[0]) + input.Substring(1);
}
