using System.Text;

public static class DaoInterfaceGenerator
{
    private static string GenerateInterface(string className, List<ColumnInfo> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine(ClassCommentGenerator.Generate($"I{className}DAO", @"Interface created for the corresponding DAO class, declares the methods the DAO class uses.
*           Used in the dependency injection system (RegisterServices), allowing us to easily swap implementations."));
        sb.AppendLine($"public interface I{className}DAO");
        sb.AppendLine("{");
        sb.AppendLine($"    Task<int> Add({className} newEntity);");
        sb.AppendLine($"    Task<int> Delete(int id);");
        sb.AppendLine($"    Task<{className}?> GetById(int id);");
        sb.AppendLine($"    Task<int> Update({className} updatedEntity);");
        sb.AppendLine($"    Task<PagedResult<{className}>> GetAll(int? page = null, int? pageSize = null);");

        sb.Append(DaoMethodGenerator.GenerateFkIndexInterfaceMethods(className, columns));

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static void WriteInterfaceToFile(string className, List<ColumnInfo> columns, string outputDirectory)
    {
        string code = GenerateInterface(className, columns);
        string path = Path.Combine(outputDirectory, $"I{className}DAO.cs");
        FileWriteHelper.WriteFileSafely(path, code);
    }

    private static string CamelCase(string input)
        => char.ToLowerInvariant(input[0]) + input.Substring(1);
}
