public static class NameHumanizer
{
    public static string Singularize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        if (name.EndsWith("ies"))
            return name[..^3] + "y"; // e.g., Categories → Category
        if (name.EndsWith("ses") || name.EndsWith("xes") || name.EndsWith("zes"))
            return name[..^2];      // e.g., Addresses → Address
        if (name.EndsWith("s") && !name.EndsWith("ss"))
            return name[..^1];      // e.g., Users → User
        return name;
    }

    public static string Pluralize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        if (name.EndsWith("y") && !IsVowel(name[^2])) // Category → Categories
            return name[..^1] + "ies";
        if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("z"))
            return name + "es";                       // Address → Addresses
        return name + "s";                            // User → Users
    }

    private static bool IsVowel(char c)
        => "aeiouAEIOU".IndexOf(c) >= 0;
}
