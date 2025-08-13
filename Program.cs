using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

// Prompt for connection string
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

List<string> tables = schemaReader.GetTables(connectionString);
Dictionary<string, List<ColumnInfo>> mappedTables = new();

foreach (var table in tables)
{
    var className = NameHumanizer.Singularize(table);
    List<ColumnInfo> columns = schemaReader.GetColumns(connectionString, table);
    mappedTables.Add(table, columns);

    modelGenerator.GenerateModelClass(className, columns, Path.Combine(outputRoot, "DA_Layer/Models"));
    DaoInterfaceGenerator.WriteInterfaceToFile(className, columns, Path.Combine(outputRoot, "DA_Layer/Interfaces"));
    DaoClassGenerator.WriteDaoToFile(className, $"{databaseName}Context", Path.Combine(outputRoot, "DA_Layer/DataAccessObjects"), columns);
}

// Generate DbContext
string contextPath = Path.Combine(outputRoot, $"DA_Layer/{databaseName}Context.cs");
if (File.Exists(contextPath) == false)
{
    DbContextGenerator.GenerateContextClassToFile(databaseName, mappedTables, foreignKeys);
    Console.WriteLine($"✅ DbContext generated at: {contextPath}");
}
else
{
    Console.Write($"⚠️  DbContext file '{contextPath}' exists. Overwrite? (Y/N): ");
    string? ctxAnswer = Console.ReadLine()?.Trim().ToUpper();
    if (ctxAnswer != "Y" && ctxAnswer != "YES")
    {
        Console.WriteLine("Skipped DbContext.");
    }
    else
    {
        DbContextGenerator.GenerateContextClassToFile(databaseName, mappedTables, foreignKeys);
        Console.WriteLine($"✅ DbContext generated at: {contextPath}");
    }
}

Console.WriteLine("\n🎉 Done!");

static string ReadPassword()
{
    // Securely reads password without echoing to the console
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
