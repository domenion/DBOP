using System.Collections;

namespace DBOP.Models
{
    public class WhereCondition
    {
        public string Field { get; set; } = null!;
        public string Operator { get; set; } = "=";
        public object Value { get; set; } = null!;
        public string Aliases { get; set; } = "";
        public string Prefix { get; set; } = "";

        public string CreateCondition()
        {
            if (IsList(Value))
            {
                if (Operator == "=")
                {
                    Operator = "IN";
                }
                if (Operator == "!=")
                {
                    Operator = "NOT IN";
                }
            }
            return $"{Aliases}.{Field}{Operator}{Prefix}{Field}";
        }

        private bool IsList(object o)
        {
            if (o == null)
            {
                return false;
            }

            return o is IList
                && o.GetType().IsGenericType
                && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
    }
}