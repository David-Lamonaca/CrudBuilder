public class ColumnInfo
{
    public ColumnInfo(
        string name,
        bool isPK,
        bool isIdx,
        bool isUK,
        int? maxLength,
        string? sqlType,
        bool isRowVer,
        bool isNullable)
    {
        Name = name;
        IsPrimaryKey = isPK;
        IsIndexed = isIdx;
        IsUnique = isUK;
        MaxLength = maxLength;
        SqlType = sqlType;
        IsRowVersion = isRowVer;
        IsNullable = isNullable;
    }

    public string Name { get; set; } = default!;
    public bool IsPrimaryKey { get; set; }
    public bool IsIndexed { get; set; }
    public bool IsUnique { get; set; }
    public int? MaxLength { get; set; }
    public string? SqlType { get; set; }
    public bool IsRowVersion { get; set; }
    public bool IsNullable { get; set; }
}
