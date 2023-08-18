using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Tests.Models
{
    public class PersonEntityStatus : PersonEntity
    {
        public BulkMergeStatus Status { get; set; }
        public string StatusStr { get; set; }
    }
}
