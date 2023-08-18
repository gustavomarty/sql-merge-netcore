using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Models
{
    internal class ConditionBuilder
    {
        public ConditionBuilder(List<string> fields, ConditionType conditionType, ConditionOperator conditionOperator)
        {
            Fields = fields;
            ConditionType = conditionType;
            ConditionOperator = conditionOperator;
        }

        public List<string> Fields { get; set; }
        public ConditionType ConditionType { get; set; }
        public ConditionOperator ConditionOperator { get; set; }
    }
}