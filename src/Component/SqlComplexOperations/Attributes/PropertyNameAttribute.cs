namespace SqlComplexOperations.Attributes
{
    /// <summary>
    /// Atributo utilizado quando os nomes de coluna de banco estão diferentes dos nomes das propriedades.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyNameAttribute : Attribute
    {
        public PropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Nome da coluna do db.
        /// </summary>
        public string PropertyName { get; }
    }
}
