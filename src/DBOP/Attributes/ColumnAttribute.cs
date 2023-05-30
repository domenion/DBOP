namespace DBOP.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnNameAttribute : Attribute
{
    public ColumnNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}