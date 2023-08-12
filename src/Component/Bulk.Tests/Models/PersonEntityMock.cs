using Bogus;
using Bogus.Extensions.Brazil;

namespace Bulk.Tests.Models
{
    public static class PersonEntityMock
    {
        public static PersonEntity Get()
        {
            return Get(1).First();
        }

        public static List<PersonEntity> Get(int quantity)
        {
            var faker = new Faker<PersonEntity>("pt_BR")
                .RuleFor(x => x.Id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FullName)
                .RuleFor(x => x.BirthDate, f => f.Person.DateOfBirth)
                .RuleFor(x => x.Document, f => f.Person.Cpf(false))
                .RuleFor(x => x.UpdatedDate, DateTime.Now);

            return faker.Generate(quantity);
        }
    }
}
