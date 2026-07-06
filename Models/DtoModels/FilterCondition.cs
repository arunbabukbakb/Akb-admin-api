using System;

namespace Models.DtoModels
{
    public class FilterCondition
    {
        public string Operator { get; set; } = null!; // "eq", "neq", "gt", "gte", "lt", "lte", "contains", "startswith"
        public string Value { get; set; } = null!;
    }
}
