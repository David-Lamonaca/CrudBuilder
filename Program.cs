using Microsoft.Data.SqlClient;

var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=GameOracle;Integrated Security=True;TrustServerCertificate=True";
string databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
string outputRoot = Path.Combine("Output", databaseName);

IDatabaseSchemaReader schemaReader = new SqlServerSchemaReader();
ModelClassGenerator modelGenerator = new ModelClassGenerator(schemaReader.GetForeignKeys(connectionString));

foreach (var table in schemaReader.GetTables(connectionString))
{
    var columns = schemaReader.GetColumns(connectionString, table);

    var classCode = modelGenerator.GenerateModelClass(NameHumanizer.Singularize(table), columns);

    var outputPath = Path.Combine(outputRoot, $"{NameHumanizer.Singularize(table)}.cs");
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    File.WriteAllText(outputPath, classCode);

    Console.WriteLine($"Generated model for table '{NameHumanizer.Singularize(table)}' at: {outputPath}");
}
