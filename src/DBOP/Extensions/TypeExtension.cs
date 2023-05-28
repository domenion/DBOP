using System.Reflection;
using System.Text.Json.Serialization;
using DBOP.Attributes;

namespace DBOP.Extensions;
public static class TypeExtension
{
    public static string GetTableName(this Type type)
    {
        var columnAttr = type.GetCustomAttributes(typeof(TableNameAttribute), false).FirstOrDefault() as TableNameAttribute;
        var name = columnAttr?.Name ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            var jsonAttr = type.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false).FirstOrDefault() as JsonPropertyNameAttribute;
            name = jsonAttr?.Name ?? "";
            name = string.IsNullOrWhiteSpace(name) ? type.Name : name;
        }
        return name;
    }

    public static PropertyInfo? GetPrimaryKeyProperty(this Type type)
    {
        var properties = type.GetProperties();
        return Array.Find(properties, p => p.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length > 0);
    }
}
