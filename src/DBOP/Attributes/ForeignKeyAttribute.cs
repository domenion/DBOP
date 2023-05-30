namespace DBOP.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ForeignKeyAttribute : Attribute
{
    public ForeignKeyAttribute(string key)
    {
        KeyName = key;
    }

    public string KeyName { get; }
}
