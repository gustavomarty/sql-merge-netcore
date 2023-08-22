using System.ComponentModel;

namespace SqlComplexOperations.Models.Enumerators
{
    public enum OutputAction
    {
        [Description("UPDATE")]
        UPDATE,

        [Description("DELETE")]
        DELETE,

        [Description("INSERT")]
        INSERT
    }
}
