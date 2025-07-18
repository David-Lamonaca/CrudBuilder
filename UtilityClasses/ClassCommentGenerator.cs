public static class ClassCommentGenerator
{
    public static string Generate(string className, string brief)
    {
        return
$@"/**
*  \File:     {className}.cs
*  \Author:  		
*  \Date:     {DateTime.Now:MMM/dd/yyyy}
*  \Version:  1.0
*  \Brief:    {brief}
*/
";
    }
}
