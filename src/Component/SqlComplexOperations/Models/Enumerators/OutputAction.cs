using System.ComponentModel;

namespace SqlComplexOperations.Models.Enumerators
{
    /// <summary>
    /// Define qual a ação do resultado.
    /// </summary>
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
