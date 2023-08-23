using SqlComplexOperations.Attributes;

namespace SqlComplexOperations.Tests.Models
{
    public class PersonEntityPropName
    {
        [PropertyName("id")]
        public int Id { get; set; }

        [PropertyName("nome")]
        public string Name { get; set; }

        [PropertyName("cpf")]
        public string Document { get; set; }

        [PropertyName("dta_aniversario")]
        public DateTime BirthDate { get; set; }

        [PropertyName("dta_atualizacao")]
        public DateTime UpdatedDate { get; set; }
    }
}
