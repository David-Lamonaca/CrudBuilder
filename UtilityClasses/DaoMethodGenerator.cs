using System.Text;

public static class DaoMethodGenerator
{
    public static string GenerateFkIndexInterfaceMethods(string className, List<ColumnInfo> columns)
    {
        var sb = new StringBuilder();

        foreach (ColumnInfo col in columns)
        {
            if (col.IsPrimaryKey || (!col.IsIndexed && !col.IsUnique)) continue;

            string methodName = $"Get{className}By{col.Name}";
            string clrType = SqlTypeMapper.MapToCSharpType(col.SqlType ?? "", col.IsNullable);
            string paramName = CamelCase(col.Name);

            sb.AppendLine($"    Task<PagedResult<{className}>> {methodName}({clrType} {paramName}, int? page = null, int? pageSize = null);");
        }

        return sb.ToString();
    }

    public static string GenerateFkIndexDaoMethods(string className, List<ColumnInfo> columns)
    {
        var sb = new StringBuilder();

        foreach (ColumnInfo col in columns)
        {
            if (col.IsPrimaryKey || (!col.IsIndexed && !col.IsUnique)) continue;

            string methodName = $"Get{className}By{col.Name}";
            string clrType = SqlTypeMapper.MapToCSharpType(col.SqlType ?? "", col.IsNullable);
            string paramName = CamelCase(col.Name);

            sb.AppendLine($"    public async Task<PagedResult<{className}>> {methodName}({clrType} {paramName}, int? page = null, int? pageSize = null)");
            sb.AppendLine("    {");
            sb.AppendLine("        using var ctx = _factory.CreateDbContext();");
            sb.AppendLine($"        var query = ctx.Set<{className}>().Where(e => e.{col.Name} == {paramName});");
            sb.AppendLine();
            sb.AppendLine("        int totalCount = await query.CountAsync();");
            sb.AppendLine($"        List<{className}> items;");
            sb.AppendLine();
            sb.AppendLine("        if (page != null && pageSize != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            items = await query");
            sb.AppendLine("                .Skip(((int)page - 1) * (int)pageSize)");
            sb.AppendLine("                .Take((int)pageSize)");
            sb.AppendLine("                .ToListAsync();");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            sb.AppendLine("            items = await query.ToListAsync();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        return new PagedResult<{className}>(items, totalCount, page, pageSize);");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string CamelCase(string input)
        => char.ToLowerInvariant(input[0]) + input.Substring(1);
}
