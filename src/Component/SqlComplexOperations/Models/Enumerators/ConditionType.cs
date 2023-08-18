using System.ComponentModel;

namespace SqlComplexOperations.Models.Enumerators
{
    /// <summary>
    /// Tipo da condição (== | != | > | < | etc...).
    /// </summary>
    public enum ConditionType
    {
        [Description("=")]
        EQUALS,

        [Description("!=")]
        NOT_EQUAL,

        [Description(">")]
        GREATER,

        [Description("<")]
        LESS,

        [Description(">=")]
        GREATER_EQUAL,

        [Description("<=")]
        LESS_EQUAL
    }
}
