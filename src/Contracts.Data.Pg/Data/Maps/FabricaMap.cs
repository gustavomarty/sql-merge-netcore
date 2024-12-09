using Contracts.Data.Data.Entities;
using Contracts.Data.Data.Maps.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracts.Data.Data.Maps
{
    public class FabricaMap : BaseMap<Fabrica>
    {
        public override void Configure(EntityTypeBuilder<Fabrica> builder)
        {
            builder.ToTable(nameof(Fabrica), "test");

            base.Configure(builder);
        }
    }
}
