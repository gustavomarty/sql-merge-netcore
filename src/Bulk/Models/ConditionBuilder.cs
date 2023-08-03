using Bulk.Models.Enumerators;

namespace Bulk.Models
{
    public class ConditionBuilder
    {

        public ConditionBuilder(List<string> fields, ConditionTypes conditionType, ConditionOperator conditionOperator)
        {
            Fields = fields;
            ConditionType = conditionType;
            ConditionOperator = conditionOperator;
        }

        public List<string> Fields { get; set; }
        public ConditionTypes ConditionType { get; set; }
        public ConditionOperator ConditionOperator { get; set; }
    }
}
