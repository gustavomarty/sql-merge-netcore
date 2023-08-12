using System.ComponentModel;

namespace Bulk.Models.Enumerators
{
    public enum ConditionTypes
    {
        [Description("=")]
        EQUALS,

        [Description("!=")]
        NOT_EQUAL,

        //LESS, 
        //GREATER, 
        //LESS_OR,
    }
}
