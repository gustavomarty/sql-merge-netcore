using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Models
{
    internal class MergeBuilderSqlConfiguration
    {
        public string TableName { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
        public List<string> AllColumns { get; set; } = new();
        public List<string> MergedColumns { get; set; } = new();
        public List<string> UpdatedColumns { get; set; } = new();
        public List<string> InsertedColumns { get; set; } = new();
        public List<ConditionBuilder> Conditions { get; set; } = new();
        public string StatusColumn { get; set; } = string.Empty;
        public bool UseEnumStatus { get; set; }
        public ResponseType ResponseType { get; set; }
    }
}
