using Contracts.Data.Data.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracts.Data.Data.Maps.Base
{
    public class BaseMap<T> : IEntityTypeConfiguration<T>
        where T : BaseEntity
    {
        public BaseMap(string idColumnName = "")
        {
            IdColumnName = idColumnName;
        }

        public string IdColumnName { get; }

        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            if(!string.IsNullOrWhiteSpace(IdColumnName))
            {
                builder.Property(x => x.Id)
                   .HasColumnName(IdColumnName);
            }
               
            builder.HasIndex(x => x.Id);
        }
    }
}
