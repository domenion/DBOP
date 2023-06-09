namespace DBOP.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TableAttribute : Attribute
{
    public TableAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}