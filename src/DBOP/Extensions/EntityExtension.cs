using System.Collections;
using System.Data.Common;
using System.Reflection;
using DBOP.Attributes;
using DBOP.Interfaces;

namespace DBOP.Extensions;

public static class EntityExtension
{
    public static T GetEntityData<T>(this T entity, DbDataReader dr)
        where T : IEntityBase
    {
        foreach (var info in entity.GetType().GetProperties())
        {
            var colName = info.GetColumnName();
            if (Attribute.IsDefined(info, typeof(RelationshipAttribute), true))
            {
                if (info.PropertyType.IsGenericType)
                {
                    var row = Activator.CreateInstance(info.PropertyType.GetGenericArguments()[0]);
                    (row as IEntityBase)?.GetEntityData(dr);
                    var genericType = info.PropertyType.GetGenericArguments().First();
                    var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType)) as IList;
                    listInstance?.Add(row);
                    info.SetValue(entity, listInstance, null);
                }
                else
                {
                    var related = Activator.CreateInstance(info.PropertyType) as IEntityBase;
                    related?.GetEntityData(dr);
                    info.SetValue(entity, related);
                }
            }
            else
            {
                if (!dr.HasColumn(colName))
                {
                    continue;
                }

                var colValue = dr[colName];
                info.SetValue(entity, colValue.ConvertToType(info.PropertyType), null);
            }
        }
        return entity;
    }

    public static bool CompareKey<T>(this T e1, T e2)
        where T : IEntityBase
    {
        var k1 = GetKeyValue(e1);
        var k2 = GetKeyValue(e2);
        return k1?.ToString()?.Equals(k2?.ToString()) == true;
    }

    public static bool CompareExcludeRelationship<T>(this T e1, T e2)
        where T : IEntityBase
    {
        foreach (var info1 in GetPropertiesExcludeRelationship(e1))
        {
            var info2 = e2.GetType().GetProperty(info1.Name);
            var value1 = info1.GetValue(e1);
            var value2 = info2?.GetValue(e2);
            if (value1?.Equals(value2) == false)
            {
                return false;
            }
        }

        return true;
    }

    public static object? GetKeyValue<T>(this T x)
        where T : IEntityBase
    {
        var infoKey = GetKeyProperty(x);
        return infoKey?.GetValue(x);
    }

    public static PropertyInfo? GetKeyProperty<T>(this T e)
        where T : IEntityBase
    {
        return Array.Find(e.GetType().GetProperties(), x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute), true));
    }

    public static IEnumerable<PropertyInfo> GetRelationshipProperties<T>(this T e)
        where T : IEntityBase
    {
        if (e == null)
        {
            return new List<PropertyInfo>();
        }

        return e.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(RelationshipAttribute), true));
    }

    public static IEnumerable<PropertyInfo> GetPropertiesExcludeRelationship<T>(T e)
    {
        if (e == null)
        {
            return new List<PropertyInfo>();
        }

        return e.GetType().GetProperties().Where(info => !Attribute.IsDefined(info, typeof(RelationshipAttribute), true));
    }
}