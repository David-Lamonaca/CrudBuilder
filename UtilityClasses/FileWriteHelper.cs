using System;

public static class FileWriteHelper
{
    public static void WriteFileSafely(string path, string content)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(path))
        {
            Console.Write($"⚠️ File '{path}' already exists. Overwrite? (Y/N): ");
            string? input = Console.ReadLine()?.Trim().ToUpper();
            if (input != "Y" && input != "YES")
            {
                Console.WriteLine($"🚫 Skipped: {path}");
                return;
            }
        }

        File.WriteAllText(path, content);
        Console.WriteLine($"✅ File written: {path}");
    }
}
