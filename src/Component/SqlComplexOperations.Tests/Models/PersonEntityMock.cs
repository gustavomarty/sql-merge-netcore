using Bogus;
using Bogus.Extensions.Brazil;
using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Tests.Models
{
    public static class PersonEntityMock
    {
        public static PersonEntity Get()
        {
            return Get(1).First();
        }

        public static PersonEntityStatus GetWithStatus()
        {
            return GetWithStatus(1).First();
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

        public static List<PersonEntityStatus> GetWithStatus(int quantity)
        {
            var faker = new Faker<PersonEntityStatus>("pt_BR")
                .RuleFor(x => x.Id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FullName)
                .RuleFor(x => x.BirthDate, f => f.Person.DateOfBirth)
                .RuleFor(x => x.Document, f => f.Person.Cpf(false))
                .RuleFor(x => x.UpdatedDate, DateTime.Now)
                .RuleFor(x => x.Status, BulkMergeStatus.PROCESSED)
                .RuleFor(x => x.StatusStr, "PROCESSED");

            return faker.Generate(quantity);
        }
    }
}
