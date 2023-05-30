using System.Data;

namespace DBOP.Extensions;

public static class DataExtension
{
    internal static bool HasColumn(this IDataRecord dr, string columnName)
    {
        for (int i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                return true;
        }
        return false;
    }

    internal static object ConvertToType(this object value, Type type)
    {
        var tc = Type.GetTypeCode(type);
        return tc switch
        {
            TypeCode.String => IsValueNull(value) ? string.Empty : value.ToString(),
            TypeCode.DateTime => IsValueNull(value) ? default : DateTime.Parse(value.ToString()!),
            TypeCode.Int16 => IsValueNull(value) ? default : short.Parse(value.ToString()!),
            TypeCode.Int32 => IsValueNull(value) ? default : int.Parse(value.ToString()!),
            TypeCode.Int64 => IsValueNull(value) ? default : long.Parse(value.ToString()!),
            TypeCode.UInt16 => IsValueNull(value) ? default : ushort.Parse(value.ToString()!),
            TypeCode.UInt32 => IsValueNull(value) ? default : uint.Parse(value.ToString()!),
            TypeCode.UInt64 => IsValueNull(value) ? default : ulong.Parse(value.ToString()!),
            TypeCode.Boolean => !IsValueNull(value) && bool.Parse(value.ToString()!),
            _ => IsValueNull(value) ? default : value
        } ?? value;
    }

    internal static bool IsValueNull(object value) => value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString());
}
