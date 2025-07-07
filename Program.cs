using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

// Prompt for connection string
// "Server=(localdb)\MSSQLLocalDB;Database=Contractor;Integrated Security=True;TrustServerCertificate=True";
Console.WriteLine("Enter Connection String:");
string? inputConnStr = Console.ReadLine();

while (string.IsNullOrWhiteSpace(inputConnStr))
{
    Console.WriteLine("Connection string is required.\n");
    inputConnStr = Console.ReadLine();
}

// Check if username/password are required
var builder = new SqlConnectionStringBuilder(inputConnStr);
if (!builder.IntegratedSecurity)
{
    Console.Write("Do you need to provide a username and password? (Y/N): ");
    string? answer = Console.ReadLine()?.Trim().ToUpper();

    if (answer == "Y" || answer == "YES")
    {
        if (string.IsNullOrWhiteSpace(builder.UserID))
        {
            Console.Write("Username: ");
            builder.UserID = Console.ReadLine();
        }

        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            Console.Write("Password: ");
            builder.Password = ReadPassword();
        }
    }
}

// Try connecting to the database safely
string connectionString = builder.ToString();
try
{
    using var testConn = new SqlConnection(connectionString);
    testConn.Open();
}
catch (SqlException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n❌ Failed to connect to the database:");
    Console.WriteLine(ex.Message);
    Console.ResetColor();
    return;
}

string databaseName = builder.InitialCatalog;
string outputRoot = Path.Combine("Output", databaseName);

IDatabaseSchemaReader schemaReader = new SqlServerSchemaReader();
List<ForeignKeyInfo> foreignKeys = schemaReader.GetForeignKeys(connectionString);
ModelClassGenerator modelGenerator = new ModelClassGenerator(foreignKeys);

Dictionary<string, List<ColumnInfo>> mappedTables = new();

foreach (var table in schemaReader.GetTables(connectionString))
{
    var className = NameHumanizer.Singularize(table);
    List<ColumnInfo> columns = schemaReader.GetColumns(connectionString, table);
    mappedTables.Add(table, columns);

    string classCode = modelGenerator.GenerateModelClass(className, columns);
    string outputPath = Path.Combine(outputRoot, $"Models/{className}.cs");

    // Check for overwrite
    if (File.Exists(outputPath))
    {
        Console.Write($"⚠️  File '{outputPath}' exists. Overwrite? (Y/N): ");
        string? answer = Console.ReadLine()?.Trim().ToUpper();
        if (answer != "Y" && answer != "YES")
        {
            Console.WriteLine("Skipped.");
            continue;
        }
    }

    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    File.WriteAllText(outputPath, classCode);
    Console.WriteLine($"✅ Generated model for table '{className}' at: {outputPath}");
}

// Generate DbContext
string contextPath = Path.Combine(outputRoot, $"{databaseName}Context.cs");

if (File.Exists(contextPath))
{
    Console.Write($"⚠️  DbContext file '{contextPath}' exists. Overwrite? (Y/N): ");
    string? ctxAnswer = Console.ReadLine()?.Trim().ToUpper();
    if (ctxAnswer != "Y" && ctxAnswer != "YES")
    {
        Console.WriteLine("Skipped DbContext.");
        return;
    }
}

DbContextGenerator.GenerateContextClassToFile(databaseName, mappedTables, foreignKeys);
Console.WriteLine($"✅ DbContext generated at: {contextPath}");

Console.WriteLine("\n🎉 Done!");


// Securely reads password without echoing to the console
static string ReadPassword()
{
    var pwd = new Stack<char>();
    ConsoleKeyInfo key;
    while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace && pwd.Count > 0)
        {
            pwd.Pop();
            Console.Write("\b \b");
        }
        else if (!char.IsControl(key.KeyChar))
        {
            pwd.Push(key.KeyChar);
            Console.Write("*");
        }
    }
    Console.WriteLine();
    return new string(pwd.Reverse().ToArray());
}
