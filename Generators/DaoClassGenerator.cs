using System.Text;

public static class DaoClassGenerator
{
    private static string GenerateDaoClass(string className, string contextName,  List<ColumnInfo> columns)
    {
        var sb = new StringBuilder();

        // Comment header
        sb.AppendLine(ClassCommentGenerator.Generate(
            $"{className}DAO.cs",
            $"Implementation of our DAO interface. The DAO class interacts with {contextName} to perform database commands."));

        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine($"public class {className}DAO : I{className}DAO");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IDbContextFactory<{contextName}> _factory;");
        sb.AppendLine();
        sb.AppendLine($"    public {className}DAO(IDbContextFactory<{contextName}> factory)");
        sb.AppendLine("    {");
        sb.AppendLine("        _factory = factory;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetById
        sb.AppendLine($"    public async Task<{className}> GetById(int id)");
        sb.AppendLine("    {");
        sb.AppendLine("        using var ctx = _factory.CreateDbContext();");
        sb.AppendLine($"        {className} entity = await ctx.Set<{className}>()");
        sb.AppendLine("            .FirstOrDefaultAsync(e => e.Id == id);");
        sb.AppendLine();
        sb.AppendLine("        if (entity == null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            throw new KeyNotFoundException($\"No {className} with ID {{id}} found.\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return entity;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetAll (Paged)
        sb.AppendLine($"    public async Task<PagedResult<{className}>> GetAll(int? page = null, int? pageSize = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        using var ctx = _factory.CreateDbContext();");
        sb.AppendLine($"        var query = ctx.Set<{className}>().AsQueryable();");
        sb.AppendLine();
        sb.AppendLine("        int totalCount = await query.CountAsync();");
        sb.AppendLine("        List<" + className + "> items;");
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
        sb.AppendLine("        return new PagedResult<" + className + ">(items, totalCount, page, pageSize);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.Append(DaoMethodGenerator.GenerateFkIndexDaoMethods(className, columns));

        // Add
        sb.AppendLine($"    public async Task<int> Add({className} newEntity)");
        sb.AppendLine("    {");
        sb.AppendLine("        using var ctx = _factory.CreateDbContext();");
        sb.AppendLine($"        ctx.Set<{className}>().Add(newEntity);");
        sb.AppendLine("        await ctx.SaveChangesAsync();");
        sb.AppendLine();
        sb.AppendLine("        return newEntity.Id;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Update with optional concurrency logic
        sb.AppendLine($"    public async Task<int> Update({className} updatedEntity)");
        sb.AppendLine("    {");
        sb.AppendLine("        int updateStatus = -1;");
        sb.AppendLine("        using var ctx = _factory.CreateDbContext();");
        sb.AppendLine($"        var currentEntity = await ctx.Set<{className}>().FirstOrDefaultAsync(e => e.Id == updatedEntity.Id);");
        sb.AppendLine();
        sb.AppendLine("        if (currentEntity == null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            throw new KeyNotFoundException($\"No {className} with ID {{updatedEntity.Id}} found.\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // Uncomment the next line to enable concurrency checking:");
        sb.AppendLine($"        // ctx.Entry(currentEntity).OriginalValues[\"LastModified\"] = updatedEntity.LastModified;");
        sb.AppendLine();
        sb.AppendLine("        ctx.Entry(currentEntity).CurrentValues.SetValues(updatedEntity);");
        sb.AppendLine("        if (await ctx.SaveChangesAsync() == 1)");
        sb.AppendLine("        {");
        sb.AppendLine("            updateStatus = 1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return updateStatus;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Delete method
        sb.AppendLine($"    public async Task<int> Delete(int id)");
        sb.AppendLine("    {");
        sb.AppendLine("        using var ctx = _factory.CreateDbContext();");
        sb.AppendLine($"        var entity = await ctx.Set<{className}>().FirstOrDefaultAsync(e => e.Id == id);");
        sb.AppendLine("        if (entity == null)");
        sb.AppendLine("        {");
        sb.AppendLine("            return 0; // Not found");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        ctx.Set<{className}>().Remove(entity);");
        sb.AppendLine("        return await ctx.SaveChangesAsync();");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }

    public static void WriteDaoToFile(string className, string contextName, string outputDirectory, List<ColumnInfo> columns)
    {
        string code = GenerateDaoClass(className, contextName, columns);
        string path = Path.Combine(outputDirectory, $"{className}DAO.cs");
        FileWriteHelper.WriteFileSafely(path, code);
    }
}
