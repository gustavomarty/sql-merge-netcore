using System.ComponentModel;

namespace SqlComplexOperations.Models.Enumerators
{
    /// <summary>
    /// Status do BulkMerge (UpSert operation).
    /// </summary>
    public enum BulkMergeStatus
    {
        /// <summary>
        /// Quando um registro já foi processado.
        /// <br></br>
        /// Se um registro não sofreu atualização, ele ficará tambem nesse status.
        /// </summary>
        [Description("PROCESSED")]
        PROCESSED = 0,

        /// <summary>
        /// Quando um registro sofre uma atualização.
        /// </summary>
        [Description("UPDATED")]
        UPDATED = 1,

        /// <summary>
        /// Quando o registro precisa ser inserido.
        /// </summary>
        [Description("INSERTED")]
        INSERTED = 2
    }
}
