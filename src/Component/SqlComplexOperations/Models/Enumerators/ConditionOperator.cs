using System.ComponentModel;

namespace SqlComplexOperations.Models.Enumerators
{
    /// <summary>
    /// Operadores para a condition (&& ou ||)
    /// </summary>
    public enum ConditionOperator
    {
        [Description("and")]
        AND,

        [Description("or")]
        OR,

        NONE
    }
}
