using System.Reflection;
using System.Text.Json.Serialization;
using DBOP.Attributes;

namespace DBOP.Extensions;

public static class PropertyInfoExtension
{
    public static string GetColumnName(this PropertyInfo property)
    {
        var columnAttr = property.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
        var name = columnAttr?.Name ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            var jsonAttr = property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false).FirstOrDefault() as JsonPropertyNameAttribute;
            name = jsonAttr?.Name ?? "";
            name = string.IsNullOrWhiteSpace(name) ? property.Name : name;
        }
        return name;
    }
}