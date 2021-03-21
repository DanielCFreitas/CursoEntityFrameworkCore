using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCore.Configurations
{
    public class EstadoConfiguration : IEntityTypeConfiguration<Estado>
    {
        public void Configure(EntityTypeBuilder<Estado> builder)
        {
            builder
                .HasOne(p => p.Governador)
                .WithOne(p => p.Estado)
                .HasForeignKey<Governador>(p => p.EstadoId);

            // Faz o include automaticamente do governador sempre que consultar um estado
            builder.Navigation(p => p.Governador).AutoInclude();

            builder
                .HasMany(p => p.Cidades)
                .WithOne(p => p.Estado)
                .IsRequired(false);
                // .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
